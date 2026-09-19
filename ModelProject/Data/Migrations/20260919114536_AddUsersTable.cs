using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModelProject.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUsersTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ScheduledEndDate",
                table: "WorkOrders",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ScheduledStartDate",
                table: "WorkOrders",
                type: "date",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "IsActive", "LastLoginAt", "PasswordHash", "Username" },
                values: new object[] { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, null, "$2a$11$LDw7K2LrKIm8TYaksb2cHO5e9NB/6l2QBTuOOaLQ2ez9tWvhj9Ux6", "admin" });

            migrationBuilder.UpdateData(
                table: "WorkOrders",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ScheduledEndDate", "ScheduledStartDate" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "WorkOrders",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ScheduledEndDate", "ScheduledStartDate" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "WorkOrders",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ScheduledEndDate", "ScheduledStartDate" },
                values: new object[] { null, null });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropColumn(
                name: "ScheduledEndDate",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "ScheduledStartDate",
                table: "WorkOrders");
        }
    }
}
