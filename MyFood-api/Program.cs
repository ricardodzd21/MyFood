using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MyFood_api;
using MyFood_api.Data;
using MyFood_api.Services;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// JWT
var jwtSecret = builder.Configuration["JwtSecret"] ?? "MyFood-SecretKey-2024-MinLength32Characters!";
var key = Encoding.ASCII.GetBytes(jwtSecret);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorization();

// EF + PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

// CORS + JSON
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:5173", "http://localhost:3004" };
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod());
});
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = null;
});

builder.Services.AddHttpClient();
builder.Services.AddSingleton<GeminiService>();

var app = builder.Build();

app.UseCors();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

// Migrate + seed admin
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    var adminEmail = builder.Configuration["Admin:Email"] ?? "ricardodzd21@gmail.com";
    var adminPassword = builder.Configuration["Admin:Password"] ?? "200461";
    if (!db.Users.Any(u => u.Email == adminEmail))
    {
        db.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Name = "Admin",
            Email = adminEmail,
            Password = BCrypt.Net.BCrypt.HashPassword(adminPassword),
            IsAdmin = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        db.SaveChanges();
    }
}

// ==================== Helpers ====================
string GenerateToken(User user)
{
    var tokenHandler = new JwtSecurityTokenHandler();
    var descriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("is_admin", user.IsAdmin.ToString())
        }),
        Expires = DateTime.UtcNow.AddDays(30),
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
    };
    return tokenHandler.WriteToken(tokenHandler.CreateToken(descriptor));
}

Guid? GetUserId(ClaimsPrincipal principal)
{
    var id = principal.FindFirstValue(ClaimTypes.NameIdentifier);
    return Guid.TryParse(id, out var g) ? g : null;
}

ItemResponse MapItem(Item i) => new()
{
    Id = i.Id,
    Name = i.Name,
    CategoryId = i.CategoryId,
    CategoryName = i.Category?.Name ?? "",
    CategoryIcon = i.Category?.Icon,
    SubcategoryId = i.SubcategoryId,
    SubcategoryName = i.Subcategory?.Name,
    Description = i.Description,
    Rating = i.Rating,
    IsFavorite = i.IsFavorite,
    ConsumedAt = i.ConsumedAt,
    MainPhotoUrl = i.Photos.OrderByDescending(p => p.IsMain).ThenBy(p => p.Order).FirstOrDefault()?.Url,
    Photos = i.Photos.OrderBy(p => p.Order).Select(p => new PhotoResponse { Id = p.Id, Url = p.Url, Order = p.Order, IsMain = p.IsMain }).ToList(),
    Attributes = i.Attributes.OrderBy(a => a.Order).Select(a => new AttributeDto { Name = a.Name, Value = a.Value }).ToList(),
    CreatedAt = i.CreatedAt
};

IQueryable<Item> ItemQuery(AppDbContext db) => db.Items
    .Include(i => i.Category)
    .Include(i => i.Subcategory)
    .Include(i => i.Photos)
    .Include(i => i.Attributes);

// ==================== AUTH ====================
app.MapPost("/api/auth/login", async (LoginRequest req, AppDbContext db) =>
{
    var user = await db.Users.FirstOrDefaultAsync(u => u.Email == req.Email && u.IsActive);
    if (user == null || !BCrypt.Net.BCrypt.Verify(req.Password, user.Password))
        return Results.BadRequest(new { message = "Email ou senha inválidos" });

    return Results.Ok(new AuthResponse
    {
        message = "Login efetuado",
        token = GenerateToken(user),
        user = new UserResponse { Id = user.Id, Name = user.Name, Email = user.Email, IsAdmin = user.IsAdmin }
    });
});

app.MapGet("/api/auth/me", async (ClaimsPrincipal principal, AppDbContext db) =>
{
    var uid = GetUserId(principal);
    if (uid == null) return Results.Unauthorized();
    var user = await db.Users.FindAsync(uid.Value);
    if (user == null) return Results.Unauthorized();
    return Results.Ok(new UserResponse { Id = user.Id, Name = user.Name, Email = user.Email, IsAdmin = user.IsAdmin });
}).RequireAuthorization();

// ==================== CATEGORIES ====================
app.MapGet("/api/categories", async (AppDbContext db) =>
{
    var cats = await db.Categories
        .Include(c => c.Subcategories)
        .Include(c => c.SuggestedAttributes)
        .OrderBy(c => c.Order).ThenBy(c => c.Name)
        .ToListAsync();

    var counts = await db.Items.GroupBy(i => i.CategoryId).Select(g => new { g.Key, Count = g.Count() }).ToListAsync();
    var subCounts = await db.Items.Where(i => i.SubcategoryId != null)
        .GroupBy(i => i.SubcategoryId!.Value).Select(g => new { g.Key, Count = g.Count() }).ToListAsync();

    return Results.Ok(cats.Select(c => new CategoryResponse
    {
        Id = c.Id, Name = c.Name, Icon = c.Icon, Color = c.Color, Order = c.Order,
        ItemCount = counts.FirstOrDefault(x => x.Key == c.Id)?.Count ?? 0,
        Subcategories = c.Subcategories.OrderBy(s => s.Order).Select(s => new SubcategoryResponse
        {
            Id = s.Id, CategoryId = s.CategoryId, Name = s.Name, Order = s.Order,
            ItemCount = subCounts.FirstOrDefault(x => x.Key == s.Id)?.Count ?? 0
        }).ToList(),
        SuggestedAttributes = c.SuggestedAttributes.OrderBy(a => a.Order).Select(a => a.Name).ToList()
    }));
}).RequireAuthorization();

app.MapPost("/api/categories", async (CategoryRequest req, AppDbContext db) =>
{
    var cat = new Category { Name = req.Name, Icon = req.Icon, Color = req.Color, Order = req.Order, CreatedAt = DateTime.UtcNow };
    db.Categories.Add(cat);
    foreach (var (name, idx) in req.SuggestedAttributes.Where(s => !string.IsNullOrWhiteSpace(s)).Select((s, i) => (s, i)))
        db.CategoryAttributes.Add(new CategoryAttribute { Category = cat, Name = name.Trim(), Order = idx });
    await db.SaveChangesAsync();
    return Results.Ok(new { cat.Id });
}).RequireAuthorization();

app.MapPut("/api/categories/{id:guid}", async (Guid id, CategoryRequest req, AppDbContext db) =>
{
    var cat = await db.Categories.Include(c => c.SuggestedAttributes).FirstOrDefaultAsync(c => c.Id == id);
    if (cat == null) return Results.NotFound();
    cat.Name = req.Name; cat.Icon = req.Icon; cat.Color = req.Color; cat.Order = req.Order;
    db.CategoryAttributes.RemoveRange(cat.SuggestedAttributes);
    foreach (var (name, idx) in req.SuggestedAttributes.Where(s => !string.IsNullOrWhiteSpace(s)).Select((s, i) => (s, i)))
        db.CategoryAttributes.Add(new CategoryAttribute { CategoryId = cat.Id, Name = name.Trim(), Order = idx });
    await db.SaveChangesAsync();
    return Results.Ok();
}).RequireAuthorization();

app.MapDelete("/api/categories/{id:guid}", async (Guid id, AppDbContext db) =>
{
    var cat = await db.Categories.FindAsync(id);
    if (cat == null) return Results.NotFound();
    if (await db.Items.AnyAsync(i => i.CategoryId == id))
        return Results.BadRequest(new { message = "Categoria possui itens. Mova ou exclua os itens primeiro." });
    db.Categories.Remove(cat);
    await db.SaveChangesAsync();
    return Results.Ok();
}).RequireAuthorization();

// ==================== SUBCATEGORIES ====================
app.MapPost("/api/subcategories", async (SubcategoryRequest req, AppDbContext db) =>
{
    var sub = new Subcategory { CategoryId = req.CategoryId, Name = req.Name, Order = req.Order, CreatedAt = DateTime.UtcNow };
    db.Subcategories.Add(sub);
    await db.SaveChangesAsync();
    return Results.Ok(new { sub.Id });
}).RequireAuthorization();

app.MapPut("/api/subcategories/{id:guid}", async (Guid id, SubcategoryRequest req, AppDbContext db) =>
{
    var sub = await db.Subcategories.FindAsync(id);
    if (sub == null) return Results.NotFound();
    sub.Name = req.Name; sub.Order = req.Order; sub.CategoryId = req.CategoryId;
    await db.SaveChangesAsync();
    return Results.Ok();
}).RequireAuthorization();

app.MapDelete("/api/subcategories/{id:guid}", async (Guid id, AppDbContext db) =>
{
    var sub = await db.Subcategories.FindAsync(id);
    if (sub == null) return Results.NotFound();
    db.Subcategories.Remove(sub);
    await db.SaveChangesAsync();
    return Results.Ok();
}).RequireAuthorization();

// ==================== ITEMS ====================
app.MapGet("/api/items", async (AppDbContext db, Guid? category, Guid? subcategory, bool? favorite, int? minRating, string? q, string? sort) =>
{
    var query = ItemQuery(db);
    if (category.HasValue) query = query.Where(i => i.CategoryId == category.Value);
    if (subcategory.HasValue) query = query.Where(i => i.SubcategoryId == subcategory.Value);
    if (favorite == true) query = query.Where(i => i.IsFavorite);
    if (minRating.HasValue) query = query.Where(i => i.Rating >= minRating.Value);
    if (!string.IsNullOrWhiteSpace(q))
    {
        var term = q.ToLower();
        query = query.Where(i => i.Name.ToLower().Contains(term) || (i.Description != null && i.Description.ToLower().Contains(term)));
    }
    query = sort switch
    {
        "rating" => query.OrderByDescending(i => i.Rating).ThenByDescending(i => i.CreatedAt),
        "name" => query.OrderBy(i => i.Name),
        "oldest" => query.OrderBy(i => i.CreatedAt),
        _ => query.OrderByDescending(i => i.CreatedAt)
    };
    var items = await query.ToListAsync();
    return Results.Ok(items.Select(MapItem));
}).RequireAuthorization();

app.MapGet("/api/items/{id:guid}", async (Guid id, AppDbContext db) =>
{
    var item = await ItemQuery(db).FirstOrDefaultAsync(i => i.Id == id);
    return item == null ? Results.NotFound() : Results.Ok(MapItem(item));
}).RequireAuthorization();

app.MapPost("/api/items", async (ItemRequest req, AppDbContext db) =>
{
    var item = new Item
    {
        Name = req.Name,
        CategoryId = req.CategoryId,
        SubcategoryId = req.SubcategoryId,
        Description = req.Description,
        Rating = Math.Clamp(req.Rating, 0, 5),
        IsFavorite = req.IsFavorite,
        ConsumedAt = req.ConsumedAt,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };
    db.Items.Add(item);

    // Fotos: maximo 3
    var photos = req.PhotoUrls.Where(u => !string.IsNullOrWhiteSpace(u)).Take(3).ToList();
    for (int idx = 0; idx < photos.Count; idx++)
        db.ItemPhotos.Add(new ItemPhoto { Item = item, Url = photos[idx], Order = idx, IsMain = idx == req.MainPhotoIndex, CreatedAt = DateTime.UtcNow });

    foreach (var (a, idx) in req.Attributes.Where(a => !string.IsNullOrWhiteSpace(a.Name) && !string.IsNullOrWhiteSpace(a.Value)).Select((a, i) => (a, i)))
        db.ItemAttributes.Add(new ItemAttribute { Item = item, Name = a.Name.Trim(), Value = a.Value.Trim(), Order = idx });

    await db.SaveChangesAsync();
    return Results.Ok(new { item.Id });
}).RequireAuthorization();

app.MapPut("/api/items/{id:guid}", async (Guid id, ItemRequest req, AppDbContext db) =>
{
    var item = await db.Items.Include(i => i.Photos).Include(i => i.Attributes).FirstOrDefaultAsync(i => i.Id == id);
    if (item == null) return Results.NotFound();

    item.Name = req.Name;
    item.CategoryId = req.CategoryId;
    item.SubcategoryId = req.SubcategoryId;
    item.Description = req.Description;
    item.Rating = Math.Clamp(req.Rating, 0, 5);
    item.IsFavorite = req.IsFavorite;
    item.ConsumedAt = req.ConsumedAt;
    item.UpdatedAt = DateTime.UtcNow;

    db.ItemPhotos.RemoveRange(item.Photos);
    var photos = req.PhotoUrls.Where(u => !string.IsNullOrWhiteSpace(u)).Take(3).ToList();
    for (int idx = 0; idx < photos.Count; idx++)
        db.ItemPhotos.Add(new ItemPhoto { ItemId = item.Id, Url = photos[idx], Order = idx, IsMain = idx == req.MainPhotoIndex, CreatedAt = DateTime.UtcNow });

    db.ItemAttributes.RemoveRange(item.Attributes);
    foreach (var (a, idx) in req.Attributes.Where(a => !string.IsNullOrWhiteSpace(a.Name) && !string.IsNullOrWhiteSpace(a.Value)).Select((a, i) => (a, i)))
        db.ItemAttributes.Add(new ItemAttribute { ItemId = item.Id, Name = a.Name.Trim(), Value = a.Value.Trim(), Order = idx });

    await db.SaveChangesAsync();
    return Results.Ok();
}).RequireAuthorization();

app.MapPost("/api/items/{id:guid}/toggle-favorite", async (Guid id, AppDbContext db) =>
{
    var item = await db.Items.FindAsync(id);
    if (item == null) return Results.NotFound();
    item.IsFavorite = !item.IsFavorite;
    item.UpdatedAt = DateTime.UtcNow;
    await db.SaveChangesAsync();
    return Results.Ok(new { item.IsFavorite });
}).RequireAuthorization();

app.MapDelete("/api/items/{id:guid}", async (Guid id, AppDbContext db) =>
{
    var item = await db.Items.FindAsync(id);
    if (item == null) return Results.NotFound();
    db.Items.Remove(item);
    await db.SaveChangesAsync();
    return Results.Ok();
}).RequireAuthorization();

// ==================== STATS ====================
app.MapGet("/api/stats", async (AppDbContext db) =>
{
    var total = await db.Items.CountAsync();
    var favs = await db.Items.CountAsync(i => i.IsFavorite);
    var cats = await db.Categories.CountAsync();
    var avg = total > 0 ? await db.Items.AverageAsync(i => (double)i.Rating) : 0;
    var byCat = await db.Items.Include(i => i.Category)
        .GroupBy(i => new { i.Category.Name, i.Category.Icon })
        .Select(g => new CategoryCount { Name = g.Key.Name, Icon = g.Key.Icon, Count = g.Count() })
        .OrderByDescending(c => c.Count).ToListAsync();
    var recent = await ItemQuery(db).OrderByDescending(i => i.CreatedAt).Take(6).ToListAsync();

    return Results.Ok(new StatsResponse
    {
        TotalItems = total, TotalFavorites = favs, TotalCategories = cats,
        AverageRating = Math.Round(avg, 1), ByCategory = byCat, RecentItems = recent.Select(MapItem).ToList()
    });
}).RequireAuthorization();

// ==================== UPLOAD ====================
app.MapPost("/api/upload", async (HttpRequest request) =>
{
    if (!request.HasFormContentType) return Results.BadRequest(new { message = "Envie um arquivo (multipart/form-data)" });
    var form = await request.ReadFormAsync();
    var file = form.Files.FirstOrDefault();
    if (file == null || file.Length == 0) return Results.BadRequest(new { message = "Arquivo vazio" });

    var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
    Directory.CreateDirectory(uploadsDir);
    var ext = Path.GetExtension(file.FileName);
    var fileName = $"{Guid.NewGuid()}{ext}";
    var filePath = Path.Combine(uploadsDir, fileName);
    using (var stream = new FileStream(filePath, FileMode.Create))
        await file.CopyToAsync(stream);

    return Results.Ok(new { url = $"/uploads/{fileName}" });
}).RequireAuthorization();

// ==================== IA (Gemini) ====================
app.MapGet("/api/ai/status", (GeminiService ai) => Results.Ok(new { enabled = ai.IsEnabled })).RequireAuthorization();

// Recebe uma imagem (multipart) e devolve os campos sugeridos para o formulario.
app.MapPost("/api/ai/analyze", async (HttpRequest request, GeminiService ai, AppDbContext db) =>
{
    if (!ai.IsEnabled) return Results.Json(new { message = "IA não configurada" }, statusCode: 503);
    if (!request.HasFormContentType) return Results.BadRequest(new { message = "Envie a imagem (multipart/form-data)" });
    var form = await request.ReadFormAsync();
    var file = form.Files.FirstOrDefault();
    if (file == null || file.Length == 0) return Results.BadRequest(new { message = "Imagem vazia" });

    using var ms = new MemoryStream();
    await file.CopyToAsync(ms);
    var mime = string.IsNullOrWhiteSpace(file.ContentType) ? "image/jpeg" : file.ContentType;

    var categories = await db.Categories.OrderBy(c => c.Order).Select(c => c.Name).ToListAsync();
    try
    {
        var result = await ai.AnalyzeAsync(ms.ToArray(), mime, categories);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.Json(new { message = "Falha ao analisar imagem", detail = ex.Message }, statusCode: 502);
    }
}).RequireAuthorization();

app.MapGet("/", () => "MyFood API online 🍷🍽️");

app.Run();
