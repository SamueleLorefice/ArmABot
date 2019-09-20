using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ArmA_Bot.Migrations
{
    public partial class Main : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdminTable",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<decimal>(nullable: false),
                    GroupId = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminTable", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PollTable",
                columns: table => new
                {
                    PollId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    MessageId = table.Column<long>(nullable: false),
                    UserId = table.Column<decimal>(nullable: false),
                    GroupId = table.Column<decimal>(nullable: false),
                    Title = table.Column<string>(nullable: true),
                    EventDate = table.Column<DateTime>(nullable: false),
                    EventQuota = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PollTable", x => x.PollId);
                });

            migrationBuilder.CreateTable(
                name: "VoteTable",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<decimal>(nullable: false),
                    Username = table.Column<string>(nullable: true),
                    Choice = table.Column<int>(nullable: false),
                    PollId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoteTable", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdminTable");

            migrationBuilder.DropTable(
                name: "PollTable");

            migrationBuilder.DropTable(
                name: "VoteTable");
        }
    }
}
