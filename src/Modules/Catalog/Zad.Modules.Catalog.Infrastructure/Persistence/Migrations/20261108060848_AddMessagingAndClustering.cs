using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zad.Modules.Catalog.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMessagingAndClustering : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OutboxMessages_Pending",
                schema: "catalog",
                table: "OutboxMessages");

            migrationBuilder.AddColumn<long>(
                name: "SequenceNumber",
                schema: "catalog",
                table: "Products",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.CreateTable(
                name: "InboxMessages",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HandlerName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CorrelationId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    CausationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReceivedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AttemptCount = table.Column<int>(type: "int", nullable: false),
                    NextAttemptAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LockedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LockedUntilUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastError = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ProcessedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InboxMessages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "CX_Product_SequenceNumber",
                schema: "catalog",
                table: "Products",
                columns: new[] { "TenantId", "SequenceNumber" },
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_Dispatched",
                schema: "catalog",
                table: "OutboxMessages",
                column: "DispatchedAtUtc",
                filter: "[Status] = 'Dispatched'");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_Pending",
                schema: "catalog",
                table: "OutboxMessages",
                column: "NextAttemptAtUtc",
                filter: "[Status] = 'Pending'");

            migrationBuilder.CreateIndex(
                name: "IX_InboxMessages_Pending",
                schema: "catalog",
                table: "InboxMessages",
                column: "NextAttemptAtUtc",
                filter: "[Status] = 'Pending'");

            migrationBuilder.CreateIndex(
                name: "IX_InboxMessages_Processed",
                schema: "catalog",
                table: "InboxMessages",
                column: "ProcessedAtUtc",
                filter: "[Status] = 'Processed'");

            migrationBuilder.CreateIndex(
                name: "UX_InboxMessages_MessageId_HandlerName",
                schema: "catalog",
                table: "InboxMessages",
                columns: new[] { "MessageId", "HandlerName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InboxMessages",
                schema: "catalog");

            migrationBuilder.DropIndex(
                name: "CX_Product_SequenceNumber",
                schema: "catalog",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_OutboxMessages_Dispatched",
                schema: "catalog",
                table: "OutboxMessages");

            migrationBuilder.DropIndex(
                name: "IX_OutboxMessages_Pending",
                schema: "catalog",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "SequenceNumber",
                schema: "catalog",
                table: "Products");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_Pending",
                schema: "catalog",
                table: "OutboxMessages",
                column: "NextAttemptAtUtc");
        }
    }
}
