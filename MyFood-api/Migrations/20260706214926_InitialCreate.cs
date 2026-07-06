using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MyFood_api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    icon = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    color = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    password = table.Column<string>(type: "text", nullable: false),
                    is_admin = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "category_attributes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_category_attributes", x => x.id);
                    table.ForeignKey(
                        name: "FK_category_attributes_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "subcategories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subcategories", x => x.id);
                    table.ForeignKey(
                        name: "FK_subcategories_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    subcategory_id = table.Column<Guid>(type: "uuid", nullable: true),
                    description = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    rating = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    is_favorite = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    consumed_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_items_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_items_subcategories_subcategory_id",
                        column: x => x.subcategory_id,
                        principalTable: "subcategories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "item_attributes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    value = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_item_attributes", x => x.id);
                    table.ForeignKey(
                        name: "FK_item_attributes_items_item_id",
                        column: x => x.item_id,
                        principalTable: "items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "item_photos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    is_main = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_item_photos", x => x.id);
                    table.ForeignKey(
                        name: "FK_item_photos_items_item_id",
                        column: x => x.item_id,
                        principalTable: "items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "categories",
                columns: new[] { "id", "color", "created_at", "icon", "name", "order" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0001-000000000001"), "#7f1d1d", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "🍷", "Vinhos", 1 },
                    { new Guid("00000000-0000-0000-0001-000000000002"), "#b45309", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "🍺", "Cervejas", 2 },
                    { new Guid("00000000-0000-0000-0001-000000000003"), "#92400e", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "🥃", "Destilados", 3 },
                    { new Guid("00000000-0000-0000-0001-000000000004"), "#166534", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "🍽️", "Comidas", 4 },
                    { new Guid("00000000-0000-0000-0001-000000000005"), "#44403c", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "☕", "Cafés", 5 }
                });

            migrationBuilder.InsertData(
                table: "category_attributes",
                columns: new[] { "id", "category_id", "name", "order" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0002-000000000001"), new Guid("00000000-0000-0000-0001-000000000001"), "Teor Alcoólico", 1 },
                    { new Guid("00000000-0000-0000-0002-000000000002"), new Guid("00000000-0000-0000-0001-000000000001"), "Origem", 2 },
                    { new Guid("00000000-0000-0000-0002-000000000003"), new Guid("00000000-0000-0000-0001-000000000001"), "Safra", 3 },
                    { new Guid("00000000-0000-0000-0002-000000000004"), new Guid("00000000-0000-0000-0001-000000000001"), "Uva", 4 },
                    { new Guid("00000000-0000-0000-0002-000000000005"), new Guid("00000000-0000-0000-0001-000000000001"), "Preço", 5 },
                    { new Guid("00000000-0000-0000-0002-000000000006"), new Guid("00000000-0000-0000-0001-000000000002"), "Teor Alcoólico", 1 },
                    { new Guid("00000000-0000-0000-0002-000000000007"), new Guid("00000000-0000-0000-0001-000000000002"), "Origem", 2 },
                    { new Guid("00000000-0000-0000-0002-000000000008"), new Guid("00000000-0000-0000-0001-000000000002"), "IBU", 3 },
                    { new Guid("00000000-0000-0000-0002-000000000009"), new Guid("00000000-0000-0000-0001-000000000002"), "Volume", 4 },
                    { new Guid("00000000-0000-0000-0002-000000000010"), new Guid("00000000-0000-0000-0001-000000000002"), "Preço", 5 },
                    { new Guid("00000000-0000-0000-0002-000000000011"), new Guid("00000000-0000-0000-0001-000000000003"), "Teor Alcoólico", 1 },
                    { new Guid("00000000-0000-0000-0002-000000000012"), new Guid("00000000-0000-0000-0001-000000000003"), "Origem", 2 },
                    { new Guid("00000000-0000-0000-0002-000000000013"), new Guid("00000000-0000-0000-0001-000000000003"), "Idade", 3 },
                    { new Guid("00000000-0000-0000-0002-000000000014"), new Guid("00000000-0000-0000-0001-000000000003"), "Preço", 4 },
                    { new Guid("00000000-0000-0000-0002-000000000015"), new Guid("00000000-0000-0000-0001-000000000004"), "Estabelecimento", 1 },
                    { new Guid("00000000-0000-0000-0002-000000000016"), new Guid("00000000-0000-0000-0001-000000000004"), "Ingredientes", 2 },
                    { new Guid("00000000-0000-0000-0002-000000000017"), new Guid("00000000-0000-0000-0001-000000000004"), "Preço", 3 },
                    { new Guid("00000000-0000-0000-0002-000000000018"), new Guid("00000000-0000-0000-0001-000000000005"), "Origem", 1 },
                    { new Guid("00000000-0000-0000-0002-000000000019"), new Guid("00000000-0000-0000-0001-000000000005"), "Torra", 2 },
                    { new Guid("00000000-0000-0000-0002-000000000020"), new Guid("00000000-0000-0000-0001-000000000005"), "Método", 3 },
                    { new Guid("00000000-0000-0000-0002-000000000021"), new Guid("00000000-0000-0000-0001-000000000005"), "Preço", 4 }
                });

            migrationBuilder.InsertData(
                table: "subcategories",
                columns: new[] { "id", "category_id", "created_at", "name", "order" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0003-000000000001"), new Guid("00000000-0000-0000-0001-000000000001"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Tinto Seco", 1 },
                    { new Guid("00000000-0000-0000-0003-000000000002"), new Guid("00000000-0000-0000-0001-000000000001"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Tinto Suave", 2 },
                    { new Guid("00000000-0000-0000-0003-000000000003"), new Guid("00000000-0000-0000-0001-000000000001"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Branco", 3 },
                    { new Guid("00000000-0000-0000-0003-000000000004"), new Guid("00000000-0000-0000-0001-000000000001"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Rosé", 4 },
                    { new Guid("00000000-0000-0000-0003-000000000005"), new Guid("00000000-0000-0000-0001-000000000001"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Espumante", 5 },
                    { new Guid("00000000-0000-0000-0003-000000000006"), new Guid("00000000-0000-0000-0001-000000000002"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Pilsen", 1 },
                    { new Guid("00000000-0000-0000-0003-000000000007"), new Guid("00000000-0000-0000-0001-000000000002"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "IPA", 2 },
                    { new Guid("00000000-0000-0000-0003-000000000008"), new Guid("00000000-0000-0000-0001-000000000002"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Weiss", 3 },
                    { new Guid("00000000-0000-0000-0003-000000000009"), new Guid("00000000-0000-0000-0001-000000000002"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Stout", 4 },
                    { new Guid("00000000-0000-0000-0003-000000000010"), new Guid("00000000-0000-0000-0001-000000000002"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Lager", 5 },
                    { new Guid("00000000-0000-0000-0003-000000000011"), new Guid("00000000-0000-0000-0001-000000000003"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Whisky", 1 },
                    { new Guid("00000000-0000-0000-0003-000000000012"), new Guid("00000000-0000-0000-0001-000000000003"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Vodka", 2 },
                    { new Guid("00000000-0000-0000-0003-000000000013"), new Guid("00000000-0000-0000-0001-000000000003"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Gin", 3 },
                    { new Guid("00000000-0000-0000-0003-000000000014"), new Guid("00000000-0000-0000-0001-000000000003"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Cachaça", 4 },
                    { new Guid("00000000-0000-0000-0003-000000000015"), new Guid("00000000-0000-0000-0001-000000000003"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Rum", 5 },
                    { new Guid("00000000-0000-0000-0003-000000000016"), new Guid("00000000-0000-0000-0001-000000000003"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Tequila", 6 },
                    { new Guid("00000000-0000-0000-0003-000000000017"), new Guid("00000000-0000-0000-0001-000000000004"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Massa", 1 },
                    { new Guid("00000000-0000-0000-0003-000000000018"), new Guid("00000000-0000-0000-0001-000000000004"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Carne", 2 },
                    { new Guid("00000000-0000-0000-0003-000000000019"), new Guid("00000000-0000-0000-0001-000000000004"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Peixe", 3 },
                    { new Guid("00000000-0000-0000-0003-000000000020"), new Guid("00000000-0000-0000-0001-000000000004"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Lanche", 4 },
                    { new Guid("00000000-0000-0000-0003-000000000021"), new Guid("00000000-0000-0000-0001-000000000004"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Sobremesa", 5 },
                    { new Guid("00000000-0000-0000-0003-000000000022"), new Guid("00000000-0000-0000-0001-000000000004"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Petisco", 6 },
                    { new Guid("00000000-0000-0000-0003-000000000023"), new Guid("00000000-0000-0000-0001-000000000005"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Espresso", 1 },
                    { new Guid("00000000-0000-0000-0003-000000000024"), new Guid("00000000-0000-0000-0001-000000000005"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Coado", 2 },
                    { new Guid("00000000-0000-0000-0003-000000000025"), new Guid("00000000-0000-0000-0001-000000000005"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Especial", 3 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_categories_name",
                table: "categories",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_category_attributes_category_id",
                table: "category_attributes",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_item_attributes_item_id",
                table: "item_attributes",
                column: "item_id");

            migrationBuilder.CreateIndex(
                name: "IX_item_photos_item_id",
                table: "item_photos",
                column: "item_id");

            migrationBuilder.CreateIndex(
                name: "IX_items_category_id",
                table: "items",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_items_is_favorite",
                table: "items",
                column: "is_favorite");

            migrationBuilder.CreateIndex(
                name: "IX_items_rating",
                table: "items",
                column: "rating");

            migrationBuilder.CreateIndex(
                name: "IX_items_subcategory_id",
                table: "items",
                column: "subcategory_id");

            migrationBuilder.CreateIndex(
                name: "IX_subcategories_category_id_name",
                table: "subcategories",
                columns: new[] { "category_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                table: "users",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "category_attributes");

            migrationBuilder.DropTable(
                name: "item_attributes");

            migrationBuilder.DropTable(
                name: "item_photos");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "items");

            migrationBuilder.DropTable(
                name: "subcategories");

            migrationBuilder.DropTable(
                name: "categories");
        }
    }
}
