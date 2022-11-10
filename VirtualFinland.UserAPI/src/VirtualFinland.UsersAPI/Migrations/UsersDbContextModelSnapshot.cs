﻿// <auto-generated />
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using VirtualFinland.UserAPI.Data;

#nullable disable

namespace VirtualFinland.UserAPI.Migrations
{
    [DbContext(typeof(UsersDbContext))]
    partial class UsersDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.9")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("VirtualFinland.UserAPI.Models.UsersDatabase.ExternalIdentity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("IdentityId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Issuer")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("Modified")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.ToTable("ExternalIdentities");

                    b.HasData(
                        new
                        {
                            Id = new Guid("1bcf54d5-7d87-4d79-973d-9fec4e2b626b"),
                            Created = new DateTime(2022, 11, 10, 9, 31, 24, 968, DateTimeKind.Utc).AddTicks(5420),
                            IdentityId = "709e02eb-9215-4665-890b-9b508e9cc909",
                            Issuer = "3ab418ba-8bd0-4072-b74e-33b8eed26d9b",
                            Modified = new DateTime(2022, 11, 10, 9, 31, 24, 968, DateTimeKind.Utc).AddTicks(5420),
                            UserId = new Guid("5a8af4b4-8cb4-44ac-8291-010614601719")
                        });
                });

            modelBuilder.Entity("VirtualFinland.UserAPI.Models.UsersDatabase.SearchProfile", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("IsDefault")
                        .HasColumnType("boolean");

                    b.Property<List<string>>("JobTitles")
                        .HasColumnType("text[]");

                    b.Property<DateTime>("Modified")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<List<string>>("Regions")
                        .HasColumnType("text[]");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.ToTable("SearchProfiles");
                });

            modelBuilder.Entity("VirtualFinland.UserAPI.Models.UsersDatabase.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Address")
                        .HasMaxLength(512)
                        .HasColumnType("character varying(512)");

                    b.Property<string>("CitizenshipCode")
                        .HasMaxLength(10)
                        .HasColumnType("character varying(10)");

                    b.Property<string>("CountryOfBirthCode")
                        .HasMaxLength(10)
                        .HasColumnType("character varying(10)");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateOnly?>("DateOfBirth")
                        .HasColumnType("date");

                    b.Property<string>("FirstName")
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<string>("Gender")
                        .HasMaxLength(10)
                        .HasColumnType("character varying(10)");

                    b.Property<bool>("ImmigrationDataConsent")
                        .HasColumnType("boolean");

                    b.Property<bool>("JobsDataConsent")
                        .HasColumnType("boolean");

                    b.Property<string>("LastName")
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<DateTime>("Modified")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("NativeLanguageCode")
                        .HasMaxLength(10)
                        .HasColumnType("character varying(10)");

                    b.Property<string>("OccupationCode")
                        .HasMaxLength(10)
                        .HasColumnType("character varying(10)");

                    b.HasKey("Id");

                    b.ToTable("Users");

                    b.HasData(
                        new
                        {
                            Id = new Guid("5a8af4b4-8cb4-44ac-8291-010614601719"),
                            Created = new DateTime(2022, 11, 10, 9, 31, 24, 968, DateTimeKind.Utc).AddTicks(5300),
                            FirstName = "WarmUpUser",
                            ImmigrationDataConsent = false,
                            JobsDataConsent = false,
                            LastName = "WarmUpUser",
                            Modified = new DateTime(2022, 11, 10, 9, 31, 24, 968, DateTimeKind.Utc).AddTicks(5300)
                        });
                });
#pragma warning restore 612, 618
        }
    }
}
