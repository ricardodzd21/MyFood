using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyFood_api.Migrations
{
    /// <inheritdoc />
    public partial class MultiUserAndRatings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "observations",
                table: "items",
                type: "character varying(5000)",
                maxLength: 5000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "rating_ambiance",
                table: "items",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "rating_cleanliness",
                table: "items",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "rating_service",
                table: "items",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "state",
                table: "items",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "user_id",
                table: "items",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_items_user_id",
                table: "items",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_items_users_user_id",
                table: "items",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_items_users_user_id",
                table: "items");

            migrationBuilder.DropIndex(
                name: "IX_items_user_id",
                table: "items");

            migrationBuilder.DropColumn(
                name: "observations",
                table: "items");

            migrationBuilder.DropColumn(
                name: "rating_ambiance",
                table: "items");

            migrationBuilder.DropColumn(
                name: "rating_cleanliness",
                table: "items");

            migrationBuilder.DropColumn(
                name: "rating_service",
                table: "items");

            migrationBuilder.DropColumn(
                name: "state",
                table: "items");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "items");
        }
    }
}
