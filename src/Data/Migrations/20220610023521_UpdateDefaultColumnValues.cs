using Microsoft.EntityFrameworkCore.Migrations;

namespace WhMgr.Migrations
{
    public partial class UpdateDefaultColumnValues : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "gender",
                table: "pvp",
                type: "varchar(1)",
                nullable: false);

            migrationBuilder.AlterColumn<string>(
                name: "gender",
                table: "pokemon",
                type: "varchar(1)",
                nullable: false,
                defaultValue: '*');

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "locations",
                type: "longtext",
                nullable: false,
                defaultValue: null);

            migrationBuilder.AlterColumn<string>(
                name: "pokestop_name",
                table: "quests",
                type: "longtext",
                nullable: true,
                defaultValue: null);

            migrationBuilder.AlterColumn<string>(
                name: "pokestop_name",
                table: "invasions",
                type: "longtext",
                nullable: true,
                defaultValue: null);

            migrationBuilder.AlterColumn<string>(
                name: "pokestop_name",
                table: "lures",
                type: "longtext",
                nullable: true,
                defaultValue: null);

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "gyms",
                type: "longtext",
                nullable: true,
                defaultValue: null);

            migrationBuilder.AlterColumn<int>(
                name: "max_cp",
                table: "pokemon",
                type: "int",
                nullable: false,
                defaultValue: int.MaxValue);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "gender",
                table: "pvp",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(1)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "gender",
                table: "pokemon",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(1)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "locations",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "pokestop_name",
                table: "quests",
                type: "longtext",
                nullable: true,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "pokestop_name",
                table: "invasions",
                type: "longtext",
                nullable: true,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "pokestop_name",
                table: "lures",
                type: "longtext",
                nullable: true,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "gyms",
                type: "longtext",
                nullable: true,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "max_cp",
                table: "pokemon",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: false)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
