using System.Text.Json;

namespace SmartX.Client;

// reads the api url from appsettings.json instead of hardcoding it in the form,
// falls back to localhost if the file is missing for some reason
public static class AppConfig
{
    public static string GetApiBaseUrl()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");

        if (!File.Exists(path))
        {
            return "http://localhost:5154/";
        }

        var json = File.ReadAllText(path);
        var settings = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

        return settings != null && settings.TryGetValue("ApiBaseUrl", out var url)
            ? url
            : "http://localhost:5154/";
    }
}
