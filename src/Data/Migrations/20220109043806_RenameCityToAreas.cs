using Microsoft.EntityFrameworkCore.Migrations;

namespace WhMgr.Migrations
{
    public partial class RenameCityToAreas : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "city",
                table: "raids",
                newName: "areas");

            migrationBuilder.RenameColumn(
                name: "city",
                table: "quests",
                newName: "areas");

            migrationBuilder.RenameColumn(
                name: "city",
                table: "pvp",
                newName: "areas");

            migrationBuilder.RenameColumn(
                name: "city",
                table: "pokemon",
                newName: "areas");

            migrationBuilder.RenameColumn(
                name: "city",
                table: "lures",
                newName: "areas");

            migrationBuilder.RenameColumn(
                name: "city",
                table: "invasions",
                newName: "areas");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "areas",
                table: "raids",
                newName: "city");

            migrationBuilder.RenameColumn(
                name: "areas",
                table: "quests",
                newName: "city");

            migrationBuilder.RenameColumn(
                name: "areas",
                table: "pvp",
                newName: "city");

            migrationBuilder.RenameColumn(
                name: "areas",
                table: "pokemon",
                newName: "city");

            migrationBuilder.RenameColumn(
                name: "areas",
                table: "lures",
                newName: "city");

            migrationBuilder.RenameColumn(
                name: "areas",
                table: "invasions",
                newName: "city");
        }
    }
}
