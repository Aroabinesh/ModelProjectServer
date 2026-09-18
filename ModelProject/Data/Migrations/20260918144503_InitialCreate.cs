using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ModelProject.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Technicians",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Technicians", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Facilities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Facilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Facilities_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Assets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FacilityId = table.Column<int>(type: "int", nullable: false),
                    AssetCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AssetType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Assets_Facilities_FacilityId",
                        column: x => x.FacilityId,
                        principalTable: "Facilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssetId = table.Column<int>(type: "int", nullable: false),
                    FacilityId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AssignedTechnicianId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkOrders_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkOrders_Facilities_FacilityId",
                        column: x => x.FacilityId,
                        principalTable: "Facilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkOrders_Technicians_AssignedTechnicianId",
                        column: x => x.AssignedTechnicianId,
                        principalTable: "Technicians",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkOrderHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkOrderId = table.Column<int>(type: "int", nullable: false),
                    OldStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NewStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrderHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkOrderHistories_WorkOrders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "WorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Customers",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Acme Manufacturing" },
                    { 2, "Northwind Logistics" }
                });

            migrationBuilder.InsertData(
                table: "Technicians",
                columns: new[] { "Id", "Email", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, "arun.kumar@example.com", true, "Arun Kumar" },
                    { 2, "priya.sharma@example.com", true, "Priya Sharma" },
                    { 3, "ravi.verma@example.com", true, "Ravi Verma" }
                });

            migrationBuilder.InsertData(
                table: "Facilities",
                columns: new[] { "Id", "CustomerId", "Location", "Name" },
                values: new object[,]
                {
                    { 1, 1, "Chennai, IN", "Acme Plant 1" },
                    { 2, 1, "Coimbatore, IN", "Acme Plant 2" },
                    { 3, 2, "Bengaluru, IN", "Northwind Warehouse A" }
                });

            migrationBuilder.InsertData(
                table: "Assets",
                columns: new[] { "Id", "AssetCode", "AssetType", "FacilityId", "Name", "Status" },
                values: new object[,]
                {
                    { 1, "AC-1001", "Compressor", 1, "Air Compressor #1", "Active" },
                    { 2, "CV-1002", "Conveyor", 1, "Conveyor Belt A", "Active" },
                    { 3, "HV-2001", "HVAC", 2, "HVAC Unit 3", "UnderMaintenance" },
                    { 4, "FL-3001", "Vehicle", 3, "Forklift 7", "Active" }
                });

            migrationBuilder.InsertData(
                table: "WorkOrders",
                columns: new[] { "Id", "AssetId", "AssignedTechnicianId", "CreatedAt", "Description", "FacilityId", "Priority", "Status", "Title", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 1, null, new DateTime(2026, 1, 1, 9, 0, 0, 0, DateTimeKind.Utc), "Pressure drops below threshold after 20 minutes of operation.", 1, "High", "New", "Compressor pressure drop", new DateTime(2026, 1, 1, 9, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, 2, 1, new DateTime(2026, 1, 1, 9, 0, 0, 0, DateTimeKind.Utc), "Belt drifts to one side under load.", 1, "Medium", "Assigned", "Conveyor belt misalignment", new DateTime(2026, 1, 1, 9, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, 3, 2, new DateTime(2026, 1, 1, 9, 0, 0, 0, DateTimeKind.Utc), "Unit runs continuously without reaching the set temperature.", 2, "Critical", "InProgress", "HVAC unit not cooling", new DateTime(2026, 1, 1, 9, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "WorkOrderHistories",
                columns: new[] { "Id", "ChangedAt", "ChangedBy", "Comments", "NewStatus", "OldStatus", "WorkOrderId" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 9, 0, 0, 0, DateTimeKind.Utc), "system.seed", "Assigned to Arun Kumar.", "Assigned", "New", 2 },
                    { 2, new DateTime(2026, 1, 1, 9, 0, 0, 0, DateTimeKind.Utc), "system.seed", "Assigned to Priya Sharma.", "Assigned", "New", 3 },
                    { 3, new DateTime(2026, 1, 1, 9, 0, 0, 0, DateTimeKind.Utc), "system.seed", "Technician started work.", "InProgress", "Assigned", 3 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assets_AssetCode",
                table: "Assets",
                column: "AssetCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Assets_FacilityId",
                table: "Assets",
                column: "FacilityId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Name",
                table: "Customers",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Facilities_CustomerId",
                table: "Facilities",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Technicians_Email",
                table: "Technicians",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderHistories_WorkOrderId_ChangedAt",
                table: "WorkOrderHistories",
                columns: new[] { "WorkOrderId", "ChangedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_AssetId",
                table: "WorkOrders",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_AssignedTechnicianId",
                table: "WorkOrders",
                column: "AssignedTechnicianId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_CreatedAt",
                table: "WorkOrders",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_FacilityId_Status_CreatedAt",
                table: "WorkOrders",
                columns: new[] { "FacilityId", "Status", "CreatedAt" },
                descending: new[] { false, false, true });

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_Priority",
                table: "WorkOrders",
                column: "Priority");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkOrderHistories");

            migrationBuilder.DropTable(
                name: "WorkOrders");

            migrationBuilder.DropTable(
                name: "Assets");

            migrationBuilder.DropTable(
                name: "Technicians");

            migrationBuilder.DropTable(
                name: "Facilities");

            migrationBuilder.DropTable(
                name: "Customers");
        }
    }
}
