using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialMediaApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class GroupMessageUserNull : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupMessages_AspNetUsers_UserId",
                table: "GroupMessages");

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "GroupMessages",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GroupMessages_ApplicationUserId",
                table: "GroupMessages",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupMessages_AspNetUsers_ApplicationUserId",
                table: "GroupMessages",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupMessages_AspNetUsers_UserId",
                table: "GroupMessages",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupMessages_AspNetUsers_ApplicationUserId",
                table: "GroupMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupMessages_AspNetUsers_UserId",
                table: "GroupMessages");

            migrationBuilder.DropIndex(
                name: "IX_GroupMessages_ApplicationUserId",
                table: "GroupMessages");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "GroupMessages");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupMessages_AspNetUsers_UserId",
                table: "GroupMessages",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
