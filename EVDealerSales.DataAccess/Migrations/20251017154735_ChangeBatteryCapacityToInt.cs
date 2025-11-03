using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EVDealerSales.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ChangeBatteryCapacityToInt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "BatteryCapacity",
                table: "Vehicles",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "BatteryCapacity",
                table: "Vehicles",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
