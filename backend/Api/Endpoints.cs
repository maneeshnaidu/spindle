using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Spindle.Backend.Application;
using Spindle.Backend.Domain;
using Spindle.Backend.Flows;
using Spindle.Backend.Infrastructure;

namespace Spindle.Backend.Api;

public static class Endpoints
{
    public static void MapSpindleEndpoints(this WebApplication app)
    {
        var auth = app.MapGroup("/api/auth");
        auth.MapPost("/signup", Signup);
        auth.MapPost("/login", Login);

        var api = app.MapGroup("/api").RequireAuthorization();
        api.MapGet("/flows", GetFlows);
        api.MapPost("/flows", CreateFlow);
        api.MapGet("/flows/{flowId:guid}", GetFlow);
        api.MapPut("/flows/{flowId:guid}/steps", SaveSteps);
        api.MapPost("/flows/{flowId:guid}/execute", ExecuteFlow);
        api.MapGet("/runs", GetRuns);
        api.MapGet("/runs/{executionId:guid}", GetRun);
        api.MapPost("/mappings", SaveMapping);
        api.MapPost("/scripts", SaveScript);

        api.MapPost("/ai/mapping-suggestions", AiMapping);
        api.MapPost("/ai/script-generation", AiScript);
        api.MapPost("/ai/flow-draft", AiFlowDraft);

        app.MapPost("/webhooks/{webhookKey}", Webhook).AllowAnonymous();
    }

    private static Guid UserId(ClaimsPrincipal user) => Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub")!);

    private static async Task<IResult> Signup(AppDbContext db, IPasswordService pwd, IJwtService jwt, SignupRequest request)
    {
        if (await db.Users.AnyAsync(x => x.Email == request.Email)) return Results.BadRequest(new { error = "Email already used" });
        var user = new User { Email = request.Email, PasswordHash = pwd.Hash(request.Password) };
        var workspace = new Workspace { UserId = user.Id, Name = $"{request.Email.Split('@')[0]} workspace" };
        db.Users.Add(user);
        db.Workspaces.Add(workspace);
        await db.SaveChangesAsync();
        return Results.Ok(new { token = jwt.Create(user), user = new { user.Id, user.Email } });
    }

    private static async Task<IResult> Login(AppDbContext db, IPasswordService pwd, IJwtService jwt, LoginRequest request)
    {
        var user = await db.Users.FirstOrDefaultAsync(x => x.Email == request.Email);
        if (user is null || !pwd.Verify(request.Password, user.PasswordHash)) return Results.Unauthorized();
        return Results.Ok(new { token = jwt.Create(user), user = new { user.Id, user.Email } });
    }

    private static async Task<IResult> GetFlows(AppDbContext db, ClaimsPrincipal user)
    {
        var userId = UserId(user);
        var flows = await db.Flows
            .Where(f => f.Workspace.UserId == userId)
            .Select(f => new { f.Id, f.Name, f.Description, f.WebhookKey, f.UpdatedAtUtc })
            .ToListAsync();
        return Results.Ok(flows);
    }

    private static async Task<IResult> CreateFlow(AppDbContext db, ClaimsPrincipal user, CreateFlowRequest request)
    {
        var userId = UserId(user);
        var workspace = await db.Workspaces.FirstAsync(x => x.UserId == userId);
        var flow = new Flow { WorkspaceId = workspace.Id, Name = request.Name, Description = request.Description ?? "" };
        db.Flows.Add(flow);
        await db.SaveChangesAsync();
        return Results.Ok(flow);
    }

    private static async Task<IResult> GetFlow(AppDbContext db, ClaimsPrincipal user, Guid flowId)
    {
        var userId = UserId(user);
        var flow = await db.Flows.Include(x => x.Steps).FirstOrDefaultAsync(x => x.Id == flowId && x.Workspace.UserId == userId);
        return flow is null ? Results.NotFound() : Results.Ok(flow);
    }

    private static async Task<IResult> SaveSteps(AppDbContext db, ClaimsPrincipal user, Guid flowId, SaveStepsRequest request)
    {
        var userId = UserId(user);
        var flow = await db.Flows.Include(x => x.Steps).FirstOrDefaultAsync(x => x.Id == flowId && x.Workspace.UserId == userId);
        if (flow is null) return Results.NotFound();
        db.FlowSteps.RemoveRange(flow.Steps);
        flow.Steps = request.Steps.Select((s, i) => new FlowStep
        {
            FlowId = flowId,
            SortOrder = i,
            Name = s.Name,
            StepType = s.StepType,
            Config = JsonDocument.Parse(s.Config.GetRawText())
        }).ToList();
        flow.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Results.Ok(flow.Steps);
    }

    private static async Task<IResult> ExecuteFlow(AppDbContext db, ClaimsPrincipal user, FlowExecutionService executionService, Guid flowId, JsonElement payload)
    {
        var userId = UserId(user);
        var flow = await db.Flows.Include(x => x.Steps).FirstOrDefaultAsync(x => x.Id == flowId && x.Workspace.UserId == userId);
        if (flow is null) return Results.NotFound();
        var execution = await executionService.ExecuteAsync(flow, JsonDocument.Parse(payload.GetRawText()));
        return Results.Ok(new { execution.Id, execution.Status });
    }

    private static async Task<IResult> Webhook(AppDbContext db, FlowExecutionService executionService, string webhookKey, JsonElement payload)
    {
        var flow = await db.Flows.Include(x => x.Steps).FirstOrDefaultAsync(x => x.WebhookKey == webhookKey);
        if (flow is null) return Results.NotFound();
        var execution = await executionService.ExecuteAsync(flow, JsonDocument.Parse(payload.GetRawText()));
        return Results.Ok(new { execution.Id, execution.Status });
    }

    private static async Task<IResult> GetRuns(AppDbContext db, ClaimsPrincipal user)
    {
        var userId = UserId(user);
        var runs = await db.Executions
            .Where(x => x.Flow.Workspace.UserId == userId)
            .OrderByDescending(x => x.StartedAtUtc)
            .Select(x => new { x.Id, x.FlowId, x.Status, x.StartedAtUtc, x.CompletedAtUtc, x.Error })
            .Take(100)
            .ToListAsync();
        return Results.Ok(runs);
    }

    private static async Task<IResult> GetRun(AppDbContext db, ClaimsPrincipal user, Guid executionId)
    {
        var userId = UserId(user);
        var run = await db.Executions.Include(x => x.Steps).FirstOrDefaultAsync(x => x.Id == executionId && x.Flow.Workspace.UserId == userId);
        return run is null ? Results.NotFound() : Results.Ok(run);
    }

    private static async Task<IResult> SaveMapping(AppDbContext db, SaveMappingRequest request)
    {
        var mapping = new Mapping { FlowStepId = request.FlowStepId, Rules = JsonDocument.Parse(request.Rules.GetRawText()) };
        db.Mappings.Add(mapping);
        await db.SaveChangesAsync();
        return Results.Ok(mapping);
    }

    private static async Task<IResult> SaveScript(AppDbContext db, SaveScriptRequest request)
    {
        var script = new ScriptDefinition { FlowStepId = request.FlowStepId, Name = request.Name, SourceCode = request.SourceCode };
        db.Scripts.Add(script);
        await db.SaveChangesAsync();
        return Results.Ok(script);
    }

    private static async Task<IResult> AiMapping(AiService ai, AiRequest req)
    {
        var text = await ai.PromptAsync(
            "You generate JSON mapping suggestions only. Return strict JSON array with targetPath, sourcePath, expression.",
            $"Source JSON: {req.SourceJson}\nTarget JSON: {req.TargetJson}");
        return Results.Ok(new { result = text });
    }

    private static async Task<IResult> AiScript(AiService ai, PromptRequest req)
    {
        var text = await ai.PromptAsync(
            "Generate JavaScript transform code for Jint. Return only JS body that can use message/log/variables and returns transformed object.",
            req.Prompt);
        return Results.Ok(new { result = text });
    }

    private static async Task<IResult> AiFlowDraft(AiService ai, PromptRequest req)
    {
        var text = await ai.PromptAsync(
            "Create a linear flow draft as JSON with steps array. Allowed step types: WebhookTrigger, Mapping, Script, HttpRequest.",
            req.Prompt);
        return Results.Ok(new { result = text });
    }
}

public record SignupRequest(string Email, string Password);
public record LoginRequest(string Email, string Password);
public record CreateFlowRequest(string Name, string? Description);
public record SaveStepRequest(string Name, FlowStepType StepType, JsonElement Config);
public record SaveStepsRequest(List<SaveStepRequest> Steps);
public record SaveMappingRequest(Guid FlowStepId, JsonElement Rules);
public record SaveScriptRequest(Guid FlowStepId, string Name, string SourceCode);
public record AiRequest(string SourceJson, string TargetJson);
public record PromptRequest(string Prompt);
