using Microsoft.EntityFrameworkCore.Migrations;
using System.IO;

namespace EDennis.AspNetIdentityServer.Data.Migrations
{
    public partial class ClientUserClaims : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetClaims",
                columns: table => new
                {
                    ClaimType = table.Column<string>(nullable: false),
                    ClaimValue = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetClaims", x => new { x.ClaimType, x.ClaimValue });
                });

            migrationBuilder.CreateTable(
                name: "AspNetClients",
                columns: table => new
                {
                    ClientId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetClients", x => x.ClientId);
                });

            migrationBuilder.CreateTable(
                name: "AspNetClientClaims",
                columns: table => new
                {
                    ClientId = table.Column<string>(nullable: false),
                    ClaimType = table.Column<string>(nullable: false),
                    fk_AspNetClientClaims_AspNetClient = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetClientClaims", x => new { x.ClientId, x.ClaimType });
                    table.ForeignKey(
                        name: "FK_AspNetClientClaims_AspNetClients_fk_AspNetClientClaims_AspNetClient",
                        column: x => x.fk_AspNetClientClaims_AspNetClient,
                        principalTable: "AspNetClients",
                        principalColumn: "ClientId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetClientClaims_ClaimType",
                table: "AspNetClientClaims",
                column: "ClaimType");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetClientClaims_fk_AspNetClientClaims_AspNetClient",
                table: "AspNetClientClaims",
                column: "fk_AspNetClientClaims_AspNetClient");

            migrationBuilder.Sql(File.ReadAllText("Data\\GetUserClientClaims.sql"));

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetClaims");

            migrationBuilder.DropTable(
                name: "AspNetClientClaims");

            migrationBuilder.DropTable(
                name: "AspNetClients");
        }
    }
}
