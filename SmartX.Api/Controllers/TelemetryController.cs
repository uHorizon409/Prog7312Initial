using Microsoft.AspNetCore.Mvc;
using SmartX.Api.Data;
using SmartX.Shared;

namespace SmartX.Api.Controllers;

[ApiController]
[Route("api/telemetry")]
public class TelemetryController : ControllerBase
{
    private readonly SensorStore _sensorStore;
    private readonly TelemetryHistoryStore _historyStore;

    public TelemetryController(SensorStore sensorStore, TelemetryHistoryStore historyStore)
    {
        _sensorStore = sensorStore;
        _historyStore = historyStore;
    }

    public class TelemetryReadingRequest
    {
        public string SensorId { get; set; } = "";
        public double Value { get; set; }
    }

    public class TelemetryBatchRequest
    {
        public string SensorId { get; set; } = "";
        public double[] Readings { get; set; } = Array.Empty<double>();
    }

    [HttpPost]
    public IActionResult Ingest(TelemetryReadingRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.SensorId))
        {
            return BadRequest("sensor id is required");
        }

        if (double.IsNaN(request.Value) || double.IsInfinity(request.Value))
        {
            return BadRequest("value must be a real number");
        }

        // wrap it in a TelemetryPacket, the client already sends a typed packet
        // per category, this is the api side receiving the raw number for storage
        var packet = new TelemetryPacket<double>(request.SensorId, request.Value);

        bool updated = _sensorStore.UpdateReading(packet.SensorId, packet.Value);
        if (!updated)
        {
            return NotFound("sensor not registered");
        }

        _historyStore.AddBatch(packet.SensorId, new[] { packet.Value });

        return Ok(new { packet.SensorId, packet.Value, packet.Timestamp });
    }

    [HttpPost("batch")]
    public IActionResult IngestBatch(TelemetryBatchRequest request)
    {
        _historyStore.AddBatch(request.SensorId, request.Readings);

        if (request.Readings.Length > 0)
        {
            _sensorStore.UpdateReading(request.SensorId, request.Readings[^1]);
        }

        return Ok(new { request.SensorId, Count = request.Readings.Length });
    }

    [HttpGet("{sensorId}/history")]
    public ActionResult<List<double>> GetHistory(string sensorId)
    {
        return Ok(_historyStore.GetHistory(sensorId));
    }
}
