using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Integration.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PublishDocumentTasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    UserId = table.Column<int>(nullable: false),
                    OrganizationId = table.Column<int>(nullable: false),
                    DocumentType = table.Column<int>(nullable: false),
                    DocumentId = table.Column<int>(nullable: false),
                    DocumentRevision = table.Column<int>(nullable: false),
                    Payload = table.Column<string>(nullable: true),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: true),
                    RefId = table.Column<string>(nullable: true),
                    LoadId = table.Column<long>(nullable: true),
                    State = table.Column<int>(nullable: false),
                    IsDelivered = table.Column<bool>(nullable: false),
                    HasEisExceptions = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublishDocumentTasks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PublishDocumentTaskAttempts",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Request = table.Column<string>(nullable: true),
                    Response = table.Column<string>(nullable: true),
                    HasEisExceptions = table.Column<bool>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    PublishDocumentTaskId = table.Column<Guid>(nullable: false),
                    Result = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublishDocumentTaskAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PublishDocumentTaskAttempts_PublishDocumentTasks_PublishDocumentTaskId",
                        column: x => x.PublishDocumentTaskId,
                        principalTable: "PublishDocumentTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PublishDocumentTaskAttempts_Id",
                table: "PublishDocumentTaskAttempts",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_PublishDocumentTaskAttempts_PublishDocumentTaskId",
                table: "PublishDocumentTaskAttempts",
                column: "PublishDocumentTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_PublishDocumentTasks_Id",
                table: "PublishDocumentTasks",
                column: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PublishDocumentTaskAttempts");

            migrationBuilder.DropTable(
                name: "PublishDocumentTasks");
        }
    }
}
