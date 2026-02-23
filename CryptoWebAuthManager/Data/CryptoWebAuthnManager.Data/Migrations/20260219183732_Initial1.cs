using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CryptoWebAuthnManager.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "WebAuthnCredentials",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WebAuthnCredentials_ApplicationUserId",
                table: "WebAuthnCredentials",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_WebAuthnCredentials_AspNetUsers_ApplicationUserId",
                table: "WebAuthnCredentials",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WebAuthnCredentials_AspNetUsers_ApplicationUserId",
                table: "WebAuthnCredentials");

            migrationBuilder.DropIndex(
                name: "IX_WebAuthnCredentials_ApplicationUserId",
                table: "WebAuthnCredentials");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "WebAuthnCredentials");
        }
    }
}
