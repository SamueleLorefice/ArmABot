using Microsoft.EntityFrameworkCore.Migrations;

namespace ArmABot.Migrations {

	public partial class TablesUpdate : Migration {

		protected override void Up(MigrationBuilder migrationBuilder) {
			migrationBuilder.RenameColumn(
				name: "PollId",
				table: "PollTable",
				newName: "Id");
		}

		protected override void Down(MigrationBuilder migrationBuilder) {
			migrationBuilder.RenameColumn(
				name: "Id",
				table: "PollTable",
				newName: "PollId");
		}
	}
}