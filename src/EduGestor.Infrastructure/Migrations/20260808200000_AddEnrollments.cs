using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduGestor.Infrastructure.Migrations;

/// <inheritdoc />
#pragma warning disable CA1861
public partial class AddEnrollments : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // EnrollmentPeriods table
        migrationBuilder.CreateTable(
            name: "EnrollmentPeriods",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                Nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                DataInicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                DataFim = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                AnoLetivo = table.Column<int>(type: "integer", nullable: false),
                IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_EnrollmentPeriods", x => x.Id);
                table.ForeignKey(
                    name: "FK_EnrollmentPeriods_Tenants_TenantId",
                    column: x => x.TenantId,
                    principalTable: "Tenants",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        // Enrollments table
        migrationBuilder.CreateTable(
            name: "Enrollments",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                EnrollmentPeriodId = table.Column<Guid>(type: "uuid", nullable: false),
                Status = table.Column<int>(type: "integer", nullable: false),
                MotivoRejeicao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Enrollments", x => x.Id);
                table.ForeignKey(
                    name: "FK_Enrollments_EnrollmentPeriods_EnrollmentPeriodId",
                    column: x => x.EnrollmentPeriodId,
                    principalTable: "EnrollmentPeriods",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Enrollments_Students_StudentId",
                    column: x => x.StudentId,
                    principalTable: "Students",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Enrollments_Tenants_TenantId",
                    column: x => x.TenantId,
                    principalTable: "Tenants",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        // Indexes for EnrollmentPeriods
        migrationBuilder.CreateIndex(
            name: "IX_EnrollmentPeriods_TenantId",
            table: "EnrollmentPeriods",
            column: "TenantId");

        migrationBuilder.CreateIndex(
            name: "IX_EnrollmentPeriods_TenantId_AnoLetivo",
            table: "EnrollmentPeriods",
            columns: new[] { "TenantId", "AnoLetivo" });

        // Indexes for Enrollments
        migrationBuilder.CreateIndex(
            name: "IX_Enrollments_TenantId",
            table: "Enrollments",
            column: "TenantId");

        migrationBuilder.CreateIndex(
            name: "IX_Enrollments_StudentId",
            table: "Enrollments",
            column: "StudentId");

        migrationBuilder.CreateIndex(
            name: "IX_Enrollments_TenantId_Status",
            table: "Enrollments",
            columns: new[] { "TenantId", "Status" });

        migrationBuilder.CreateIndex(
            name: "IX_Enrollments_EnrollmentPeriodId",
            table: "Enrollments",
            column: "EnrollmentPeriodId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Enrollments");
        migrationBuilder.DropTable(name: "EnrollmentPeriods");
    }
}
