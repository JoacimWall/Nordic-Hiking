using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NordicHiking.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddShowOnMapToVideo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ShowOnMap",
                table: "Videos",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShowOnMap",
                table: "Videos");
        }
    }
}
