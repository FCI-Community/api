using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Graduation_project.Migrations
{
    /// <inheritdoc />
    public partial class a : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "year",
                table: "Subjects",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SubjectId",
                table: "StudentProfiles",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentProfiles_SubjectId",
                table: "StudentProfiles",
                column: "SubjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentProfiles_Subjects_SubjectId",
                table: "StudentProfiles",
                column: "SubjectId",
                principalTable: "Subjects",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentProfiles_Subjects_SubjectId",
                table: "StudentProfiles");

            migrationBuilder.DropIndex(
                name: "IX_StudentProfiles_SubjectId",
                table: "StudentProfiles");

            migrationBuilder.DropColumn(
                name: "year",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "SubjectId",
                table: "StudentProfiles");
        }
    }
}
