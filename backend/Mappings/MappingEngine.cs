using System.Text.Json;
using Jint;
using Jint.Native;

namespace Spindle.Backend.Mappings;

public class MappingEngine
{
    public JsonDocument Apply(JsonDocument source, JsonDocument mappingRules)
    {
        var output = new Dictionary<string, object?>();
        foreach (var rule in mappingRules.RootElement.EnumerateArray())
        {
            var target = rule.GetProperty("targetPath").GetString() ?? string.Empty;
            var sourcePath = rule.TryGetProperty("sourcePath", out var sourceValue) ? sourceValue.GetString() : null;
            var expression = rule.TryGetProperty("expression", out var exprValue) ? exprValue.GetString() : null;

            object? value = null;
            if (!string.IsNullOrWhiteSpace(expression))
            {
                var engine = new Engine();
                engine.SetValue("input", JsValue.FromObject(engine, JsonSerializer.Deserialize<object>(source.RootElement.GetRawText())));
                value = engine.Evaluate(expression).ToObject();
            }
            else if (!string.IsNullOrWhiteSpace(sourcePath))
            {
                value = ResolvePath(source.RootElement, sourcePath);
            }

            if (!string.IsNullOrWhiteSpace(target))
            {
                output[target] = value;
            }
        }

        return JsonDocument.Parse(JsonSerializer.Serialize(output));
    }

    private static object? ResolvePath(JsonElement source, string path)
    {
        var current = source;
        var pieces = path.Split('.', StringSplitOptions.RemoveEmptyEntries);
        foreach (var piece in pieces)
        {
            if (current.ValueKind != JsonValueKind.Object || !current.TryGetProperty(piece, out var next))
            {
                return null;
            }
            current = next;
        }
        return JsonSerializer.Deserialize<object>(current.GetRawText());
    }
}
