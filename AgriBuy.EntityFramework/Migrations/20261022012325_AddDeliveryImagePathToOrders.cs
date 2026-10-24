using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgriBuy.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class AddDeliveryImagePathToOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeliveryImagePath",
                table: "Orders",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliveryImagePath",
                table: "Orders");
        }
    }
}
