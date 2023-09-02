using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCurrencySettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var entity = migrationBuilder.Sql(@"
                DO $$ 
                BEGIN
                    IF NOT EXISTS 
                        (SELECT * FROM currency_api.cur.settings WHERE Id = 1)
                    THEN
                        INSERT INTO currency_api.cur.settings (id, default_currency)
                        VALUES (1, 'USD');
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "cur",
                table: "settings",
                keyColumns: new []{"id"},
                keyValues: new object[] { 1 }
            );
        }
    }
}
