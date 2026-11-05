using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BankSystemEF04.Migrations
{
    /// <inheritdoc />
    public partial class SeedBranchesAndManagers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Managers",
                columns: new[] { "Id", "Email", "FullName", "HireDate", "PhoneNumber" },
                values: new object[,]
                {
                    { 1, "khaled.mansour@nationalbank.com", "Khaled Mansour", new DateTime(2015, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "0100000001" },
                    { 2, "rania.youssef@nationalbank.com", "Rania Youssef", new DateTime(2017, 9, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "0100000002" },
                    { 3, "tamer.said@nationalbank.com", "Tamer Said", new DateTime(2019, 1, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "0100000003" }
                });

            migrationBuilder.InsertData(
                table: "Branches",
                columns: new[] { "Code", "Address", "ManagerId", "Name", "PhoneNumber" },
                values: new object[,]
                {
                    { "ALX-01", "25 Corniche Road, Alexandria", 2, "Alexandria Branch", "0335000002" },
                    { "CAI-01", "1 Tahrir Square, Cairo", 1, "Cairo Main Branch", "0225000001" },
                    { "GIZ-01", "10 Pyramids Street, Giza", 3, "Giza Branch", "0235000003" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "Code",
                keyValue: "ALX-01");

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "Code",
                keyValue: "CAI-01");

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "Code",
                keyValue: "GIZ-01");

            migrationBuilder.DeleteData(
                table: "Managers",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Managers",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Managers",
                keyColumn: "Id",
                keyValue: 3);
        }
    }
}
