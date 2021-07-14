using Microsoft.EntityFrameworkCore.Migrations;

namespace WhMgr.Migrations
{
    public partial class AddExRaidGymSubFilter : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ex_eligible",
                table: "raids",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ex_eligible",
                table: "gyms",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ex_eligible",
                table: "raids");

            migrationBuilder.DropColumn(
                name: "ex_eligible",
                table: "gyms");
        }
    }
}
