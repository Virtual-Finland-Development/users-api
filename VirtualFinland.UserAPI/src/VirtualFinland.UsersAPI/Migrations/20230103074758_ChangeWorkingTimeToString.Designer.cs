// <auto-generated />
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using VirtualFinland.UserAPI.Data;

#nullable disable

namespace VirtualFinland.UserAPI.Migrations
{
    [DbContext(typeof(UsersDbContext))]
    [Migration("20230103074758_ChangeWorkingTimeToString")]
    partial class ChangeWorkingTimeToString
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.9")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("VirtualFinland.UserAPI.Models.UsersDatabase.Certification", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("Modified")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Name")
                        .HasMaxLength(512)
                        .HasColumnType("character varying(512)");

                    b.Property<string>("Type")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.HasKey("Id");

                    b.ToTable("Certifications");
                });

            modelBuilder.Entity("VirtualFinland.UserAPI.Models.UsersDatabase.Education", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("EducationField")
                        .HasMaxLength(4)
                        .HasColumnType("character varying(4)");

                    b.Property<int?>("EducationLevelEnum")
                        .HasColumnType("integer");

                    b.Property<string>("EducationOrganization")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<DateTime?>("GraduationDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("Modified")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.ToTable("Educations");
                });

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
                            Id = new Guid("1e394aa4-1e0a-4f38-86e3-ee95e84e53fd"),
                            Created = new DateTime(2023, 1, 3, 7, 47, 58, 17, DateTimeKind.Utc).AddTicks(520),
                            IdentityId = "bc32a78c-5c0a-4912-9587-35d3977debe2",
                            Issuer = "c9ead4c6-3d8b-47e1-aeb2-602124828743",
                            Modified = new DateTime(2023, 1, 3, 7, 47, 58, 17, DateTimeKind.Utc).AddTicks(530),
                            UserId = new Guid("5a8af4b4-8cb4-44ac-8291-010614601719")
                        });
                });

            modelBuilder.Entity("VirtualFinland.UserAPI.Models.UsersDatabase.Language", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("EscoUri")
                        .HasColumnType("text");

                    b.Property<string>("LanguageCode")
                        .HasMaxLength(3)
                        .HasColumnType("character varying(3)");

                    b.Property<DateTime>("Modified")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int?>("SkillLevelEnum")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("Languages");
                });

            modelBuilder.Entity("VirtualFinland.UserAPI.Models.UsersDatabase.Occupation", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("EscoCode")
                        .HasMaxLength(16)
                        .HasColumnType("character varying(16)");

                    b.Property<string>("EscoUri")
                        .HasColumnType("text");

                    b.Property<DateTime>("Modified")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("NaceCode")
                        .HasMaxLength(7)
                        .HasColumnType("character varying(7)");

                    b.Property<Guid?>("UserId")
                        .HasColumnType("uuid");

                    b.Property<int?>("WorkMonths")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Occupations");
                });

            modelBuilder.Entity("VirtualFinland.UserAPI.Models.UsersDatabase.Permit", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("Modified")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Name")
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<string>("Type")
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.HasKey("Id");

                    b.ToTable("Permits");
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

            modelBuilder.Entity("VirtualFinland.UserAPI.Models.UsersDatabase.Skills", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("EscoUrl")
                        .HasColumnType("text");

                    b.Property<string>("LanguageCode")
                        .HasMaxLength(3)
                        .HasColumnType("character varying(3)");

                    b.Property<DateTime>("Modified")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int?>("SkillLevelEnum")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("Skills");
                });

            modelBuilder.Entity("VirtualFinland.UserAPI.Models.UsersDatabase.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("CitizenshipCode")
                        .HasMaxLength(10)
                        .HasColumnType("character varying(10)");

                    b.Property<string>("City")
                        .HasMaxLength(512)
                        .HasColumnType("character varying(512)");

                    b.Property<string>("Country")
                        .HasMaxLength(512)
                        .HasColumnType("character varying(512)");

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
                        .IsRequired()
                        .HasColumnType("text");

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

                    b.Property<string>("StreetAddress")
                        .HasMaxLength(512)
                        .HasColumnType("character varying(512)");

                    b.Property<string>("ZipCode")
                        .HasMaxLength(5)
                        .HasColumnType("character varying(5)");

                    b.HasKey("Id");

                    b.ToTable("Users");

                    b.HasData(
                        new
                        {
                            Id = new Guid("5a8af4b4-8cb4-44ac-8291-010614601719"),
                            Created = new DateTime(2023, 1, 3, 7, 47, 58, 16, DateTimeKind.Utc).AddTicks(9090),
                            FirstName = "WarmUpUser",
                            Gender = "Other",
                            ImmigrationDataConsent = false,
                            JobsDataConsent = false,
                            LastName = "WarmUpUser",
                            Modified = new DateTime(2023, 1, 3, 7, 47, 58, 16, DateTimeKind.Utc).AddTicks(9090)
                        });
                });

            modelBuilder.Entity("VirtualFinland.UserAPI.Models.UsersDatabase.WorkPreferences", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("EmploymentTypeCode")
                        .HasColumnType("text");

                    b.Property<DateTime>("Modified")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("PreferredMunicipalityEnum")
                        .HasColumnType("text");

                    b.Property<string>("PreferredRegionEnum")
                        .HasColumnType("text");

                    b.Property<string>("WorkingLanguageEnum")
                        .HasColumnType("text");

                    b.Property<string>("WorkingTimeEnum")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("WorkPreferences");
                });

            modelBuilder.Entity("VirtualFinland.UserAPI.Models.UsersDatabase.Occupation", b =>
                {
                    b.HasOne("VirtualFinland.UserAPI.Models.UsersDatabase.User", null)
                        .WithMany("Occupations")
                        .HasForeignKey("UserId");
                });

            modelBuilder.Entity("VirtualFinland.UserAPI.Models.UsersDatabase.WorkPreferences", b =>
                {
                    b.HasOne("VirtualFinland.UserAPI.Models.UsersDatabase.User", "User")
                        .WithOne("WorkPreferences")
                        .HasForeignKey("VirtualFinland.UserAPI.Models.UsersDatabase.WorkPreferences", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("VirtualFinland.UserAPI.Models.UsersDatabase.User", b =>
                {
                    b.Navigation("Occupations");

                    b.Navigation("WorkPreferences");
                });
#pragma warning restore 612, 618
        }
    }
}
