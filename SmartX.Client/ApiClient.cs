using System.Net.Http.Json;
using SmartX.Shared;

namespace SmartX.Client;

// wraps every call to the api, every method is async so the ui thread never
// freezes while waiting for a response over the network
public class ApiClient
{
    private readonly HttpClient _http;

    public ApiClient(string baseUrl)
    {
        _http = new HttpClient { BaseAddress = new Uri(baseUrl) };
    }

    public class RegisterSensorRequest
    {
        public string MacAddress { get; set; } = "";
        public string Location { get; set; } = "";
        public SensorCategory Category { get; set; }
    }

    public class RegisterResult
    {
        public SensorRegistration? Sensor { get; set; }
        public string? Error { get; set; }
    }

    public async Task<RegisterResult> RegisterSensorAsync(RegisterSensorRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/sensors", request);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            return new RegisterResult { Error = string.IsNullOrWhiteSpace(error) ? "registration failed" : error };
        }

        var sensor = await response.Content.ReadFromJsonAsync<SensorRegistration>();
        return new RegisterResult { Sensor = sensor };
    }

    public async Task<List<SensorRegistration>> GetSensorsAsync()
    {
        var result = await _http.GetFromJsonAsync<List<SensorRegistration>>("api/sensors");
        return result ?? new List<SensorRegistration>();
    }

    public async Task<bool> SendTelemetryAsync(string sensorId, double value)
    {
        var response = await _http.PostAsJsonAsync("api/telemetry", new { sensorId, value });
        return response.IsSuccessStatusCode;
    }

    public async Task<List<double>> GetHistoryAsync(string sensorId)
    {
        var result = await _http.GetFromJsonAsync<List<double>>($"api/telemetry/{sensorId}/history");
        return result ?? new List<double>();
    }

    public async Task<bool> UploadAttachmentAsync(string sensorId, string filePath)
    {
        using var form = new MultipartFormDataContent();
        var bytes = await File.ReadAllBytesAsync(filePath);
        var fileContent = new ByteArrayContent(bytes);
        form.Add(fileContent, "file", Path.GetFileName(filePath));

        var response = await _http.PostAsync($"api/sensors/{sensorId}/attachments", form);
        return response.IsSuccessStatusCode;
    }
}
