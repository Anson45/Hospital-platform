using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PatientAppointment.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "clinical");

            migrationBuilder.CreateTable(
                name: "departments",
                schema: "clinical",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_departments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "patients",
                schema: "clinical",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    PhoneNumber = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    MedicalHistorySummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_patients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "doctors",
                schema: "clinical",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_doctors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_doctors_departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalSchema: "clinical",
                        principalTable: "departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "appointments",
                schema: "clinical",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScheduledStart = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ScheduledEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_appointments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_appointments_doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalSchema: "clinical",
                        principalTable: "doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_appointments_patients_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "clinical",
                        principalTable: "patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "doctor_schedules",
                schema: "clinical",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uuid", nullable: false),
                    DayOfWeek = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_doctor_schedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_doctor_schedules_doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalSchema: "clinical",
                        principalTable: "doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "visits",
                schema: "clinical",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AppointmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    EndedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_visits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_visits_appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalSchema: "clinical",
                        principalTable: "appointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_visits_patients_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "clinical",
                        principalTable: "patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "visit_notes",
                schema: "clinical",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VisitId = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_visit_notes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_visit_notes_visits_VisitId",
                        column: x => x.VisitId,
                        principalSchema: "clinical",
                        principalTable: "visits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_appointments_DoctorId_ScheduledStart",
                schema: "clinical",
                table: "appointments",
                columns: new[] { "DoctorId", "ScheduledStart" });

            migrationBuilder.CreateIndex(
                name: "IX_appointments_PatientId",
                schema: "clinical",
                table: "appointments",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_departments_Name",
                schema: "clinical",
                table: "departments",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_doctor_schedules_DoctorId_DayOfWeek",
                schema: "clinical",
                table: "doctor_schedules",
                columns: new[] { "DoctorId", "DayOfWeek" });

            migrationBuilder.CreateIndex(
                name: "IX_doctors_DepartmentId",
                schema: "clinical",
                table: "doctors",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_doctors_StaffUserId",
                schema: "clinical",
                table: "doctors",
                column: "StaffUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_visit_notes_VisitId",
                schema: "clinical",
                table: "visit_notes",
                column: "VisitId");

            migrationBuilder.CreateIndex(
                name: "IX_visits_AppointmentId",
                schema: "clinical",
                table: "visits",
                column: "AppointmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_visits_PatientId",
                schema: "clinical",
                table: "visits",
                column: "PatientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "doctor_schedules",
                schema: "clinical");

            migrationBuilder.DropTable(
                name: "visit_notes",
                schema: "clinical");

            migrationBuilder.DropTable(
                name: "visits",
                schema: "clinical");

            migrationBuilder.DropTable(
                name: "appointments",
                schema: "clinical");

            migrationBuilder.DropTable(
                name: "doctors",
                schema: "clinical");

            migrationBuilder.DropTable(
                name: "patients",
                schema: "clinical");

            migrationBuilder.DropTable(
                name: "departments",
                schema: "clinical");
        }
    }
}
