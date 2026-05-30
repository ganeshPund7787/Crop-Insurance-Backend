using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend_Crop_Insurrance.Migrations
{
    /// <inheritdoc />
    public partial class AgentAPIs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Claims",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DamageType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DamageDescription = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    EstimatedLossAmount = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    ApprovedAmount = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    IncidentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    RejectionReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AgentRemarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReviewedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CropId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FarmerProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AgentProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Claims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Claims_AgentProfiles_AgentProfileId",
                        column: x => x.AgentProfileId,
                        principalTable: "AgentProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Claims_Crops_CropId",
                        column: x => x.CropId,
                        principalTable: "Crops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Claims_FarmerProfiles_FarmerProfileId",
                        column: x => x.FarmerProfileId,
                        principalTable: "FarmerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Inspections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InspectionNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ScheduledAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false),
                    Findings = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DamagePercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    RecommendedAmount = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    InspectorNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ClaimId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AgentProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inspections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Inspections_AgentProfiles_AgentProfileId",
                        column: x => x.AgentProfileId,
                        principalTable: "AgentProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Inspections_Claims_ClaimId",
                        column: x => x.ClaimId,
                        principalTable: "Claims",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Claims_AgentProfileId",
                table: "Claims",
                column: "AgentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Claims_ClaimNumber",
                table: "Claims",
                column: "ClaimNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Claims_CropId",
                table: "Claims",
                column: "CropId");

            migrationBuilder.CreateIndex(
                name: "IX_Claims_FarmerProfileId",
                table: "Claims",
                column: "FarmerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Inspections_AgentProfileId",
                table: "Inspections",
                column: "AgentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Inspections_ClaimId",
                table: "Inspections",
                column: "ClaimId");

            migrationBuilder.CreateIndex(
                name: "IX_Inspections_InspectionNumber",
                table: "Inspections",
                column: "InspectionNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Inspections");

            migrationBuilder.DropTable(
                name: "Claims");
        }
    }
}
