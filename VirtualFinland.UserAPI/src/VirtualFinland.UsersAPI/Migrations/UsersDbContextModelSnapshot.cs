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

            modelBuilder.Entity("VirtualFinland.UserAPI.Models.UsersDatabase.Address", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid");

                    b.Property<string>("City")
                        .HasMaxLength(512)
                        .HasColumnType("character varying(512)");

                    b.Property<string>("Country")
                        .HasMaxLength(512)
                        .HasColumnType("character varying(512)");

                    b.Property<string>("StreetAddress")
                        .HasMaxLength(512)
                        .HasColumnType("character varying(512)");

                    b.Property<string>("ZipCode")
                        .HasMaxLength(5)
                        .HasColumnType("character varying(5)");

                    b.HasKey("Id");

                    b.ToTable("PersonAdditionalInformation", (string)null);
                });

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

                    b.Property<Guid?>("PersonId")
                        .HasColumnType("uuid");

                    b.Property<string>("Type")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.HasKey("Id");

                    b.HasIndex("PersonId");

                    b.ToTable("Certifications");
                });

            modelBuilder.Entity("VirtualFinland.UserAPI.Models.UsersDatabase.Education", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("EducationFieldCode")
                        .HasMaxLength(4)
                        .HasColumnType("character varying(4)");

                    b.Property<string>("EducationLevelCode")
                        .HasMaxLength(1)
                        .HasColumnType("character varying(1)");

                    b.Property<DateOnly?>("GraduationDate")
                        .HasColumnType("date");

                    b.Property<string>("InstitutionName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<DateTime>("Modified")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<Guid?>("PersonId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("PersonId");

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
                            Id = new Guid("a7b9ef7e-d75d-478d-a3ed-75f64a7e7b38"),
                            Created = new DateTime(2023, 1, 30, 7, 16, 1, 375, DateTimeKind.Utc).AddTicks(300),
                            IdentityId = "118ff6e5-3644-4120-b4a0-3ebee2bfdc8b",
                            Issuer = "fc108242-bd2f-4086-8e91-5b7e7b9a0d94",
                            Modified = new DateTime(2023, 1, 30, 7, 16, 1, 375, DateTimeKind.Utc).AddTicks(300),
                            UserId = new Guid("5a8af4b4-8cb4-44ac-8291-010614601719")
                        });
                });

            modelBuilder.Entity("VirtualFinland.UserAPI.Models.UsersDatabase.Language", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("CerfCode")
                        .HasColumnType("text");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("EscoUri")
                        .HasColumnType("text");

                    b.Property<string>("LanguageCode")
                        .HasMaxLength(3)
                        .HasColumnType("character varying(3)");

                    b.Property<DateTime>("Modified")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("PersonId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("PersonId");

                    b.ToTable("Languages");
                });

            modelBuilder.Entity("VirtualFinland.UserAPI.Models.UsersDatabase.Occupation", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Employer")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

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

                    b.Property<Guid>("PersonId")
                        .HasColumnType("uuid");

                    b.Property<int?>("WorkMonths")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("PersonId");

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

                    b.Property<Guid?>("PersonId")
                        .HasColumnType("uuid");

                    b.Property<string>("TypeCode")
                        .HasMaxLength(3)
                        .HasColumnType("character varying(3)");

                    b.HasKey("Id");

                    b.HasIndex("PersonId");

                    b.ToTable("Permits");
                });

            modelBuilder.Entity("VirtualFinland.UserAPI.Models.UsersDatabase.Person", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Email")
                        .HasColumnType("text");

                    b.Property<string>("GivenName")
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<string>("LastName")
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<DateTime>("Modified")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("text");

                    b.Property<string>("ResidencyCode")
                        .HasMaxLength(3)
                        .HasColumnType("character varying(3)");

                    b.HasKey("Id");

                    b.ToTable("Persons");

                    b.HasData(
                        new
                        {
                            Id = new Guid("5a8af4b4-8cb4-44ac-8291-010614601719"),
                            Created = new DateTime(2023, 1, 30, 7, 16, 1, 375, DateTimeKind.Utc).AddTicks(160),
                            GivenName = "WarmUpUserGivenName",
                            LastName = "WarmUpUserLastName",
                            Modified = new DateTime(2023, 1, 30, 7, 16, 1, 375, DateTimeKind.Utc).AddTicks(160)
                        });
                });

            modelBuilder.Entity("VirtualFinland.UserAPI.Models.UsersDatabase.PersonAdditionalInformation", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

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

                    b.Property<string>("Gender")
                        .HasColumnType("text");

                    b.Property<DateTime>("Modified")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("NativeLanguageCode")
                        .HasMaxLength(10)
                        .HasColumnType("character varying(10)");

                    b.Property<string>("OccupationCode")
                        .HasMaxLength(10)
                        .HasColumnType("character varying(10)");

                    b.HasKey("Id");

                    b.ToTable("PersonAdditionalInformation", (string)null);
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

                    b.Property<Guid>("PersonId")
                        .HasColumnType("uuid");

                    b.Property<List<string>>("Regions")
                        .HasColumnType("text[]");

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

                    b.Property<Guid?>("EducationId")
                        .HasColumnType("uuid");

                    b.Property<string>("EscoUri")
                        .HasColumnType("text");

                    b.Property<DateTime>("Modified")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("OccupationId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("PersonId")
                        .HasColumnType("uuid");

                    b.Property<string>("SkillLevelEnum")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("EducationId");

                    b.HasIndex("OccupationId");

                    b.HasIndex("PersonId");

                    b.ToTable("Skills");
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

                    b.Property<Guid>("PersonId")
                        .HasColumnType("uuid");

                    b.Property<string>("PreferredMunicipalityCode")
                        .HasColumnType("text");

                    b.Property<string>("PreferredRegionCode")
                        .HasColumnType("text");

                    b.Property<string>("WorkingLanguageEnum")
                        .HasMaxLength(2)
                        .HasColumnType("character varying(2)");

                    b.Property<string>("WorkingTimeCode")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("PersonId")
                        .IsUnique();

                    b.ToTable("WorkPreferences");
                });

            modelBuilder.Entity("VirtualFinland.UserAPI.Models.UsersDatabase.Address", b =>
                {
                    b.HasOne("VirtualFinland.UserAPI.Models.UsersDatabase.PersonAdditionalInformation", null)
                        .WithOne("Address")
                        .HasForeignKey("VirtualFinland.UserAPI.Models.UsersDatabase.Address", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("VirtualFinland.UserAPI.Models.UsersDatabase.Certification", b =>
                {
                    b.HasOne("VirtualFinland.UserAPI.Models.UsersDatabase.Person", null)
                        .WithMany("Certifications")
                        .HasForeignKey("PersonId");
                });

            modelBuilder.Entity("VirtualFinland.UserAPI.Models.UsersDatabase.Education", b =>
                {
                    b.HasOne("VirtualFinland.UserAPI.Models.UsersDatabase.Person", null)
                        .WithMany("Educations")
                        .HasForeignKey("PersonId");
                });

            modelBuilder.Entity("VirtualFinland.UserAPI.Models.UsersDatabase.Language", b =>
                {
                    b.HasOne("VirtualFinland.UserAPI.Models.UsersDatabase.Person", null)
                        .WithMany("LanguageSkills")
                        .HasForeignKey("PersonId");
                });

            modelBuilder.Entity("VirtualFinland.UserAPI.Models.UsersDatabase.Occupation", b =>
                {
                    b.HasOne("VirtualFinland.UserAPI.Models.UsersDatabase.Person", null)
                        .WithMany("Occupations")
                        .HasForeignKey("PersonId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("VirtualFinland.UserAPI.Models.UsersDatabase.Permit", b =>
                {
                    b.HasOne("VirtualFinland.UserAPI.Models.UsersDatabase.Person", null)
                        .WithMany("Permits")
                        .HasForeignKey("PersonId");
                });

            modelBuilder.Entity("VirtualFinland.UserAPI.Models.UsersDatabase.PersonAdditionalInformation", b =>
                {
                    b.HasOne("VirtualFinland.UserAPI.Models.UsersDatabase.Person", "Person")
                        .WithOne("AdditionalInformation")
                        .HasForeignKey("VirtualFinland.UserAPI.Models.UsersDatabase.PersonAdditionalInformation", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Person");
                });

            modelBuilder.Entity("VirtualFinland.UserAPI.Models.UsersDatabase.Skills", b =>
                {
                    b.HasOne("VirtualFinland.UserAPI.Models.UsersDatabase.Education", null)
                        .WithMany("Skills")
                        .HasForeignKey("EducationId");

                    b.HasOne("VirtualFinland.UserAPI.Models.UsersDatabase.Occupation", null)
                        .WithMany("Skills")
                        .HasForeignKey("OccupationId");

                    b.HasOne("VirtualFinland.UserAPI.Models.UsersDatabase.Person", "Person")
                        .WithMany("Skills")
                        .HasForeignKey("PersonId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Person");
                });

            modelBuilder.Entity("VirtualFinland.UserAPI.Models.UsersDatabase.WorkPreferences", b =>
                {
                    b.HasOne("VirtualFinland.UserAPI.Models.UsersDatabase.Person", null)
                        .WithOne("WorkPreferences")
                        .HasForeignKey("VirtualFinland.UserAPI.Models.UsersDatabase.WorkPreferences", "PersonId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("VirtualFinland.UserAPI.Models.UsersDatabase.Education", b =>
                {
                    b.Navigation("Skills");
                });

            modelBuilder.Entity("VirtualFinland.UserAPI.Models.UsersDatabase.Occupation", b =>
                {
                    b.Navigation("Skills");
                });

            modelBuilder.Entity("VirtualFinland.UserAPI.Models.UsersDatabase.Person", b =>
                {
                    b.Navigation("AdditionalInformation");

                    b.Navigation("Certifications");

                    b.Navigation("Educations");

                    b.Navigation("LanguageSkills");

                    b.Navigation("Occupations");

                    b.Navigation("Permits");

                    b.Navigation("Skills");

                    b.Navigation("WorkPreferences");
                });

            modelBuilder.Entity("VirtualFinland.UserAPI.Models.UsersDatabase.PersonAdditionalInformation", b =>
                {
                    b.Navigation("Address");
                });
#pragma warning restore 612, 618
        }
    }
}
