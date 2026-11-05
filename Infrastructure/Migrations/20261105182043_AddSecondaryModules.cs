using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSecondaryModules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ACTIVITYLOGS",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OccurredAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ACTIVITYLOGS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ACTIVITYLOGS_USERS_UserId",
                        column: x => x.UserId,
                        principalTable: "USERS",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "INSTITUTIONALAREAS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ParentAreaId = table.Column<int>(type: "int", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_INSTITUTIONALAREAS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_INSTITUTIONALAREAS_INSTITUTIONALAREAS_ParentAreaId",
                        column: x => x.ParentAreaId,
                        principalTable: "INSTITUTIONALAREAS",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PHYSICALLOCATIONS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Site = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Building = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Floor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PHYSICALLOCATIONS", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AUDITRECORDS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ScheduledDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClosedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AreaId = table.Column<int>(type: "int", nullable: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AUDITRECORDS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AUDITRECORDS_INSTITUTIONALAREAS_AreaId",
                        column: x => x.AreaId,
                        principalTable: "INSTITUTIONALAREAS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AUDITRECORDS_USERS_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "USERS",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ACTIVITYLOGS_OccurredAt",
                table: "ACTIVITYLOGS",
                column: "OccurredAt");

            migrationBuilder.CreateIndex(
                name: "IX_ACTIVITYLOGS_UserId",
                table: "ACTIVITYLOGS",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AUDITRECORDS_AreaId",
                table: "AUDITRECORDS",
                column: "AreaId");

            migrationBuilder.CreateIndex(
                name: "IX_AUDITRECORDS_CreatedByUserId",
                table: "AUDITRECORDS",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_INSTITUTIONALAREAS_Code",
                table: "INSTITUTIONALAREAS",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_INSTITUTIONALAREAS_ParentAreaId",
                table: "INSTITUTIONALAREAS",
                column: "ParentAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_PHYSICALLOCATIONS_Code",
                table: "PHYSICALLOCATIONS",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ACTIVITYLOGS");

            migrationBuilder.DropTable(
                name: "AUDITRECORDS");

            migrationBuilder.DropTable(
                name: "PHYSICALLOCATIONS");

            migrationBuilder.DropTable(
                name: "INSTITUTIONALAREAS");
        }
    }
}
