﻿// <auto-generated />
using System;
using Fuse8_ByteMinds.SummerSchool.InternalApi.DbContexts;
using Fuse8_ByteMinds.SummerSchool.InternalApi.DbContexts.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Data.Migrations
{
    [DbContext(typeof(CurrencyRateDbContext))]
    [Migration("20230902211937_Initial")]
    partial class Initial
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("cur")
                .HasAnnotation("ProductVersion", "7.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.HasPostgresExtension(modelBuilder, "uuid-ossp");
            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Fuse8_ByteMinds.SummerSchool.InternalApi.DbContexts.Entities.CacheTask", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id")
                        .HasDefaultValueSql("uuid_generate_v4()");

                    b.Property<DateTimeOffset>("LastUpdated")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("last_updated");

                    b.Property<string>("NewBaseCurrency")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("new_base_currency");

                    b.Property<int>("Status")
                        .HasColumnType("integer")
                        .HasColumnName("status");

                    b.HasKey("Id")
                        .HasName("pk_cache_tasks");

                    b.HasIndex("Id", "LastUpdated")
                        .HasDatabaseName("ix_cache_tasks_id_last_updated");

                    b.ToTable("cache_tasks", "cur", t =>
                        {
                            t.HasCheckConstraint("CK_cache_tasks_status_Enum", "status IN (0, 1, 2, 3, 4)");
                        });
                });

            modelBuilder.Entity("Fuse8_ByteMinds.SummerSchool.InternalApi.DbContexts.Entities.CurrenciesRates", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("BaseCurrency")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("base_currency");

                    b.Property<CurrencyRate[]>("Currencies")
                        .IsRequired()
                        .HasColumnType("jsonb")
                        .HasColumnName("currencies");

                    b.Property<DateTimeOffset>("DateTime")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("date_time");

                    b.HasKey("Id")
                        .HasName("pk_currencies");

                    b.HasIndex("BaseCurrency", "DateTime")
                        .HasDatabaseName("ix_currencies_base_currency_date_time");

                    b.ToTable("currencies", "cur");
                });

            modelBuilder.Entity("Fuse8_ByteMinds.SummerSchool.InternalApi.DbContexts.Entities.CurrencySettings", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("DefaultCurrency")
                        .IsRequired()
                        .HasMaxLength(3)
                        .HasColumnType("character varying(3)")
                        .HasColumnName("default_currency");

                    b.HasKey("Id")
                        .HasName("pk_settings");

                    b.HasIndex("DefaultCurrency")
                        .IsUnique()
                        .HasDatabaseName("ix_settings_default_currency");

                    b.ToTable("settings", "cur", t =>
                        {
                            t.HasCheckConstraint("CK_settings_default_currency_MinLength", "LENGTH(default_currency) >= 3");
                        });
                });
#pragma warning restore 612, 618
        }
    }
}
