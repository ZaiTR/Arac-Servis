using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AracServis.Migrations
{
    /// <inheritdoc />
    public partial class AddIhtiyaclarToRandevu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Ihtiyaclar",
                table: "Randevular",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ihtiyaclar",
                table: "Randevular");
        }
    }
}
