using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Migrations
{
    /// <inheritdoc />
    public partial class AddingCurrencySettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "user",
                table: "settings",
                columns: new [] { "default_currency", "currency_round_count" },
                values: new object[] { "USD" , 2 }
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "user",
                table: "settings",
                keyColumns: new [] { "default_currency", "currency_round_count" },
                keyValues: new object[] { "USD" , 2 }
            );
        }
    }
}
