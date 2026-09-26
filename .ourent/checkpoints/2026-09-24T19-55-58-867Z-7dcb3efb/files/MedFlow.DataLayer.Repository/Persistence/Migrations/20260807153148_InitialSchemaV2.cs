using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MedFlow.DataLayer.Repository.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchemaV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "PHARMACY");

            migrationBuilder.EnsureSchema(
                name: "HRM");

            migrationBuilder.EnsureSchema(
                name: "ADM");

            migrationBuilder.EnsureSchema(
                name: "PAYMENT");

            migrationBuilder.EnsureSchema(
                name: "CLINIC");

            migrationBuilder.EnsureSchema(
                name: "IDM");

            migrationBuilder.CreateTable(
                name: "MEDICINES",
                schema: "PHARMACY",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", precision: 10, nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NAME = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    UNIT = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PRICE = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    STOCK_QTY = table.Column<int>(type: "integer", nullable: false),
                    MIN_STOCK_LEVEL = table.Column<int>(type: "integer", nullable: false),
                    EXPIRY_DATE = table.Column<DateTime>(type: "DATE", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MEDICINES", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "NURSES",
                schema: "HRM",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", precision: 10, nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FULL_NAME = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    TYPE = table.Column<int>(type: "integer", nullable: false),
                    GENDER = table.Column<int>(type: "integer", nullable: true),
                    PHONE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    SPECIALIZATION = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    EXPERIENCE_START_DATE = table.Column<DateTime>(type: "DATE", nullable: false),
                    LOGIN = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RATING = table.Column<decimal>(type: "numeric(3,2)", precision: 3, scale: 2, nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    HIRED_AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NURSES", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "PATIENTS",
                schema: "ADM",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", precision: 19, nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FULL_NAME = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    BIRTH_DATE = table.Column<DateTime>(type: "DATE", nullable: false),
                    PHONE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    PARENT_ID = table.Column<long>(type: "bigint", precision: 19, nullable: true),
                    IS_CHILD = table.Column<bool>(type: "boolean", nullable: false),
                    ALLERGIES = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PATIENTS", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PATIENTS_PATIENTS_PARENT_ID",
                        column: x => x.PARENT_ID,
                        principalSchema: "ADM",
                        principalTable: "PATIENTS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SERVICE_TYPES",
                schema: "PAYMENT",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", precision: 10, nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NAME = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PRICE_PER_SESSION = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SERVICE_TYPES", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "PATIENT_MEDICINE_WALLETS",
                schema: "PHARMACY",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", precision: 19, nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PATIENT_ID = table.Column<long>(type: "bigint", precision: 19, nullable: false),
                    MEDICINE_ID = table.Column<int>(type: "integer", precision: 10, nullable: false),
                    PURCHASED_QTY = table.Column<int>(type: "integer", nullable: false),
                    REMAINING_QTY = table.Column<int>(type: "integer", nullable: false),
                    SOURCE = table.Column<int>(type: "integer", nullable: false),
                    PURCHASED_AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PATIENT_MEDICINE_WALLETS", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PATIENT_MEDICINE_WALLETS_MEDICINES_MEDICINE_ID",
                        column: x => x.MEDICINE_ID,
                        principalSchema: "PHARMACY",
                        principalTable: "MEDICINES",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PATIENT_MEDICINE_WALLETS_PATIENTS_PATIENT_ID",
                        column: x => x.PATIENT_ID,
                        principalSchema: "ADM",
                        principalTable: "PATIENTS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PRESCRIPTIONS",
                schema: "CLINIC",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", precision: 19, nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PATIENT_ID = table.Column<long>(type: "bigint", precision: 19, nullable: false),
                    FILE_URL = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RECOGNIZED_TEXT = table.Column<string>(type: "text", nullable: true),
                    VERIFIED_BY_NURSE_ID = table.Column<int>(type: "integer", precision: 10, nullable: true),
                    VERIFIED_AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CREATED_AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PRESCRIPTIONS", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PRESCRIPTIONS_NURSES_VERIFIED_BY_NURSE_ID",
                        column: x => x.VERIFIED_BY_NURSE_ID,
                        principalSchema: "HRM",
                        principalTable: "NURSES",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PRESCRIPTIONS_PATIENTS_PATIENT_ID",
                        column: x => x.PATIENT_ID,
                        principalSchema: "ADM",
                        principalTable: "PATIENTS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "USERS",
                schema: "IDM",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", precision: 19, nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ROLE = table.Column<int>(type: "integer", nullable: false),
                    FULL_NAME = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PHONE_NUMBER = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    PASSWORD_HASH = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    LINKED_PATIENT_ID = table.Column<long>(type: "bigint", precision: 19, nullable: true),
                    LINKED_NURSE_ID = table.Column<int>(type: "integer", precision: 10, nullable: true),
                    FAILED_LOGIN_COUNT = table.Column<int>(type: "integer", nullable: false),
                    LOCKED_UNTIL = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LAST_LOGIN_AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CREATED_AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USERS", x => x.ID);
                    table.ForeignKey(
                        name: "FK_USERS_NURSES_LINKED_NURSE_ID",
                        column: x => x.LINKED_NURSE_ID,
                        principalSchema: "HRM",
                        principalTable: "NURSES",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_USERS_PATIENTS_LINKED_PATIENT_ID",
                        column: x => x.LINKED_PATIENT_ID,
                        principalSchema: "ADM",
                        principalTable: "PATIENTS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TREATMENT_COURSES",
                schema: "CLINIC",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", precision: 19, nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PATIENT_ID = table.Column<long>(type: "bigint", precision: 19, nullable: false),
                    PRESCRIPTION_ID = table.Column<long>(type: "bigint", precision: 19, nullable: false),
                    PRIMARY_NURSE_ID = table.Column<int>(type: "integer", precision: 10, nullable: false),
                    SERVICE_TYPE_ID = table.Column<int>(type: "integer", precision: 10, nullable: false),
                    LOCATION_TYPE = table.Column<int>(type: "integer", nullable: false),
                    ADDRESS = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DURATION_DAYS = table.Column<int>(type: "integer", nullable: false),
                    TIMES_PER_DAY = table.Column<int>(type: "integer", nullable: false),
                    PREFERRED_TIMES_MINUTES = table.Column<int[]>(type: "integer[]", nullable: true),
                    START_DATE = table.Column<DateTime>(type: "DATE", nullable: false),
                    TOTAL_SESSIONS = table.Column<int>(type: "integer", nullable: false),
                    COMPLETED_SESSIONS = table.Column<int>(type: "integer", nullable: false),
                    STATUS = table.Column<int>(type: "integer", nullable: false),
                    CREATED_AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TREATMENT_COURSES", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TREATMENT_COURSES_NURSES_PRIMARY_NURSE_ID",
                        column: x => x.PRIMARY_NURSE_ID,
                        principalSchema: "HRM",
                        principalTable: "NURSES",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TREATMENT_COURSES_PATIENTS_PATIENT_ID",
                        column: x => x.PATIENT_ID,
                        principalSchema: "ADM",
                        principalTable: "PATIENTS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TREATMENT_COURSES_PRESCRIPTIONS_PRESCRIPTION_ID",
                        column: x => x.PRESCRIPTION_ID,
                        principalSchema: "CLINIC",
                        principalTable: "PRESCRIPTIONS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TREATMENT_COURSES_SERVICE_TYPES_SERVICE_TYPE_ID",
                        column: x => x.SERVICE_TYPE_ID,
                        principalSchema: "PAYMENT",
                        principalTable: "SERVICE_TYPES",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "REFRESH_TOKENS",
                schema: "IDM",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", precision: 19, nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    USER_ID = table.Column<long>(type: "bigint", precision: 19, nullable: false),
                    TOKEN_HASH = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    EXPIRES_AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    REVOKED_AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CREATED_AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_REFRESH_TOKENS", x => x.ID);
                    table.ForeignKey(
                        name: "FK_REFRESH_TOKENS_USERS_USER_ID",
                        column: x => x.USER_ID,
                        principalSchema: "IDM",
                        principalTable: "USERS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PAYMENTS",
                schema: "PAYMENT",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", precision: 19, nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PATIENT_ID = table.Column<long>(type: "bigint", precision: 19, nullable: false),
                    COURSE_ID = table.Column<long>(type: "bigint", precision: 19, nullable: false),
                    SERVICE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    MEDICINE_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TRANSPORT_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TOTAL_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    METHOD = table.Column<int>(type: "integer", nullable: false),
                    PAID_AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PAYMENTS", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PAYMENTS_PATIENTS_PATIENT_ID",
                        column: x => x.PATIENT_ID,
                        principalSchema: "ADM",
                        principalTable: "PATIENTS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PAYMENTS_TREATMENT_COURSES_COURSE_ID",
                        column: x => x.COURSE_ID,
                        principalSchema: "CLINIC",
                        principalTable: "TREATMENT_COURSES",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SESSIONS",
                schema: "CLINIC",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", precision: 19, nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    COURSE_ID = table.Column<long>(type: "bigint", precision: 19, nullable: false),
                    SEQ_NO = table.Column<int>(type: "integer", nullable: false),
                    SCHEDULED_AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ACTUAL_START_AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ACTUAL_END_AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    NURSE_ID = table.Column<int>(type: "integer", precision: 10, nullable: false),
                    STATUS = table.Column<int>(type: "integer", nullable: false),
                    NOTES = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SESSIONS", x => x.ID);
                    table.ForeignKey(
                        name: "FK_SESSIONS_NURSES_NURSE_ID",
                        column: x => x.NURSE_ID,
                        principalSchema: "HRM",
                        principalTable: "NURSES",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SESSIONS_TREATMENT_COURSES_COURSE_ID",
                        column: x => x.COURSE_ID,
                        principalSchema: "CLINIC",
                        principalTable: "TREATMENT_COURSES",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MEDICINE_ISSUES",
                schema: "PHARMACY",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", precision: 19, nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MEDICINE_ID = table.Column<int>(type: "integer", precision: 10, nullable: false),
                    PATIENT_ID = table.Column<long>(type: "bigint", precision: 19, nullable: false),
                    NURSE_ID = table.Column<int>(type: "integer", precision: 10, nullable: false),
                    SESSION_ID = table.Column<long>(type: "bigint", precision: 19, nullable: false),
                    QUANTITY = table.Column<int>(type: "integer", nullable: false),
                    SOURCE = table.Column<int>(type: "integer", nullable: false),
                    PRICE_AT_ISSUE = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ISSUED_AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MEDICINE_ISSUES", x => x.ID);
                    table.ForeignKey(
                        name: "FK_MEDICINE_ISSUES_MEDICINES_MEDICINE_ID",
                        column: x => x.MEDICINE_ID,
                        principalSchema: "PHARMACY",
                        principalTable: "MEDICINES",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MEDICINE_ISSUES_NURSES_NURSE_ID",
                        column: x => x.NURSE_ID,
                        principalSchema: "HRM",
                        principalTable: "NURSES",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MEDICINE_ISSUES_PATIENTS_PATIENT_ID",
                        column: x => x.PATIENT_ID,
                        principalSchema: "ADM",
                        principalTable: "PATIENTS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MEDICINE_ISSUES_SESSIONS_SESSION_ID",
                        column: x => x.SESSION_ID,
                        principalSchema: "CLINIC",
                        principalTable: "SESSIONS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MEDICINE_ISSUES_MEDICINE_ID",
                schema: "PHARMACY",
                table: "MEDICINE_ISSUES",
                column: "MEDICINE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MEDICINE_ISSUES_NURSE_ID",
                schema: "PHARMACY",
                table: "MEDICINE_ISSUES",
                column: "NURSE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MEDICINE_ISSUES_PATIENT_ID",
                schema: "PHARMACY",
                table: "MEDICINE_ISSUES",
                column: "PATIENT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MEDICINE_ISSUES_SESSION_ID",
                schema: "PHARMACY",
                table: "MEDICINE_ISSUES",
                column: "SESSION_ID");

            migrationBuilder.CreateIndex(
                name: "IX_NURSES_LOGIN",
                schema: "HRM",
                table: "NURSES",
                column: "LOGIN",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PATIENT_MEDICINE_WALLETS_MEDICINE_ID",
                schema: "PHARMACY",
                table: "PATIENT_MEDICINE_WALLETS",
                column: "MEDICINE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_PATIENT_MEDICINE_WALLETS_PATIENT_ID_MEDICINE_ID",
                schema: "PHARMACY",
                table: "PATIENT_MEDICINE_WALLETS",
                columns: new[] { "PATIENT_ID", "MEDICINE_ID" });

            migrationBuilder.CreateIndex(
                name: "IX_PATIENTS_PARENT_ID",
                schema: "ADM",
                table: "PATIENTS",
                column: "PARENT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_PATIENTS_PHONE",
                schema: "ADM",
                table: "PATIENTS",
                column: "PHONE");

            migrationBuilder.CreateIndex(
                name: "IX_PAYMENTS_COURSE_ID",
                schema: "PAYMENT",
                table: "PAYMENTS",
                column: "COURSE_ID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PAYMENTS_PATIENT_ID",
                schema: "PAYMENT",
                table: "PAYMENTS",
                column: "PATIENT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_PRESCRIPTIONS_PATIENT_ID",
                schema: "CLINIC",
                table: "PRESCRIPTIONS",
                column: "PATIENT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_PRESCRIPTIONS_VERIFIED_BY_NURSE_ID",
                schema: "CLINIC",
                table: "PRESCRIPTIONS",
                column: "VERIFIED_BY_NURSE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_REFRESH_TOKENS_TOKEN_HASH",
                schema: "IDM",
                table: "REFRESH_TOKENS",
                column: "TOKEN_HASH",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_REFRESH_TOKENS_USER_ID",
                schema: "IDM",
                table: "REFRESH_TOKENS",
                column: "USER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_SESSIONS_COURSE_ID",
                schema: "CLINIC",
                table: "SESSIONS",
                column: "COURSE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_SESSIONS_NURSE_ID_SCHEDULED_AT",
                schema: "CLINIC",
                table: "SESSIONS",
                columns: new[] { "NURSE_ID", "SCHEDULED_AT" });

            migrationBuilder.CreateIndex(
                name: "IX_TREATMENT_COURSES_PATIENT_ID",
                schema: "CLINIC",
                table: "TREATMENT_COURSES",
                column: "PATIENT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_TREATMENT_COURSES_PRESCRIPTION_ID",
                schema: "CLINIC",
                table: "TREATMENT_COURSES",
                column: "PRESCRIPTION_ID");

            migrationBuilder.CreateIndex(
                name: "IX_TREATMENT_COURSES_PRIMARY_NURSE_ID",
                schema: "CLINIC",
                table: "TREATMENT_COURSES",
                column: "PRIMARY_NURSE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_TREATMENT_COURSES_SERVICE_TYPE_ID",
                schema: "CLINIC",
                table: "TREATMENT_COURSES",
                column: "SERVICE_TYPE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_USERS_LINKED_NURSE_ID",
                schema: "IDM",
                table: "USERS",
                column: "LINKED_NURSE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_USERS_LINKED_PATIENT_ID",
                schema: "IDM",
                table: "USERS",
                column: "LINKED_PATIENT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_USERS_PHONE_NUMBER",
                schema: "IDM",
                table: "USERS",
                column: "PHONE_NUMBER",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MEDICINE_ISSUES",
                schema: "PHARMACY");

            migrationBuilder.DropTable(
                name: "PATIENT_MEDICINE_WALLETS",
                schema: "PHARMACY");

            migrationBuilder.DropTable(
                name: "PAYMENTS",
                schema: "PAYMENT");

            migrationBuilder.DropTable(
                name: "REFRESH_TOKENS",
                schema: "IDM");

            migrationBuilder.DropTable(
                name: "SESSIONS",
                schema: "CLINIC");

            migrationBuilder.DropTable(
                name: "MEDICINES",
                schema: "PHARMACY");

            migrationBuilder.DropTable(
                name: "USERS",
                schema: "IDM");

            migrationBuilder.DropTable(
                name: "TREATMENT_COURSES",
                schema: "CLINIC");

            migrationBuilder.DropTable(
                name: "PRESCRIPTIONS",
                schema: "CLINIC");

            migrationBuilder.DropTable(
                name: "SERVICE_TYPES",
                schema: "PAYMENT");

            migrationBuilder.DropTable(
                name: "NURSES",
                schema: "HRM");

            migrationBuilder.DropTable(
                name: "PATIENTS",
                schema: "ADM");
        }
    }
}
