using Microsoft.EntityFrameworkCore.Migrations;

namespace RokoJelinicWeinerTest.Data.Migrations
{
    /// <inheritdoc />
    public partial class Policies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Policies",
                columns: table => new
                {
                    PolicyNumber = table.Column<string>(maxLength:15,nullable: false),
                    PolicyPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Policies", x => x.PolicyNumber);
                }
                );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Policies");
        }
    }
}
