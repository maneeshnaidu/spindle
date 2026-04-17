using System.Text;
using System.Text.Json;

namespace Spindle.Backend.Infrastructure;

public class AiService(HttpClient client, IConfiguration config)
{
    public async Task<string> PromptAsync(string system, string user, CancellationToken ct = default)
    {
        var apiKey = config["OpenAI:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return "{\"error\":\"OpenAI key not configured\"}";
        }

        var model = config["OpenAI:Model"] ?? "gpt-4o-mini";
        var payload = JsonSerializer.Serialize(new
        {
            model,
            messages = new object[]
            {
                new { role = "system", content = system },
                new { role = "user", content = user }
            },
            temperature = 0.2
        });

        using var req = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
        req.Headers.Add("Authorization", $"Bearer {apiKey}");
        req.Content = new StringContent(payload, Encoding.UTF8, "application/json");
        using var response = await client.SendAsync(req, ct);
        var body = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
        {
            return JsonSerializer.Serialize(new { error = body });
        }

        using var doc = JsonDocument.Parse(body);
        return doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "{}";
    }
}
