using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NordicHiking.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSeasonToVideo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Season",
                table: "Videos",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Season",
                table: "Videos");
        }
    }
}
