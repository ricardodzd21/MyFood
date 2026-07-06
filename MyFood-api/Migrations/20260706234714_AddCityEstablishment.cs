using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyFood_api.Migrations
{
    /// <inheritdoc />
    public partial class AddCityEstablishment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "city",
                table: "items",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "establishment",
                table: "items",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "city",
                table: "items");

            migrationBuilder.DropColumn(
                name: "establishment",
                table: "items");
        }
    }
}
