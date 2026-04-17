using System.Text.Json;

namespace Spindle.Backend.Domain;

public enum FlowStepType
{
    WebhookTrigger = 1,
    Mapping = 2,
    Script = 3,
    HttpRequest = 4
}

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public Workspace Workspace { get; set; } = null!;
}

public class Workspace
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string Name { get; set; } = "Default Workspace";
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public User User { get; set; } = null!;
    public ICollection<Flow> Flows { get; set; } = new List<Flow>();
}

public class Flow
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WorkspaceId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string WebhookKey { get; set; } = Guid.NewGuid().ToString("N");
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    public Workspace Workspace { get; set; } = null!;
    public ICollection<FlowStep> Steps { get; set; } = new List<FlowStep>();
    public ICollection<Execution> Executions { get; set; } = new List<Execution>();
}

public class FlowStep
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid FlowId { get; set; }
    public int SortOrder { get; set; }
    public FlowStepType StepType { get; set; }
    public string Name { get; set; } = string.Empty;
    public JsonDocument Config { get; set; } = JsonDocument.Parse("{}");
    public Flow Flow { get; set; } = null!;
}

public class Mapping
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid FlowStepId { get; set; }
    public JsonDocument Rules { get; set; } = JsonDocument.Parse("[]");
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

public class ScriptDefinition
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid FlowStepId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SourceCode { get; set; } = "return message;";
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

public class Execution
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid FlowId { get; set; }
    public DateTime StartedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAtUtc { get; set; }
    public string Status { get; set; } = "Running";
    public JsonDocument InputPayload { get; set; } = JsonDocument.Parse("{}");
    public JsonDocument? OutputPayload { get; set; }
    public string? Error { get; set; }
    public Flow Flow { get; set; } = null!;
    public ICollection<ExecutionStep> Steps { get; set; } = new List<ExecutionStep>();
}

public class ExecutionStep
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ExecutionId { get; set; }
    public Guid FlowStepId { get; set; }
    public string StepName { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public string Status { get; set; } = "Pending";
    public string? Log { get; set; }
    public JsonDocument? InputPayload { get; set; }
    public JsonDocument? OutputPayload { get; set; }
    public DateTime StartedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAtUtc { get; set; }
    public Execution Execution { get; set; } = null!;
}
