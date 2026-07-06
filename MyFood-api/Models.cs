using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MyFood_api;

// ==================== ENTITIES ====================

public class User
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    [JsonIgnore]
    public string Password { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class Category
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;      // Vinhos, Cervejas, Comidas
    public string? Icon { get; set; }                     // emoji ou nome de icone (ex: "🍷")
    public string? Color { get; set; }                    // hex opcional para o card
    public int Order { get; set; }
    public DateTime CreatedAt { get; set; }

    [JsonIgnore]
    public ICollection<Subcategory> Subcategories { get; set; } = new List<Subcategory>();
    [JsonIgnore]
    public ICollection<CategoryAttribute> SuggestedAttributes { get; set; } = new List<CategoryAttribute>();
    [JsonIgnore]
    public ICollection<Item> Items { get; set; } = new List<Item>();
}

public class Subcategory
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;      // Tinto Suave, IPA, Sobremesa...
    public int Order { get; set; }
    public DateTime CreatedAt { get; set; }

    [JsonIgnore]
    public Category Category { get; set; } = null!;
}

// Atributos sugeridos por categoria (ex: Vinhos -> Teor, Origem, Safra; Comidas -> Estabelecimento, Prato)
public class CategoryAttribute
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Order { get; set; }

    [JsonIgnore]
    public Category Category { get; set; } = null!;
}

public class Item
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public Guid? SubcategoryId { get; set; }
    public string? Description { get; set; }              // opcional
    public string? City { get; set; }                     // cidade onde consumiu (texto livre)
    public string? Establishment { get; set; }            // estabelecimento (texto livre, p/ comidas)
    public int Rating { get; set; }                       // 0-5 estrelas
    public bool IsFavorite { get; set; }
    public DateTime? ConsumedAt { get; set; }             // quando consumiu (opcional)
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    [JsonIgnore]
    public Category Category { get; set; } = null!;
    [JsonIgnore]
    public Subcategory? Subcategory { get; set; }
    [JsonIgnore]
    public ICollection<ItemPhoto> Photos { get; set; } = new List<ItemPhoto>();
    [JsonIgnore]
    public ICollection<ItemAttribute> Attributes { get; set; } = new List<ItemAttribute>();
}

public class ItemPhoto
{
    public Guid Id { get; set; }
    public Guid ItemId { get; set; }
    public string Url { get; set; } = string.Empty;
    public int Order { get; set; }
    public bool IsMain { get; set; }
    public DateTime CreatedAt { get; set; }

    [JsonIgnore]
    public Item Item { get; set; } = null!;
}

// Atributo flexivel nome->valor (Teor=13%, Origem=Chile, Estabelecimento=Restaurante X)
public class ItemAttribute
{
    public Guid Id { get; set; }
    public Guid ItemId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public int Order { get; set; }

    [JsonIgnore]
    public Item Item { get; set; } = null!;
}

// ==================== DTOs - Requests ====================

public class LoginRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
    [Required]
    public string Password { get; set; } = string.Empty;
}

public class CategoryRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public int Order { get; set; }
    public List<string> SuggestedAttributes { get; set; } = new();
}

public class SubcategoryRequest
{
    [Required]
    public Guid CategoryId { get; set; }
    [Required]
    public string Name { get; set; } = string.Empty;
    public int Order { get; set; }
}

public class AttributeDto
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public class ItemRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;
    [Required]
    public Guid CategoryId { get; set; }
    public Guid? SubcategoryId { get; set; }
    public string? Description { get; set; }
    public string? City { get; set; }
    public string? Establishment { get; set; }
    public int Rating { get; set; }
    public bool IsFavorite { get; set; }
    public DateTime? ConsumedAt { get; set; }
    public List<string> PhotoUrls { get; set; } = new();   // maximo 3 (backend corta)
    public int MainPhotoIndex { get; set; } = 0;
    public List<AttributeDto> Attributes { get; set; } = new();
}

// ==================== DTOs - Responses ====================

public class AuthResponse
{
    public string message { get; set; } = string.Empty;
    public string token { get; set; } = string.Empty;
    public UserResponse user { get; set; } = new();
}

public class UserResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
}

public class CategoryResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public int Order { get; set; }
    public int ItemCount { get; set; }
    public List<SubcategoryResponse> Subcategories { get; set; } = new();
    public List<string> SuggestedAttributes { get; set; } = new();
}

public class SubcategoryResponse
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Order { get; set; }
    public int ItemCount { get; set; }
}

public class PhotoResponse
{
    public Guid Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public int Order { get; set; }
    public bool IsMain { get; set; }
}

public class ItemResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? CategoryIcon { get; set; }
    public Guid? SubcategoryId { get; set; }
    public string? SubcategoryName { get; set; }
    public string? Description { get; set; }
    public string? City { get; set; }
    public string? Establishment { get; set; }
    public int Rating { get; set; }
    public bool IsFavorite { get; set; }
    public DateTime? ConsumedAt { get; set; }
    public string? MainPhotoUrl { get; set; }
    public List<PhotoResponse> Photos { get; set; } = new();
    public List<AttributeDto> Attributes { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class StatsResponse
{
    public int TotalItems { get; set; }
    public int TotalFavorites { get; set; }
    public int TotalCategories { get; set; }
    public double AverageRating { get; set; }
    public List<CategoryCount> ByCategory { get; set; } = new();
    public List<ItemResponse> RecentItems { get; set; } = new();
}

public class CategoryCount
{
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public int Count { get; set; }
}
