using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RDS_Error_Repication.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Customer",
                columns: table => new
                {
                    UUID = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ExternalId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Surname = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Created = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customer", x => x.UUID);
                });

            migrationBuilder.CreateTable(
                name: "Contact_Information",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Cell = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    CustomerId = table.Column<string>(type: "character varying(50)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contact_Information", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contact_Information_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "UUID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Customer_Favourite_Products",
                columns: table => new
                {
                    CustomerId = table.Column<string>(type: "character varying(70)", maxLength: 70, nullable: false),
                    ProductId = table.Column<string>(type: "character varying(70)", maxLength: 70, nullable: false),
                    StoreId = table.Column<string>(type: "character varying(70)", maxLength: 70, nullable: false),
                    MallId = table.Column<string>(type: "character varying(70)", maxLength: 70, nullable: false),
                    Weight = table.Column<int>(type: "integer", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customer_Favourite_Products", x => new { x.CustomerId, x.StoreId, x.ProductId, x.MallId });
                    table.ForeignKey(
                        name: "FK_Customer_Favourite_Products_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "UUID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Customer_Favourite_Stores",
                columns: table => new
                {
                    CustomerId = table.Column<string>(type: "character varying(70)", maxLength: 70, nullable: false),
                    StoreId = table.Column<string>(type: "character varying(70)", maxLength: 70, nullable: false),
                    MallId = table.Column<string>(type: "character varying(70)", maxLength: 70, nullable: false),
                    Weight = table.Column<int>(type: "integer", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customer_Favourite_Stores", x => new { x.CustomerId, x.StoreId, x.MallId });
                    table.ForeignKey(
                        name: "FK_Customer_Favourite_Stores_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "UUID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomerInterests",
                columns: table => new
                {
                    CustomerUUID = table.Column<string>(type: "character varying(50)", nullable: false),
                    CategoryId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Weight = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerInterests", x => new { x.CategoryId, x.CustomerUUID });
                    table.ForeignKey(
                        name: "FK_CustomerInterests_Customer_CustomerUUID",
                        column: x => x.CustomerUUID,
                        principalTable: "Customer",
                        principalColumn: "UUID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Popi_Info",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TermsAndConditions = table.Column<bool>(type: "boolean", nullable: false),
                    Marketing = table.Column<bool>(type: "boolean", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LastModified = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Popi_Info", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Popi_Info_Customer_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Customer",
                        principalColumn: "UUID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Contact_Information_CustomerId",
                table: "Contact_Information",
                column: "CustomerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerInterests_CustomerUUID",
                table: "CustomerInterests",
                column: "CustomerUUID");

            migrationBuilder.CreateIndex(
                name: "IX_Popi_Info_CreatedBy",
                table: "Popi_Info",
                column: "CreatedBy",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Contact_Information");

            migrationBuilder.DropTable(
                name: "Customer_Favourite_Products");

            migrationBuilder.DropTable(
                name: "Customer_Favourite_Stores");

            migrationBuilder.DropTable(
                name: "CustomerInterests");

            migrationBuilder.DropTable(
                name: "Popi_Info");

            migrationBuilder.DropTable(
                name: "Customer");
        }
    }
}
