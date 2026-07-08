using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyFood_api.Migrations
{
    /// <inheritdoc />
    public partial class CategoryVenueRating : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "has_venue_rating",
                table: "categories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            // Comidas = categoria de local (mostra sub-notas)
            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0001-000000000004"),
                column: "has_venue_rating",
                value: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "has_venue_rating",
                table: "categories");
        }
    }
}
