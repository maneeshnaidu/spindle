using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Spindle.Backend.Domain;
using Spindle.Backend.Infrastructure;
using Spindle.Backend.Mappings;
using Spindle.Backend.Scripts;

namespace Spindle.Backend.Flows;

public class FlowExecutionService(
    AppDbContext db,
    MappingEngine mappingEngine,
    ScriptEngine scriptEngine,
    IHttpClientFactory httpClientFactory)
{
    public async Task<Execution> ExecuteAsync(Flow flow, JsonDocument input, CancellationToken ct = default)
    {
        var execution = new Execution
        {
            FlowId = flow.Id,
            InputPayload = JsonDocument.Parse(input.RootElement.GetRawText())
        };
        db.Executions.Add(execution);
        await db.SaveChangesAsync(ct);

        JsonDocument payload = JsonDocument.Parse(input.RootElement.GetRawText());
        try
        {
            var orderedSteps = flow.Steps.OrderBy(x => x.SortOrder).ToList();
            foreach (var step in orderedSteps)
            {
                var execStep = new ExecutionStep
                {
                    ExecutionId = execution.Id,
                    FlowStepId = step.Id,
                    StepName = step.Name,
                    SortOrder = step.SortOrder,
                    InputPayload = JsonDocument.Parse(payload.RootElement.GetRawText()),
                    Status = "Running",
                    StartedAtUtc = DateTime.UtcNow
                };
                db.ExecutionSteps.Add(execStep);
                await db.SaveChangesAsync(ct);

                payload = await ExecuteStep(step, payload, execStep, ct);
                execStep.OutputPayload = JsonDocument.Parse(payload.RootElement.GetRawText());
                execStep.Status = "Success";
                execStep.CompletedAtUtc = DateTime.UtcNow;
                await db.SaveChangesAsync(ct);
            }

            execution.Status = "Success";
            execution.OutputPayload = JsonDocument.Parse(payload.RootElement.GetRawText());
            execution.CompletedAtUtc = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
            await TrimRuns(flow.Id, ct);
            return execution;
        }
        catch (Exception ex)
        {
            execution.Status = "Failed";
            execution.Error = ex.Message;
            execution.CompletedAtUtc = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
            await TrimRuns(flow.Id, ct);
            throw;
        }
    }

    private async Task<JsonDocument> ExecuteStep(FlowStep step, JsonDocument payload, ExecutionStep log, CancellationToken ct)
    {
        switch (step.StepType)
        {
            case FlowStepType.Mapping:
                var mappingId = step.Config.RootElement.GetProperty("mappingId").GetGuid();
                var mapping = await db.Mappings.FirstAsync(x => x.Id == mappingId, ct);
                return mappingEngine.Apply(payload, mapping.Rules);

            case FlowStepType.Script:
                var scriptId = step.Config.RootElement.GetProperty("scriptId").GetGuid();
                var script = await db.Scripts.FirstAsync(x => x.Id == scriptId, ct);
                var result = scriptEngine.Execute(script.SourceCode, payload);
                log.Log = string.Join("\n", result.Logs);
                return result.Output;

            case FlowStepType.HttpRequest:
                var method = step.Config.RootElement.GetProperty("method").GetString() ?? "POST";
                var url = step.Config.RootElement.GetProperty("url").GetString() ?? throw new Exception("Missing URL");
                using (var message = new HttpRequestMessage(new HttpMethod(method), url))
                {
                    if (step.Config.RootElement.TryGetProperty("headers", out var headers) && headers.ValueKind == JsonValueKind.Object)
                    {
                        foreach (var header in headers.EnumerateObject())
                        {
                            message.Headers.TryAddWithoutValidation(header.Name, header.Value.GetString());
                        }
                    }

                    if (method != "GET")
                    {
                        var body = step.Config.RootElement.TryGetProperty("body", out var configBody)
                            ? configBody.GetRawText()
                            : payload.RootElement.GetRawText();
                        message.Content = new StringContent(body, Encoding.UTF8, "application/json");
                    }

                    var client = httpClientFactory.CreateClient();
                    using var response = await client.SendAsync(message, ct);
                    var responseBody = await response.Content.ReadAsStringAsync(ct);
                    if (!response.IsSuccessStatusCode) throw new Exception($"HTTP step failed: {response.StatusCode} {responseBody}");
                    return JsonDocument.Parse(string.IsNullOrWhiteSpace(responseBody) ? "{}" : responseBody);
                }

            default:
                return payload;
        }
    }

    private async Task TrimRuns(Guid flowId, CancellationToken ct)
    {
        var stale = await db.Executions
            .Where(x => x.FlowId == flowId)
            .OrderByDescending(x => x.StartedAtUtc)
            .Skip(20)
            .ToListAsync(ct);
        if (stale.Count == 0) return;
        db.Executions.RemoveRange(stale);
        await db.SaveChangesAsync(ct);
    }
}
