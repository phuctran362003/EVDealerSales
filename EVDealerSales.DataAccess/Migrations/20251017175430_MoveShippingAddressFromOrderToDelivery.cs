using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EVDealerSales.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class MoveShippingAddressFromOrderToDelivery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShippingAddress",
                table: "Orders");

            migrationBuilder.AddColumn<string>(
                name: "ShippingAddress",
                table: "Deliveries",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShippingAddress",
                table: "Deliveries");

            migrationBuilder.AddColumn<string>(
                name: "ShippingAddress",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
