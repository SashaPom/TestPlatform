using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestPlatform.Migrations
{
    /// <inheritdoc />
    public partial class modified_tests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPassed",
                table: "AssignedTests",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPassed",
                table: "AssignedTests");
        }
    }
}
