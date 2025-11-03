using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EVDealerSales.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDeliveryFlowWithCustomerRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Deliveries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StaffNotes",
                table: "Deliveries",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Deliveries");

            migrationBuilder.DropColumn(
                name: "StaffNotes",
                table: "Deliveries");
        }
    }
}
