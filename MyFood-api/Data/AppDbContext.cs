using Microsoft.EntityFrameworkCore;

namespace MyFood_api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Subcategory> Subcategories => Set<Subcategory>();
    public DbSet<CategoryAttribute> CategoryAttributes => Set<CategoryAttribute>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<ItemPhoto> ItemPhotos => Set<ItemPhoto>();
    public DbSet<ItemAttribute> ItemAttributes => Set<ItemAttribute>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // User
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(255).IsRequired();
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
            entity.Property(e => e.Password).HasColumnName("password").IsRequired();
            entity.Property(e => e.IsAdmin).HasColumnName("is_admin").HasDefaultValue(false);
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // Category
        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("categories");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Icon).HasColumnName("icon").HasMaxLength(50);
            entity.Property(e => e.Color).HasColumnName("color").HasMaxLength(20);
            entity.Property(e => e.Order).HasColumnName("order").HasDefaultValue(0);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // Subcategory
        modelBuilder.Entity<Subcategory>(entity =>
        {
            entity.ToTable("subcategories");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Order).HasColumnName("order").HasDefaultValue(0);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");

            entity.HasOne(e => e.Category)
                .WithMany(c => c.Subcategories)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.CategoryId, e.Name }).IsUnique();
        });

        // CategoryAttribute (sugeridos)
        modelBuilder.Entity<CategoryAttribute>(entity =>
        {
            entity.ToTable("category_attributes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Order).HasColumnName("order").HasDefaultValue(0);

            entity.HasOne(e => e.Category)
                .WithMany(c => c.SuggestedAttributes)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Item
        modelBuilder.Entity<Item>(entity =>
        {
            entity.ToTable("items");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(255).IsRequired();
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.SubcategoryId).HasColumnName("subcategory_id");
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(5000);
            entity.Property(e => e.Observations).HasColumnName("observations").HasMaxLength(5000);
            entity.Property(e => e.City).HasColumnName("city").HasMaxLength(150);
            entity.Property(e => e.State).HasColumnName("state").HasMaxLength(2);
            entity.Property(e => e.Establishment).HasColumnName("establishment").HasMaxLength(200);
            entity.Property(e => e.Rating).HasColumnName("rating").HasDefaultValue(0);
            entity.Property(e => e.RatingCleanliness).HasColumnName("rating_cleanliness").HasDefaultValue(0);
            entity.Property(e => e.RatingService).HasColumnName("rating_service").HasDefaultValue(0);
            entity.Property(e => e.RatingAmbiance).HasColumnName("rating_ambiance").HasDefaultValue(0);
            entity.Property(e => e.IsFavorite).HasColumnName("is_favorite").HasDefaultValue(false);
            entity.Property(e => e.ConsumedAt).HasColumnName("consumed_at");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Category)
                .WithMany(c => c.Items)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.UserId);

            entity.HasOne(e => e.Subcategory)
                .WithMany()
                .HasForeignKey(e => e.SubcategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.CategoryId);
            entity.HasIndex(e => e.IsFavorite);
            entity.HasIndex(e => e.Rating);
        });

        // ItemPhoto
        modelBuilder.Entity<ItemPhoto>(entity =>
        {
            entity.ToTable("item_photos");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.ItemId).HasColumnName("item_id");
            entity.Property(e => e.Url).HasColumnName("url").HasMaxLength(1000).IsRequired();
            entity.Property(e => e.Order).HasColumnName("order").HasDefaultValue(0);
            entity.Property(e => e.IsMain).HasColumnName("is_main").HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");

            entity.HasOne(e => e.Item)
                .WithMany(i => i.Photos)
                .HasForeignKey(e => e.ItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ItemAttribute (flexivel)
        modelBuilder.Entity<ItemAttribute>(entity =>
        {
            entity.ToTable("item_attributes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.ItemId).HasColumnName("item_id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Value).HasColumnName("value").HasMaxLength(500).IsRequired();
            entity.Property(e => e.Order).HasColumnName("order").HasDefaultValue(0);

            entity.HasOne(e => e.Item)
                .WithMany(i => i.Attributes)
                .HasForeignKey(e => e.ItemId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.ItemId);
        });

        // ==================== SEED ====================
        var seed = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        Guid cid(int n) => Guid.Parse($"00000000-0000-0000-0001-{n:D12}");
        Guid said(int n) => Guid.Parse($"00000000-0000-0000-0002-{n:D12}");
        Guid subid(int n) => Guid.Parse($"00000000-0000-0000-0003-{n:D12}");

        modelBuilder.Entity<Category>().HasData(
            new Category { Id = cid(1), Name = "Vinhos", Icon = "🍷", Color = "#7f1d1d", Order = 1, CreatedAt = seed },
            new Category { Id = cid(2), Name = "Cervejas", Icon = "🍺", Color = "#b45309", Order = 2, CreatedAt = seed },
            new Category { Id = cid(3), Name = "Destilados", Icon = "🥃", Color = "#92400e", Order = 3, CreatedAt = seed },
            new Category { Id = cid(4), Name = "Comidas", Icon = "🍽️", Color = "#166534", Order = 4, CreatedAt = seed },
            new Category { Id = cid(5), Name = "Cafés", Icon = "☕", Color = "#44403c", Order = 5, CreatedAt = seed }
        );

        // Subcategorias de exemplo
        var subs = new List<Subcategory>();
        int subSeq = 1;
        void addSubs(int catN, params string[] names)
        {
            int o = 1;
            foreach (var n in names)
                subs.Add(new Subcategory { Id = subid(subSeq++), CategoryId = cid(catN), Name = n, Order = o++, CreatedAt = seed });
        }
        addSubs(1, "Tinto Seco", "Tinto Suave", "Branco", "Rosé", "Espumante");
        addSubs(2, "Pilsen", "IPA", "Weiss", "Stout", "Lager");
        addSubs(3, "Whisky", "Vodka", "Gin", "Cachaça", "Rum", "Tequila");
        addSubs(4, "Massa", "Carne", "Peixe", "Lanche", "Sobremesa", "Petisco");
        addSubs(5, "Espresso", "Coado", "Especial");
        modelBuilder.Entity<Subcategory>().HasData(subs.ToArray());

        // Atributos sugeridos por categoria
        var attrs = new List<CategoryAttribute>();
        int aSeq = 1;
        void addAttrs(int catN, params string[] names)
        {
            int o = 1;
            foreach (var n in names)
                attrs.Add(new CategoryAttribute { Id = said(aSeq++), CategoryId = cid(catN), Name = n, Order = o++ });
        }
        addAttrs(1, "Teor Alcoólico", "Origem", "Safra", "Uva", "Preço");
        addAttrs(2, "Teor Alcoólico", "Origem", "IBU", "Volume", "Preço");
        addAttrs(3, "Teor Alcoólico", "Origem", "Idade", "Preço");
        addAttrs(4, "Estabelecimento", "Ingredientes", "Preço");
        addAttrs(5, "Origem", "Torra", "Método", "Preço");
        modelBuilder.Entity<CategoryAttribute>().HasData(attrs.ToArray());
    }
}
