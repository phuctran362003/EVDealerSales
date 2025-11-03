using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EVDealerSales.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTestDriveEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "TestDrives",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<DateTime>(
                name: "CanceledAt",
                table: "TestDrives",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                table: "TestDrives",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "TestDrives",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ConfirmedAt",
                table: "TestDrives",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CanceledAt",
                table: "TestDrives");

            migrationBuilder.DropColumn(
                name: "CancellationReason",
                table: "TestDrives");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "TestDrives");

            migrationBuilder.DropColumn(
                name: "ConfirmedAt",
                table: "TestDrives");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "TestDrives",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
