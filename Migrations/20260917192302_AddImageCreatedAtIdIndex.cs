using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace where_is_waldo_api_csharp.Migrations
{
    /// <inheritdoc />
    public partial class AddImageCreatedAtIdIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Images",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_Images_CreatedAt_Id",
                table: "Images",
                columns: new[] { "CreatedAt", "Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Images_CreatedAt_Id",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Images");
        }
    }
}
