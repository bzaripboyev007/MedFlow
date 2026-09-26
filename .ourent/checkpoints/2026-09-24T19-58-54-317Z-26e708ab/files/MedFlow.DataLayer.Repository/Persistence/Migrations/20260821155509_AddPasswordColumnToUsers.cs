using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedFlow.DataLayer.Repository.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordColumnToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PASSWORD",
                schema: "IDM",
                table: "USERS",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PASSWORD",
                schema: "IDM",
                table: "USERS");
        }
    }
}
