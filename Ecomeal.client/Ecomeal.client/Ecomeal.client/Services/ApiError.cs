using System.Text.Json;

namespace Ecomeal.client.Services;

public static class ApiError
{
    public static async Task<string> ReadAsync(HttpResponseMessage response, string fallback)
    {
        try
        {
            var text = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(text))
                return fallback;

            using var doc = JsonDocument.Parse(text);
            var root = doc.RootElement;

            if (root.ValueKind == JsonValueKind.String)
                return root.GetString() is { Length: > 0 } s ? s : fallback;

            if (root.ValueKind == JsonValueKind.Object)
            {
                if (root.TryGetProperty("errors", out var errorsProp) || root.TryGetProperty("Errors", out errorsProp))
                {
                    var messages = new List<string>();
                    CollectMessages(errorsProp, messages);

                    var joined = string.Join(" ", messages.Where(m => !string.IsNullOrWhiteSpace(m)));
                    if (!string.IsNullOrWhiteSpace(joined))
                        return joined;
                }

                if (root.TryGetProperty("detail", out var detailProp) && detailProp.ValueKind == JsonValueKind.String)
                    return detailProp.GetString() is { Length: > 0 } d ? d : fallback;

                if (root.TryGetProperty("title", out var titleProp) && titleProp.ValueKind == JsonValueKind.String)
                    return titleProp.GetString() is { Length: > 0 } t ? t : fallback;
            }
        }
        catch
        {
            // response body wasn't readable JSON - fall back below
        }

        return fallback;
    }

    private static void CollectMessages(JsonElement element, List<string> messages)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.String:
                var value = element.GetString();
                if (!string.IsNullOrWhiteSpace(value))
                    messages.Add(value);
                break;
            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                    CollectMessages(item, messages);
                break;
            case JsonValueKind.Object:
                foreach (var prop in element.EnumerateObject())
                    CollectMessages(prop.Value, messages);
                break;
        }
    }
}
