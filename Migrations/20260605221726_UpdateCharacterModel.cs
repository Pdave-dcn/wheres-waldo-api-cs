using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace where_is_waldo_api_csharp.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCharacterModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Characters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CharacterType = table.Column<string>(type: "text", nullable: false),
                    TargetXRatio = table.Column<double>(type: "double precision", nullable: false),
                    TargetYRatio = table.Column<double>(type: "double precision", nullable: false),
                    ToleranceXRatio = table.Column<double>(type: "double precision", nullable: false),
                    ToleranceYRatio = table.Column<double>(type: "double precision", nullable: false),
                    ImageId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Characters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Characters_Images_ImageId",
                        column: x => x.ImageId,
                        principalTable: "Images",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Characters_ImageId_CharacterType",
                table: "Characters",
                columns: new[] { "ImageId", "CharacterType" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Characters");
        }
    }
}
