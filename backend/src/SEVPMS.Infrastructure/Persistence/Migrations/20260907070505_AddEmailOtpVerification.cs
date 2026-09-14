using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SEVPMS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailOtpVerification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EmailVerificationFailedAttempts",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "EmailVerificationOtpExpiresAtUtc",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmailVerificationOtpHash",
                table: "Users",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EmailVerificationOtpSentAtUtc",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EmailVerifiedAtUtc",
                table: "Users",
                type: "datetime2",
                nullable: true);

            // Existing users are treated as already verified.
            migrationBuilder.Sql(
                """
                UPDATE [Users]
                SET [EmailVerifiedAtUtc] = SYSUTCDATETIME()
                WHERE [EmailVerifiedAtUtc] IS NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailVerificationFailedAttempts",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EmailVerificationOtpExpiresAtUtc",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EmailVerificationOtpHash",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EmailVerificationOtpSentAtUtc",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EmailVerifiedAtUtc",
                table: "Users");
        }
    }
}