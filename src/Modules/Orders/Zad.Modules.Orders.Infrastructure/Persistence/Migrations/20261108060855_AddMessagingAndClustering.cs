using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zad.Modules.Orders.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMessagingAndClustering : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OutboxMessages_Pending",
                schema: "orders",
                table: "OutboxMessages");

            migrationBuilder.AddColumn<long>(
                name: "SequenceNumber",
                schema: "orders",
                table: "Orders",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<long>(
                name: "SequenceNumber",
                schema: "orders",
                table: "OrderItems",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.CreateTable(
                name: "InboxMessages",
                schema: "orders",
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
                name: "IX_OutboxMessages_Dispatched",
                schema: "orders",
                table: "OutboxMessages",
                column: "DispatchedAtUtc",
                filter: "[Status] = 'Dispatched'");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_Pending",
                schema: "orders",
                table: "OutboxMessages",
                column: "NextAttemptAtUtc",
                filter: "[Status] = 'Pending'");

            migrationBuilder.CreateIndex(
                name: "CX_Order_SequenceNumber",
                schema: "orders",
                table: "Orders",
                columns: new[] { "TenantId", "SequenceNumber" },
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "CX_OrderItem_SequenceNumber",
                schema: "orders",
                table: "OrderItems",
                columns: new[] { "TenantId", "SequenceNumber" },
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_InboxMessages_Pending",
                schema: "orders",
                table: "InboxMessages",
                column: "NextAttemptAtUtc",
                filter: "[Status] = 'Pending'");

            migrationBuilder.CreateIndex(
                name: "IX_InboxMessages_Processed",
                schema: "orders",
                table: "InboxMessages",
                column: "ProcessedAtUtc",
                filter: "[Status] = 'Processed'");

            migrationBuilder.CreateIndex(
                name: "UX_InboxMessages_MessageId_HandlerName",
                schema: "orders",
                table: "InboxMessages",
                columns: new[] { "MessageId", "HandlerName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InboxMessages",
                schema: "orders");

            migrationBuilder.DropIndex(
                name: "IX_OutboxMessages_Dispatched",
                schema: "orders",
                table: "OutboxMessages");

            migrationBuilder.DropIndex(
                name: "IX_OutboxMessages_Pending",
                schema: "orders",
                table: "OutboxMessages");

            migrationBuilder.DropIndex(
                name: "CX_Order_SequenceNumber",
                schema: "orders",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "CX_OrderItem_SequenceNumber",
                schema: "orders",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "SequenceNumber",
                schema: "orders",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "SequenceNumber",
                schema: "orders",
                table: "OrderItems");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_Pending",
                schema: "orders",
                table: "OutboxMessages",
                column: "NextAttemptAtUtc");
        }
    }
}
