using Microsoft.EntityFrameworkCore.Migrations;

namespace MQ.Migrations
{
    public partial class AddIsHasEisErrorToTask : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsHasEisErrors",
                table: "PublishDocumentTasks",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsHasEisErrors",
                table: "PublishDocumentTasks");
        }
    }
}
