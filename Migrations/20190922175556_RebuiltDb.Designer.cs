﻿// <auto-generated />
using System;
using ArmABot;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ArmABot.Migrations
{
    [DbContext(typeof(DBManager))]
    [Migration("20190922175556_RebuiltDb")]
    partial class RebuiltDb
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.6-servicing-10079")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("ArmA_Bot.DBTables.Admin", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("GroupId");

                    b.Property<long>("UserId");

                    b.HasKey("Id");

                    b.ToTable("AdminTable");
                });

            modelBuilder.Entity("ArmA_Bot.DBTables.Poll", b =>
                {
                    b.Property<int>("PollId")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("EventDate");

                    b.Property<int>("EventQuota");

                    b.Property<long>("GroupId");

                    b.Property<long>("MessageId");

                    b.Property<string>("Title");

                    b.Property<long>("UserId");

                    b.HasKey("PollId");

                    b.ToTable("PollTable");
                });

            modelBuilder.Entity("ArmA_Bot.DBTables.Vote", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("Choice");

                    b.Property<int>("PollId");

                    b.Property<long>("UserId");

                    b.Property<string>("Username");

                    b.HasKey("Id");

                    b.ToTable("VoteTable");
                });
#pragma warning restore 612, 618
        }
    }
}
