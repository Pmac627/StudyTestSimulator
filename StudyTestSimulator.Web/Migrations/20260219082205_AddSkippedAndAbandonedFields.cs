using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudyTestSimulator.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddSkippedAndAbandonedFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SkippedQuestions",
                table: "TestAttempts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "WasAbandoned",
                table: "TestAttempts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSkipped",
                table: "TestAttemptQuestions",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SkippedQuestions",
                table: "TestAttempts");

            migrationBuilder.DropColumn(
                name: "WasAbandoned",
                table: "TestAttempts");

            migrationBuilder.DropColumn(
                name: "IsSkipped",
                table: "TestAttemptQuestions");
        }
    }
}
