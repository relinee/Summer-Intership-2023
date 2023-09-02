using System;
using Fuse8_ByteMinds.SummerSchool.InternalApi.DbContexts.Entities;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "cur");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:uuid-ossp", ",,");

            migrationBuilder.CreateTable(
                name: "cache_tasks",
                schema: "cur",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    status = table.Column<int>(type: "integer", nullable: false),
                    new_base_currency = table.Column<string>(type: "text", nullable: false),
                    last_updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cache_tasks", x => x.id);
                    table.CheckConstraint("CK_cache_tasks_status_Enum", "status IN (0, 1, 2, 3, 4)");
                });

            migrationBuilder.CreateTable(
                name: "currencies",
                schema: "cur",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    base_currency = table.Column<string>(type: "text", nullable: false),
                    currencies = table.Column<CurrencyRate[]>(type: "jsonb", nullable: false),
                    date_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_currencies", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "settings",
                schema: "cur",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    default_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_settings", x => x.id);
                    table.CheckConstraint("CK_settings_default_currency_MinLength", "LENGTH(default_currency) >= 3");
                });

            migrationBuilder.CreateIndex(
                name: "ix_cache_tasks_id_last_updated",
                schema: "cur",
                table: "cache_tasks",
                columns: new[] { "id", "last_updated" });

            migrationBuilder.CreateIndex(
                name: "ix_currencies_base_currency_date_time",
                schema: "cur",
                table: "currencies",
                columns: new[] { "base_currency", "date_time" });

            migrationBuilder.CreateIndex(
                name: "ix_settings_default_currency",
                schema: "cur",
                table: "settings",
                column: "default_currency",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cache_tasks",
                schema: "cur");

            migrationBuilder.DropTable(
                name: "currencies",
                schema: "cur");

            migrationBuilder.DropTable(
                name: "settings",
                schema: "cur");
        }
    }
}
