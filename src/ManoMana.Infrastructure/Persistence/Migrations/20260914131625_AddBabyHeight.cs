using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManoMana.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBabyHeight : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PredictedHeightCentimeters",
                table: "Predictions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HeightCentimeters",
                table: "Births",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PredictedHeightCentimeters",
                table: "Predictions");

            migrationBuilder.DropColumn(
                name: "HeightCentimeters",
                table: "Births");
        }
    }
}
