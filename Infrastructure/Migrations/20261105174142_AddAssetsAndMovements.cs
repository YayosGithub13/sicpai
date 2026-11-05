using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAssetsAndMovements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ASSETS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssetCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Barcode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SerialNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AssetType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Model = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhysicalCondition = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Area = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Responsible = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AcquisitionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AcquisitionValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ASSETS", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ASSETATTACHMENTS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssetId = table.Column<int>(type: "int", nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StoredFileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RelativePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Size = table.Column<long>(type: "bigint", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UploadedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ASSETATTACHMENTS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ASSETATTACHMENTS_ASSETS_AssetId",
                        column: x => x.AssetId,
                        principalTable: "ASSETS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ASSETMOVEMENTS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssetId = table.Column<int>(type: "int", nullable: false),
                    MovementType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FromArea = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ToArea = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FromResponsible = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ToResponsible = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FromLocation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ToLocation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PerformedByUserId = table.Column<int>(type: "int", nullable: true),
                    OccurredAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ASSETMOVEMENTS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ASSETMOVEMENTS_ASSETS_AssetId",
                        column: x => x.AssetId,
                        principalTable: "ASSETS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ASSETMOVEMENTS_USERS_PerformedByUserId",
                        column: x => x.PerformedByUserId,
                        principalTable: "USERS",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ASSETATTACHMENTS_AssetId",
                table: "ASSETATTACHMENTS",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_ASSETMOVEMENTS_AssetId",
                table: "ASSETMOVEMENTS",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_ASSETMOVEMENTS_PerformedByUserId",
                table: "ASSETMOVEMENTS",
                column: "PerformedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ASSETS_AssetCode",
                table: "ASSETS",
                column: "AssetCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ASSETS_Barcode",
                table: "ASSETS",
                column: "Barcode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ASSETATTACHMENTS");

            migrationBuilder.DropTable(
                name: "ASSETMOVEMENTS");

            migrationBuilder.DropTable(
                name: "ASSETS");
        }
    }
}
