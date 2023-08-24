using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "user");

            migrationBuilder.CreateTable(
                name: "favourites_currencies_rates",
                schema: "user",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    base_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_favourites_currencies_rates", x => x.id);
                    table.CheckConstraint("CK_favourites_currencies_rates_base_currency_MinLength", "LENGTH(base_currency) >= 3");
                    table.CheckConstraint("CK_favourites_currencies_rates_currency_MinLength", "LENGTH(currency) >= 3");
                });

            migrationBuilder.CreateTable(
                name: "settings",
                schema: "user",
                columns: table => new
                {
                    default_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    currency_round_count = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.CheckConstraint("CK_settings_default_currency_MinLength", "LENGTH(default_currency) >= 3");
                });

            migrationBuilder.CreateIndex(
                name: "ix_favourites_currencies_rates_currency_base_currency",
                schema: "user",
                table: "favourites_currencies_rates",
                columns: new[] { "currency", "base_currency" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_favourites_currencies_rates_name",
                schema: "user",
                table: "favourites_currencies_rates",
                column: "name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "favourites_currencies_rates",
                schema: "user");

            migrationBuilder.DropTable(
                name: "settings",
                schema: "user");
        }
    }
}
