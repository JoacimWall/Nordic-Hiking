using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NordicHiking.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddVideoTalkSummary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VideoTalkSummary",
                table: "Videos",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VideoTalkSummary",
                table: "Videos");
        }
    }
}
