using System.Text.Json;
using Jint;
using Jint.Native;

namespace Spindle.Backend.Scripts;

public class ScriptContext
{
    public object? Message { get; set; }
    public Dictionary<string, object?> Variables { get; set; } = [];
    public List<string> Logs { get; set; } = [];
    public void Log(string value) => Logs.Add(value);
}

public class ScriptEngine
{
    public (JsonDocument Output, List<string> Logs) Execute(string script, JsonDocument input)
    {
        var context = new ScriptContext
        {
            Message = JsonSerializer.Deserialize<object>(input.RootElement.GetRawText())
        };

        var engine = new Engine();
        engine.SetValue("message", context.Message);
        engine.SetValue("variables", context.Variables);
        engine.SetValue("log", new Action<string>(context.Log));

        var wrapped = $"(function(){{ {script} }})()";
        var result = engine.Evaluate(wrapped);
        var payload = result.IsNull() || result.IsUndefined()
            ? context.Message
            : result.ToObject();

        var json = JsonDocument.Parse(JsonSerializer.Serialize(payload));
        return (json, context.Logs);
    }
}
