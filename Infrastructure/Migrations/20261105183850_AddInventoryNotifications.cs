using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AUDITASSETRESULTS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuditRecordId = table.Column<int>(type: "int", nullable: false),
                    AssetId = table.Column<int>(type: "int", nullable: false),
                    Result = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VerifiedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AUDITASSETRESULTS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AUDITASSETRESULTS_ASSETS_AssetId",
                        column: x => x.AssetId,
                        principalTable: "ASSETS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AUDITASSETRESULTS_AUDITRECORDS_AuditRecordId",
                        column: x => x.AuditRecordId,
                        principalTable: "AUDITRECORDS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AUDITASSETRESULTS_USERS_VerifiedByUserId",
                        column: x => x.VerifiedByUserId,
                        principalTable: "USERS",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "INVENTORYSESSIONS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClosedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_INVENTORYSESSIONS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_INVENTORYSESSIONS_USERS_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "USERS",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "NOTIFICATIONS",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Link = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Read = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NOTIFICATIONS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NOTIFICATIONS_USERS_UserId",
                        column: x => x.UserId,
                        principalTable: "USERS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "INVENTORYCOUNTS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InventorySessionId = table.Column<int>(type: "int", nullable: false),
                    AssetId = table.Column<int>(type: "int", nullable: false),
                    Result = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CountedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CountedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_INVENTORYCOUNTS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_INVENTORYCOUNTS_ASSETS_AssetId",
                        column: x => x.AssetId,
                        principalTable: "ASSETS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_INVENTORYCOUNTS_INVENTORYSESSIONS_InventorySessionId",
                        column: x => x.InventorySessionId,
                        principalTable: "INVENTORYSESSIONS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_INVENTORYCOUNTS_USERS_CountedByUserId",
                        column: x => x.CountedByUserId,
                        principalTable: "USERS",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AUDITASSETRESULTS_AssetId",
                table: "AUDITASSETRESULTS",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_AUDITASSETRESULTS_AuditRecordId_AssetId",
                table: "AUDITASSETRESULTS",
                columns: new[] { "AuditRecordId", "AssetId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AUDITASSETRESULTS_VerifiedByUserId",
                table: "AUDITASSETRESULTS",
                column: "VerifiedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_INVENTORYCOUNTS_AssetId",
                table: "INVENTORYCOUNTS",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_INVENTORYCOUNTS_CountedByUserId",
                table: "INVENTORYCOUNTS",
                column: "CountedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_INVENTORYCOUNTS_InventorySessionId_AssetId",
                table: "INVENTORYCOUNTS",
                columns: new[] { "InventorySessionId", "AssetId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_INVENTORYSESSIONS_CreatedByUserId",
                table: "INVENTORYSESSIONS",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_NOTIFICATIONS_UserId_Read_CreatedAt",
                table: "NOTIFICATIONS",
                columns: new[] { "UserId", "Read", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AUDITASSETRESULTS");

            migrationBuilder.DropTable(
                name: "INVENTORYCOUNTS");

            migrationBuilder.DropTable(
                name: "NOTIFICATIONS");

            migrationBuilder.DropTable(
                name: "INVENTORYSESSIONS");
        }
    }
}
