using Microsoft.EntityFrameworkCore.Migrations;

namespace WhMgr.Migrations
{
    public partial class AddPokemonMaxCP : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "max_cp",
                table: "pokemon",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "max_cp",
                table: "pokemon");
        }
    }
}
