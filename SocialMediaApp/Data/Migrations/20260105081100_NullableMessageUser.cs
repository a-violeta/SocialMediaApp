using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialMediaApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class NullableMessageUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupMessages_AspNetUsers_UserId",
                table: "GroupMessages");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "GroupMessages",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupMessages_AspNetUsers_UserId",
                table: "GroupMessages",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupMessages_AspNetUsers_UserId",
                table: "GroupMessages");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "GroupMessages",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupMessages_AspNetUsers_UserId",
                table: "GroupMessages",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
