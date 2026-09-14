using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SEVPMS.Infrastructure.Persistence.Migrations
{
    public partial class AlignEventCategoryForeignKey : Migration
    {
        protected override void Up(
            MigrationBuilder migrationBuilder)
        {
            // ---------------------------------------------------------
            // 1. Add CategoryId as nullable first.
            //    We cannot make it required until old Category strings
            //    have been safely mapped.
            // ---------------------------------------------------------
            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId",
                table: "Events",
                type: "uniqueidentifier",
                nullable: true);

            // ---------------------------------------------------------
            // 2. Preserve all legacy category strings.
            //
            // Existing EventCategories:
            // - Match old Events.Category against Name OR Code.
            //
            // Missing categories:
            // - Create a master EventCategory automatically.
            //
            // Empty legacy values:
            // - Preserve under "Legacy / Uncategorized".
            // ---------------------------------------------------------
            migrationBuilder.Sql(
                """
                INSERT INTO EventCategories
                (
                    Id,
                    Name,
                    Code,
                    IsActive,
                    CreatedAtUtc,
                    UpdatedAtUtc
                )
                SELECT
                    NEWID(),
                    source.CategoryName,
                    CONCAT(
                        'LEGACY-',
                        REPLACE(
                            CONVERT(varchar(36), NEWID()),
                            '-',
                            ''
                        )
                    ),
                    1,
                    SYSUTCDATETIME(),
                    NULL
                FROM
                (
                    SELECT DISTINCT
                        CASE
                            WHEN Category IS NULL
                                 OR LTRIM(RTRIM(Category)) = ''
                            THEN 'Legacy / Uncategorized'
                            ELSE LTRIM(RTRIM(Category))
                        END AS CategoryName
                    FROM Events
                ) AS source
                WHERE NOT EXISTS
                (
                    SELECT 1
                    FROM EventCategories existing
                    WHERE
                        existing.Name = source.CategoryName
                        OR existing.Code = source.CategoryName
                );
                """);

            // ---------------------------------------------------------
            // 3. Map every Event to its EventCategory.
            //
            // Prefer exact Name match over Code match.
            // ---------------------------------------------------------
            migrationBuilder.Sql(
                """
                UPDATE e
                SET CategoryId = matched.Id
                FROM Events e
                CROSS APPLY
                (
                    SELECT TOP (1)
                        c.Id
                    FROM EventCategories c
                    WHERE
                        c.Name =
                            CASE
                                WHEN e.Category IS NULL
                                     OR LTRIM(RTRIM(e.Category)) = ''
                                THEN 'Legacy / Uncategorized'
                                ELSE LTRIM(RTRIM(e.Category))
                            END
                        OR
                        c.Code =
                            CASE
                                WHEN e.Category IS NULL
                                     OR LTRIM(RTRIM(e.Category)) = ''
                                THEN 'Legacy / Uncategorized'
                                ELSE LTRIM(RTRIM(e.Category))
                            END
                    ORDER BY
                        CASE
                            WHEN c.Name =
                                CASE
                                    WHEN e.Category IS NULL
                                         OR LTRIM(RTRIM(e.Category)) = ''
                                    THEN 'Legacy / Uncategorized'
                                    ELSE LTRIM(RTRIM(e.Category))
                                END
                            THEN 0
                            ELSE 1
                        END,
                        c.CreatedAtUtc,
                        c.Id
                ) AS matched;
                """);

            // ---------------------------------------------------------
            // 4. Safety guard.
            //    Never drop legacy Category if any Event was not mapped.
            // ---------------------------------------------------------
            migrationBuilder.Sql(
                """
                IF EXISTS
                (
                    SELECT 1
                    FROM Events
                    WHERE CategoryId IS NULL
                )
                BEGIN
                    THROW 51000,
                        'Unable to map one or more Events to EventCategories.',
                        1;
                END;
                """);

            // ---------------------------------------------------------
            // 5. CategoryId can now safely become required.
            // ---------------------------------------------------------
            migrationBuilder.AlterColumn<Guid>(
                name: "CategoryId",
                table: "Events",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            // ---------------------------------------------------------
            // 6. Old string column is no longer needed.
            // ---------------------------------------------------------
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Events");

            // ---------------------------------------------------------
            // 7. Final-schema indexes.
            // ---------------------------------------------------------
            migrationBuilder.CreateIndex(
                name: "IX_Events_CategoryId",
                table: "Events",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name:
                    "IX_Events_StartAtUtc_Status_CategoryId_VenueId",
                table: "Events",
                columns: new[]
                {
                    "StartAtUtc",
                    "Status",
                    "CategoryId",
                    "VenueId"
                });

            // ---------------------------------------------------------
            // 8. Proper FK:
            //
            // Events.CategoryId
            //      -> EventCategories.Id
            // ---------------------------------------------------------
            migrationBuilder.AddForeignKey(
                name:
                    "FK_Events_EventCategories_CategoryId",
                table: "Events",
                column: "CategoryId",
                principalTable: "EventCategories",
                principalColumn: "Id",
                onDelete:
                    ReferentialAction.Restrict);
        }

        protected override void Down(
            MigrationBuilder migrationBuilder)
        {
            // ---------------------------------------------------------
            // Reverse FK/index changes first.
            // ---------------------------------------------------------
            migrationBuilder.DropForeignKey(
                name:
                    "FK_Events_EventCategories_CategoryId",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_CategoryId",
                table: "Events");

            migrationBuilder.DropIndex(
                name:
                    "IX_Events_StartAtUtc_Status_CategoryId_VenueId",
                table: "Events");

            // ---------------------------------------------------------
            // Restore old Category string column as nullable first.
            // ---------------------------------------------------------
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Events",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            // ---------------------------------------------------------
            // Restore category names from master table.
            // ---------------------------------------------------------
            migrationBuilder.Sql(
                """
                UPDATE e
                SET Category = c.Name
                FROM Events e
                INNER JOIN EventCategories c
                    ON c.Id = e.CategoryId;
                """);

            // Safety guard before making Category required.
            migrationBuilder.Sql(
                """
                IF EXISTS
                (
                    SELECT 1
                    FROM Events
                    WHERE Category IS NULL
                       OR LTRIM(RTRIM(Category)) = ''
                )
                BEGIN
                    THROW 51001,
                        'Unable to restore legacy Events.Category values.',
                        1;
                END;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "Events",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            // ---------------------------------------------------------
            // Remove new FK column.
            // ---------------------------------------------------------
            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Events");

            // Note:
            // Legacy categories created during Up() are intentionally
            // NOT deleted here because another record could reference
            // them later. Keeping master data is safer than deleting it.
        }
    }
}