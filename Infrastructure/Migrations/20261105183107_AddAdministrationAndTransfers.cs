using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAdministrationAndTransfers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ASSETTRANSFERS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssetId = table.Column<int>(type: "int", nullable: false),
                    FromArea = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ToArea = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FromResponsible = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ToResponsible = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FromLocation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ToLocation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequestedByUserId = table.Column<int>(type: "int", nullable: false),
                    ApprovedByUserId = table.Column<int>(type: "int", nullable: true),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ASSETTRANSFERS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ASSETTRANSFERS_ASSETS_AssetId",
                        column: x => x.AssetId,
                        principalTable: "ASSETS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ASSETTRANSFERS_USERS_ApprovedByUserId",
                        column: x => x.ApprovedByUserId,
                        principalTable: "USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ASSETTRANSFERS_USERS_RequestedByUserId",
                        column: x => x.RequestedByUserId,
                        principalTable: "USERS",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PERMISSIONCATALOGS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Module = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PERMISSIONCATALOGS", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ROLEPERMISSIONS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleCatalogId = table.Column<int>(type: "int", nullable: false),
                    PermissionCatalogId = table.Column<int>(type: "int", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ROLEPERMISSIONS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ROLEPERMISSIONS_PERMISSIONCATALOGS_PermissionCatalogId",
                        column: x => x.PermissionCatalogId,
                        principalTable: "PERMISSIONCATALOGS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ROLEPERMISSIONS_USERCATALOGS_RoleCatalogId",
                        column: x => x.RoleCatalogId,
                        principalTable: "USERCATALOGS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ASSETTRANSFERS_ApprovedByUserId",
                table: "ASSETTRANSFERS",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ASSETTRANSFERS_AssetId",
                table: "ASSETTRANSFERS",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_ASSETTRANSFERS_RequestedByUserId",
                table: "ASSETTRANSFERS",
                column: "RequestedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PERMISSIONCATALOGS_Code",
                table: "PERMISSIONCATALOGS",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ROLEPERMISSIONS_PermissionCatalogId",
                table: "ROLEPERMISSIONS",
                column: "PermissionCatalogId");

            migrationBuilder.CreateIndex(
                name: "IX_ROLEPERMISSIONS_RoleCatalogId_PermissionCatalogId",
                table: "ROLEPERMISSIONS",
                columns: new[] { "RoleCatalogId", "PermissionCatalogId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ASSETTRANSFERS");

            migrationBuilder.DropTable(
                name: "ROLEPERMISSIONS");

            migrationBuilder.DropTable(
                name: "PERMISSIONCATALOGS");
        }
    }
}
