using Microsoft.EntityFrameworkCore.Migrations;

namespace RokoJelinicWeinerTest.Data.Migrations
{
    /// <inheritdoc />
    public partial class Partners : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Partners",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(maxLength: 255, nullable: false),
                    LastName = table.Column<string>(maxLength: 255, nullable: false),
                    Address = table.Column<string>(maxLength: 255, nullable: true),
                    PartnerNumber = table.Column<string>(maxLength: 20, nullable: false),
                    CroatianPIN = table.Column<string>(maxLength: 11, nullable: true),
                    PartnerTypeId = table.Column<int>(nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(nullable: false),
                    CreateByUser = table.Column<string>(maxLength: 255, nullable: false),
                    IsForeign = table.Column<bool>(nullable: false),
                    ExternalCode = table.Column<string>(maxLength: 20, nullable: false),
                    Gender = table.Column<string>(maxLength: 1, nullable: false)

                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Partners", x => x.Id);
                }
                );

            migrationBuilder.CreateIndex(
               name: "Unique_ExternalCode",
               table: "Partners",
               column: "ExternalCode",
               unique: true
           );

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Partners");
        }
    }
}
