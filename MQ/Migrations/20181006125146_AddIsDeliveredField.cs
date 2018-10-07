using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MQ.Migrations
{
    public partial class AddIsDeliveredField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "PublishDocumentTasks",
                nullable: true,
                oldClrType: typeof(DateTime));

            migrationBuilder.AddColumn<bool>(
                name: "IsDelivered",
                table: "PublishDocumentTasks",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDelivered",
                table: "PublishDocumentTasks");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "PublishDocumentTasks",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldNullable: true);
        }
    }
}
