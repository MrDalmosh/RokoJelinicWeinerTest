using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RokoJelinicWeinerTest.Data.Migrations
{
    /// <inheritdoc />
    public partial class PartnersPolicies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PartnersPolicies",
                columns: table => new
                {
                    PartnerId = table.Column<int>(nullable: false),
                    PolicyNumber = table.Column<string>(maxLength:15, nullable: false)
                },
                constraints:
                table =>
                {
                    table.PrimaryKey("PK_PartnersPolicies", x => new { x.PartnerId, x.PolicyNumber });
                    table.ForeignKey(
                        name: "FK_PartnersPolicies_Partners",
                        column: x => x.PartnerId,
                        principalTable: "Partners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PartnersPolicies_Policies",
                        column: x => x.PolicyNumber,
                        principalTable: "Policies",
                        principalColumn: "PolicyNumber",
                        onDelete: ReferentialAction.Cascade);

                }
                );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PartnersPolicies");
        }
    }
}
