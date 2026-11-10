using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zad.Modules.Tenancy.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMessagingAndClustering : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Tenants",
                schema: "tenancy",
                table: "Tenants");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Databases",
                schema: "tenancy",
                table: "Databases");

            migrationBuilder.AddColumn<long>(
                name: "SequenceNumber",
                schema: "tenancy",
                table: "Tenants",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<long>(
                name: "SequenceNumber",
                schema: "tenancy",
                table: "Databases",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tenants",
                schema: "tenancy",
                table: "Tenants",
                column: "Id")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Databases",
                schema: "tenancy",
                table: "Databases",
                column: "Id")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "CX_Tenant_SequenceNumber",
                schema: "tenancy",
                table: "Tenants",
                column: "SequenceNumber",
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "CX_TenantDatabase_SequenceNumber",
                schema: "tenancy",
                table: "Databases",
                column: "SequenceNumber",
                unique: true)
                .Annotation("SqlServer:Clustered", true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Tenants",
                schema: "tenancy",
                table: "Tenants");

            migrationBuilder.DropIndex(
                name: "CX_Tenant_SequenceNumber",
                schema: "tenancy",
                table: "Tenants");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Databases",
                schema: "tenancy",
                table: "Databases");

            migrationBuilder.DropIndex(
                name: "CX_TenantDatabase_SequenceNumber",
                schema: "tenancy",
                table: "Databases");

            migrationBuilder.DropColumn(
                name: "SequenceNumber",
                schema: "tenancy",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "SequenceNumber",
                schema: "tenancy",
                table: "Databases");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tenants",
                schema: "tenancy",
                table: "Tenants",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Databases",
                schema: "tenancy",
                table: "Databases",
                column: "Id");
        }
    }
}
