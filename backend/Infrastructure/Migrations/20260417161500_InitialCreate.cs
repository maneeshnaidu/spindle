using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spindle.Backend.Infrastructure.Migrations;

public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Users",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                PasswordHash = table.Column<string>(type: "text", nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table => { table.PrimaryKey("PK_Users", x => x.Id); });

        migrationBuilder.CreateTable(
            name: "Workspaces",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "text", nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Workspaces", x => x.Id);
                table.ForeignKey("FK_Workspaces_Users_UserId", x => x.UserId, "Users", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Flows",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                WorkspaceId = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "text", nullable: false),
                Description = table.Column<string>(type: "text", nullable: false),
                WebhookKey = table.Column<string>(type: "text", nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Flows", x => x.Id);
                table.ForeignKey("FK_Flows_Workspaces_WorkspaceId", x => x.WorkspaceId, "Workspaces", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Executions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                FlowId = table.Column<Guid>(type: "uuid", nullable: false),
                StartedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                CompletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                Status = table.Column<string>(type: "text", nullable: false),
                InputPayload = table.Column<string>(type: "jsonb", nullable: false),
                OutputPayload = table.Column<string>(type: "jsonb", nullable: true),
                Error = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Executions", x => x.Id);
                table.ForeignKey("FK_Executions_Flows_FlowId", x => x.FlowId, "Flows", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "FlowSteps",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                FlowId = table.Column<Guid>(type: "uuid", nullable: false),
                SortOrder = table.Column<int>(type: "integer", nullable: false),
                StepType = table.Column<int>(type: "integer", nullable: false),
                Name = table.Column<string>(type: "text", nullable: false),
                Config = table.Column<string>(type: "jsonb", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_FlowSteps", x => x.Id);
                table.ForeignKey("FK_FlowSteps_Flows_FlowId", x => x.FlowId, "Flows", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ExecutionSteps",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                ExecutionId = table.Column<Guid>(type: "uuid", nullable: false),
                FlowStepId = table.Column<Guid>(type: "uuid", nullable: false),
                StepName = table.Column<string>(type: "text", nullable: false),
                SortOrder = table.Column<int>(type: "integer", nullable: false),
                Status = table.Column<string>(type: "text", nullable: false),
                Log = table.Column<string>(type: "text", nullable: true),
                InputPayload = table.Column<string>(type: "jsonb", nullable: true),
                OutputPayload = table.Column<string>(type: "jsonb", nullable: true),
                StartedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                CompletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ExecutionSteps", x => x.Id);
                table.ForeignKey("FK_ExecutionSteps_Executions_ExecutionId", x => x.ExecutionId, "Executions", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Mappings",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                FlowStepId = table.Column<Guid>(type: "uuid", nullable: false),
                Rules = table.Column<string>(type: "jsonb", nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table => { table.PrimaryKey("PK_Mappings", x => x.Id); });

        migrationBuilder.CreateTable(
            name: "Scripts",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                FlowStepId = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "text", nullable: false),
                SourceCode = table.Column<string>(type: "text", nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table => { table.PrimaryKey("PK_Scripts", x => x.Id); });

        migrationBuilder.CreateIndex("IX_Users_Email", "Users", "Email", unique: true);
        migrationBuilder.CreateIndex("IX_Workspaces_UserId", "Workspaces", "UserId", unique: true);
        migrationBuilder.CreateIndex("IX_Flows_WorkspaceId", "Flows", "WorkspaceId");
        migrationBuilder.CreateIndex("IX_Flows_WebhookKey", "Flows", "WebhookKey", unique: true);
        migrationBuilder.CreateIndex("IX_FlowSteps_FlowId", "FlowSteps", "FlowId");
        migrationBuilder.CreateIndex("IX_Executions_FlowId", "Executions", "FlowId");
        migrationBuilder.CreateIndex("IX_ExecutionSteps_ExecutionId", "ExecutionSteps", "ExecutionId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("ExecutionSteps");
        migrationBuilder.DropTable("Mappings");
        migrationBuilder.DropTable("Scripts");
        migrationBuilder.DropTable("FlowSteps");
        migrationBuilder.DropTable("Executions");
        migrationBuilder.DropTable("Flows");
        migrationBuilder.DropTable("Workspaces");
        migrationBuilder.DropTable("Users");
    }
}
