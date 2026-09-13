namespace SmartX.Shared;

// generic wrapper so a float, int or bool reading can all move through the
// same pipeline without boxing into object like a plain List<object> would need
public class TelemetryPacket<T> where T : struct
{
    public string SensorId { get; }
    public T Value { get; }
    public DateTime Timestamp { get; }

    public TelemetryPacket(string sensorId, T value)
    {
        SensorId = sensorId;
        Value = value;
        Timestamp = DateTime.UtcNow;
    }

    public override string ToString()
    {
        return $"{SensorId}: {Value} @ {Timestamp:HH:mm:ss}";
    }
}
