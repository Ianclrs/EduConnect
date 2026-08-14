using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ciclo.Infrastructure.Migrations;

/// <inheritdoc />
#pragma warning disable CA1861
[Migration("20260808195000")]
public partial class AddStudents : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Students table
        migrationBuilder.CreateTable(
            name: "Students",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                Nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                DataNascimento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                Cpf = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: true),
                Turma = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                AnoLetivo = table.Column<int>(type: "integer", nullable: false),
                Status = table.Column<int>(type: "integer", nullable: false),
                Observacoes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Students", x => x.Id);
                table.ForeignKey(
                    name: "FK_Students_Tenants_TenantId",
                    column: x => x.TenantId,
                    principalTable: "Tenants",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        // StudentParents join table
        migrationBuilder.CreateTable(
            name: "StudentParents",
            columns: table => new
            {
                StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                ParentId = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_StudentParents", x => new { x.StudentId, x.ParentId });
                table.ForeignKey(
                    name: "FK_StudentParents_Students_StudentId",
                    column: x => x.StudentId,
                    principalTable: "Students",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_StudentParents_Users_ParentId",
                    column: x => x.ParentId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        // Indexes for Students
        migrationBuilder.CreateIndex(
            name: "IX_Students_TenantId",
            table: "Students",
            column: "TenantId");

        migrationBuilder.CreateIndex(
            name: "IX_Students_TenantId_Nome",
            table: "Students",
            columns: new[] { "TenantId", "Nome" });

        migrationBuilder.CreateIndex(
            name: "IX_Students_TenantId_Cpf",
            table: "Students",
            columns: new[] { "TenantId", "Cpf" },
            unique: true,
            filter: "\"Cpf\" IS NOT NULL");

        // Index for StudentParents
        migrationBuilder.CreateIndex(
            name: "IX_StudentParents_ParentId",
            table: "StudentParents",
            column: "ParentId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "StudentParents");
        migrationBuilder.DropTable(name: "Students");
    }
}
