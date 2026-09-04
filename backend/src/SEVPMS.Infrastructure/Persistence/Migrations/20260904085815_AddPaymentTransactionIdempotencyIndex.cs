using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SEVPMS.Infrastructure.Persistence.Migrations
{
    public partial class AddPaymentTransactionIdempotencyIndex
        : Migration
    {
        protected override void Up(
            MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF EXISTS
                (
                    SELECT 1
                    FROM PaymentTransactions
                    GROUP BY
                        PaymentId,
                        Type,
                        ProviderReference
                    HAVING COUNT(*) > 1
                )
                BEGIN
                    THROW 51010,
                        'Duplicate payment transaction idempotency keys exist. Resolve duplicates before applying the unique index.',
                        1;
                END;
                """);

            migrationBuilder.CreateIndex(
                name:
                    "UX_PaymentTransactions_PaymentId_Type_ProviderReference",
                table:
                    "PaymentTransactions",
                columns: new[]
                {
                    "PaymentId",
                    "Type",
                    "ProviderReference"
                },
                unique: true);
        }

        protected override void Down(
            MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name:
                    "UX_PaymentTransactions_PaymentId_Type_ProviderReference",
                table:
                    "PaymentTransactions");
        }
    }
}