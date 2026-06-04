using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
        table: "Users",
        columns: new[]
        {
            "Id",
            "Username",
            "FirstName",
            "SecondName",
            "ThirdName",
            "Email",
            "NationalNumber",
            "PasswordHash",
            "PhoneNumber",
            "City",
            "Role"
        },
        values: new object[]
        {
            1,
            "Admin",
            "abdallah",
            "ahmed",
            "abdelmonem",
            "abdallah@gmail.com",
            "74854152632541",
            BCrypt.Net.BCrypt.HashPassword("Admin123"),
            "01234512342",
            "cairo",
            "Admin"
        });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
            table: "Users",
            keyColumn: "Id",
            keyValue: 1);
        }
    }
}
