using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HabitService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "habit");

            migrationBuilder.CreateTable(
                name: "Habits",
                schema: "habit",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Frequency = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TargetDaysPerWeek = table.Column<byte>(type: "tinyint", nullable: true),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Habits", x => x.Id);
                    table.CheckConstraint("CK_Habits_Frequency", "[Frequency] IN ('daily', 'weekly')");
                    table.CheckConstraint("CK_Habits_TargetDaysPerWeek", "[TargetDaysPerWeek] IS NULL OR ([TargetDaysPerWeek] >= 1 AND [TargetDaysPerWeek] <= 7)");
                });

            migrationBuilder.CreateTable(
                name: "HabitCompletions",
                schema: "habit",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    HabitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompletedDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HabitCompletions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HabitCompletions_Habits_HabitId",
                        column: x => x.HabitId,
                        principalSchema: "habit",
                        principalTable: "Habits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HabitCompletions_HabitId",
                schema: "habit",
                table: "HabitCompletions",
                column: "HabitId");

            migrationBuilder.CreateIndex(
                name: "IX_HabitCompletions_HabitId_CompletedDate",
                schema: "habit",
                table: "HabitCompletions",
                columns: new[] { "HabitId", "CompletedDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HabitCompletions_UserId_CompletedDate",
                schema: "habit",
                table: "HabitCompletions",
                columns: new[] { "UserId", "CompletedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Habits_IsPublic_IsActive",
                schema: "habit",
                table: "Habits",
                columns: new[] { "IsPublic", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Habits_UserId_IsActive",
                schema: "habit",
                table: "Habits",
                columns: new[] { "UserId", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HabitCompletions",
                schema: "habit");

            migrationBuilder.DropTable(
                name: "Habits",
                schema: "habit");
        }
    }
}
