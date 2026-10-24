using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgriBuy.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationsAndDeliveryProof : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShoppingCarts_Users_UserId1",
                table: "ShoppingCarts");

            migrationBuilder.DropForeignKey(
                name: "FK_Stores_Users_UserId1",
                table: "Stores");

            migrationBuilder.DropIndex(
                name: "IX_Stores_UserId1",
                table: "Stores");

            migrationBuilder.DropIndex(
                name: "IX_ShoppingCarts_UserId1",
                table: "ShoppingCarts");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "ShoppingCarts");

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    OrderId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    Message = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsRead = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OrderDeliveryProofs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    OrderId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UploadedByUserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ImagePath = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UploadedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderDeliveryProofs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderDeliveryProofs_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderDeliveryProofs_Users_UploadedByUserId",
                        column: x => x.UploadedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_IsRead_CreatedAt",
                table: "Notifications",
                columns: new[] { "UserId", "IsRead", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_OrderDeliveryProofs_OrderId",
                table: "OrderDeliveryProofs",
                column: "OrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderDeliveryProofs_UploadedByUserId",
                table: "OrderDeliveryProofs",
                column: "UploadedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "OrderDeliveryProofs");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId1",
                table: "Stores",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId1",
                table: "ShoppingCarts",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.UpdateData(
                table: "Stores",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "UserId1",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_Stores_UserId1",
                table: "Stores",
                column: "UserId1",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingCarts_UserId1",
                table: "ShoppingCarts",
                column: "UserId1",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ShoppingCarts_Users_UserId1",
                table: "ShoppingCarts",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Stores_Users_UserId1",
                table: "Stores",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
