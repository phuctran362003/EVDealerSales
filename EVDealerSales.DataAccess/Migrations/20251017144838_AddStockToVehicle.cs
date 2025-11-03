using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EVDealerSales.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddStockToVehicle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Stock",
                table: "Vehicles",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Stock",
                table: "Vehicles");
        }
    }
}
