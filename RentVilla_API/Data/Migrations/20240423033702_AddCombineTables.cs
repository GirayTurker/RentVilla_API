using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentVilla_API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCombineTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UsersAddress_Users_AppuserID",
                table: "UsersAddress");

            migrationBuilder.AlterColumn<int>(
                name: "AppuserID",
                table: "UsersAddress",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_UsersAddress_Users_AppuserID",
                table: "UsersAddress",
                column: "AppuserID",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UsersAddress_Users_AppuserID",
                table: "UsersAddress");

            migrationBuilder.AlterColumn<int>(
                name: "AppuserID",
                table: "UsersAddress",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_UsersAddress_Users_AppuserID",
                table: "UsersAddress",
                column: "AppuserID",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
