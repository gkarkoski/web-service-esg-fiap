using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EsgTrainingManagement.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JobRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Name = table.Column<string>(type: "VARCHAR2(100)", unicode: false, maxLength: 100, nullable: false),
                    Department = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Description = table.Column<string>(type: "VARCHAR2(300)", unicode: false, maxLength: 300, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Trainings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Title = table.Column<string>(type: "VARCHAR2(150)", unicode: false, maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "VARCHAR2(500)", unicode: false, maxLength: 500, nullable: false),
                    EsgPillar = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    DurationHours = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ValidityMonths = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trainings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Name = table.Column<string>(type: "VARCHAR2(150)", unicode: false, maxLength: 150, nullable: false),
                    EmployeeNumber = table.Column<string>(type: "VARCHAR2(30)", unicode: false, maxLength: 30, nullable: false),
                    Email = table.Column<string>(type: "VARCHAR2(150)", unicode: false, maxLength: 150, nullable: false),
                    HireDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    IsActive = table.Column<int>(type: "NUMBER(1)", nullable: false),
                    JobRoleId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Employees_JobRoles_JobRoleId",
                        column: x => x.JobRoleId,
                        principalTable: "JobRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrainingRequirements",
                columns: table => new
                {
                    JobRoleId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    TrainingId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    IsMandatory = table.Column<int>(type: "NUMBER(1)", nullable: false),
                    DaysAfterHire = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingRequirements", x => new { x.JobRoleId, x.TrainingId });
                    table.ForeignKey(
                        name: "FK_TrainingRequirements_JobRoles_JobRoleId",
                        column: x => x.JobRoleId,
                        principalTable: "JobRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrainingRequirements_Trainings_TrainingId",
                        column: x => x.TrainingId,
                        principalTable: "Trainings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrainingRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    EmployeeId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    TrainingId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    Score = table.Column<decimal>(type: "NUMBER(5,2)", precision: 5, scale: 2, nullable: true),
                    CertificateCode = table.Column<string>(type: "VARCHAR2(60)", unicode: false, maxLength: 60, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainingRecords_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrainingRecords_Trainings_TrainingId",
                        column: x => x.TrainingId,
                        principalTable: "Trainings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Employees_Email",
                table: "Employees",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_EmployeeNumber",
                table: "Employees",
                column: "EmployeeNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_IsActive_JobRoleId",
                table: "Employees",
                columns: new[] { "IsActive", "JobRoleId" });

            migrationBuilder.CreateIndex(
                name: "IX_Employees_JobRoleId",
                table: "Employees",
                column: "JobRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_JobRoles_Department",
                table: "JobRoles",
                column: "Department");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingRecords_EmployeeId_TrainingId_CompletedAt",
                table: "TrainingRecords",
                columns: new[] { "EmployeeId", "TrainingId", "CompletedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TrainingRecords_TrainingId",
                table: "TrainingRecords",
                column: "TrainingId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingRequirements_TrainingId",
                table: "TrainingRequirements",
                column: "TrainingId");

            migrationBuilder.CreateIndex(
                name: "IX_Trainings_EsgPillar_Title",
                table: "Trainings",
                columns: new[] { "EsgPillar", "Title" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrainingRecords");

            migrationBuilder.DropTable(
                name: "TrainingRequirements");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "Trainings");

            migrationBuilder.DropTable(
                name: "JobRoles");
        }
    }
}
