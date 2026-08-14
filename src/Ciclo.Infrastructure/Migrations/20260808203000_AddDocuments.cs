using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ciclo.Infrastructure.Migrations;

/// <inheritdoc />
#pragma warning disable CA1861
[Migration("20260808203000")]
public partial class AddDocuments : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // DocumentTypes table
        migrationBuilder.CreateTable(
            name: "DocumentTypes",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                Nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                Descricao = table.Column<string>(type: "text", nullable: true),
                IsRequired = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                ValidadeMeses = table.Column<int>(type: "integer", nullable: false),
                IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DocumentTypes", x => x.Id);
                table.ForeignKey(
                    name: "FK_DocumentTypes_Tenants_TenantId",
                    column: x => x.TenantId,
                    principalTable: "Tenants",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        // Documents table
        migrationBuilder.CreateTable(
            name: "Documents",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                DocumentTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                NomeArquivo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                CaminhoArquivo = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                Status = table.Column<int>(type: "integer", nullable: false),
                DataValidade = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                MotivoRejeicao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Documents", x => x.Id);
                table.ForeignKey(
                    name: "FK_Documents_DocumentTypes_DocumentTypeId",
                    column: x => x.DocumentTypeId,
                    principalTable: "DocumentTypes",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Documents_Students_StudentId",
                    column: x => x.StudentId,
                    principalTable: "Students",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_Documents_Tenants_TenantId",
                    column: x => x.TenantId,
                    principalTable: "Tenants",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        // Indexes
        migrationBuilder.CreateIndex(
            name: "IX_DocumentTypes_TenantId",
            table: "DocumentTypes",
            column: "TenantId");

        migrationBuilder.CreateIndex(
            name: "IX_Documents_TenantId",
            table: "Documents",
            column: "TenantId");

        migrationBuilder.CreateIndex(
            name: "IX_Documents_StudentId",
            table: "Documents",
            column: "StudentId");

        migrationBuilder.CreateIndex(
            name: "IX_Documents_TenantId_Status",
            table: "Documents",
            columns: new[] { "TenantId", "Status" });

        migrationBuilder.CreateIndex(
            name: "IX_Documents_DocumentTypeId",
            table: "Documents",
            column: "DocumentTypeId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Documents");
        migrationBuilder.DropTable(name: "DocumentTypes");
    }
}
