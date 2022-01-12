using Microsoft.EntityFrameworkCore.Migrations;

namespace WhMgr.Migrations
{
    public partial class ModifyFormsToList : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "form",
                table: "raids",
                newName: "forms");

            migrationBuilder.RenameColumn(
                name: "form",
                table: "pvp",
                newName: "forms");

            migrationBuilder.RenameColumn(
                name: "form",
                table: "pokemon",
                newName: "forms");

            migrationBuilder.AlterColumn<byte>(
                name: "size",
                table: "pokemon",
                type: "tinyint unsigned",
                nullable: false,
                oldClrType: typeof(uint),
                oldType: "int unsigned");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "forms",
                table: "raids",
                newName: "form");

            migrationBuilder.RenameColumn(
                name: "forms",
                table: "pvp",
                newName: "form");

            migrationBuilder.RenameColumn(
                name: "forms",
                table: "pokemon",
                newName: "form");

            migrationBuilder.AlterColumn<uint>(
                name: "size",
                table: "pokemon",
                type: "int unsigned",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "tinyint unsigned");
        }
    }
}
