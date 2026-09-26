using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MedFlow.DataLayer.Repository.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOtpCodeAndTelegramLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OTP_CODES",
                schema: "IDM",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", precision: 19, nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PHONE_NUMBER = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    CODE = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    EXPIRES_AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IS_USED = table.Column<bool>(type: "boolean", nullable: false),
                    VERIFIED_AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ATTEMPT_COUNT = table.Column<int>(type: "integer", nullable: false),
                    CREATED_AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OTP_CODES", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "TELEGRAM_LINKS",
                schema: "IDM",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", precision: 19, nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PHONE_NUMBER = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    CHAT_ID = table.Column<long>(type: "bigint", precision: 19, nullable: false),
                    TELEGRAM_USERNAME = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LINKED_AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TELEGRAM_LINKS", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OTP_CODES_PHONE_NUMBER_CREATED_AT",
                schema: "IDM",
                table: "OTP_CODES",
                columns: new[] { "PHONE_NUMBER", "CREATED_AT" });

            migrationBuilder.CreateIndex(
                name: "IX_TELEGRAM_LINKS_CHAT_ID",
                schema: "IDM",
                table: "TELEGRAM_LINKS",
                column: "CHAT_ID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TELEGRAM_LINKS_PHONE_NUMBER",
                schema: "IDM",
                table: "TELEGRAM_LINKS",
                column: "PHONE_NUMBER",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OTP_CODES",
                schema: "IDM");

            migrationBuilder.DropTable(
                name: "TELEGRAM_LINKS",
                schema: "IDM");
        }
    }
}
