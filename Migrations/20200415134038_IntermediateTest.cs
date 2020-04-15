using Microsoft.EntityFrameworkCore.Migrations;

namespace ArmABot.Migrations {

	public partial class IntermediateTest : Migration {

		protected override void Up(MigrationBuilder migrationBuilder) {
			migrationBuilder.AlterColumn<int>(
				name: "GradeId",
				table: "UserTable",
				nullable: true,
				oldClrType: typeof(int),
				oldType: "int");

			migrationBuilder.CreateIndex(
				name: "IX_UserTable_GradeId",
				table: "UserTable",
				column: "GradeId");

			migrationBuilder.AddForeignKey(
				name: "FK_UserTable_GradesTable_GradeId",
				table: "UserTable",
				column: "GradeId",
				principalTable: "GradesTable",
				principalColumn: "Id",
				onDelete: ReferentialAction.Restrict);
		}

		protected override void Down(MigrationBuilder migrationBuilder) {
			migrationBuilder.DropForeignKey(
				name: "FK_UserTable_GradesTable_GradeId",
				table: "UserTable");

			migrationBuilder.DropIndex(
				name: "IX_UserTable_GradeId",
				table: "UserTable");

			migrationBuilder.AlterColumn<int>(
				name: "GradeId",
				table: "UserTable",
				type: "int",
				nullable: false,
				oldClrType: typeof(int),
				oldNullable: true);
		}
	}
}