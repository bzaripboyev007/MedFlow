using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MedFlow.DataLayer.Repository.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingsAndNursePricePerVisit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // IDM.USERS.PASSWORD ustuni ataylab o'chirilmaydi: entity'dan olib tashlangan bo'lsa ham,
            // bazadagi mavjud ma'lumot saqlanib qoladi (EF ortiqcha ustunga e'tibor bermaydi).
            migrationBuilder.AddColumn<decimal>(
                name: "PRICE_PER_VISIT",
                schema: "HRM",
                table: "NURSES",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "BOOKINGS",
                schema: "CLINIC",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", precision: 19, nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FULL_NAME = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PHONE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    PATIENT_TYPE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    SERVICE_TYPE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ADDRESS = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LANDMARK = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    NURSE_ID = table.Column<int>(type: "integer", precision: 10, nullable: false),
                    BOOKING_DATE = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    BOOKING_TIME = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    PRESCRIPTION_FILE = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    PRESCRIPTION_URL = table.Column<string>(type: "text", nullable: true),
                    STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    TOTAL_PRICE = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CREATED_AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BOOKINGS", x => x.ID);
                    table.ForeignKey(
                        name: "FK_BOOKINGS_NURSES_NURSE_ID",
                        column: x => x.NURSE_ID,
                        principalSchema: "HRM",
                        principalTable: "NURSES",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BOOKINGS_CREATED_AT",
                schema: "CLINIC",
                table: "BOOKINGS",
                column: "CREATED_AT");

            migrationBuilder.CreateIndex(
                name: "IX_BOOKINGS_NURSE_ID_BOOKING_DATE",
                schema: "CLINIC",
                table: "BOOKINGS",
                columns: new[] { "NURSE_ID", "BOOKING_DATE" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BOOKINGS",
                schema: "CLINIC");

            migrationBuilder.DropColumn(
                name: "PRICE_PER_VISIT",
                schema: "HRM",
                table: "NURSES");
        }
    }
}
