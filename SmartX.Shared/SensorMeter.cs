namespace SmartX.Shared;

public enum SensorCategory
{
    Environmental,
    PowerConsumption,
    Actuator
}

// one reading from a meter style sensor, operators let us do stuff like
// Meter3 = Meter1 + Meter2 to get the combined load of two smart meters
// instead of writing a separate add method every time
public class SensorMeter
{
    public string SensorId { get; set; }
    public double Reading { get; set; }

    public SensorMeter(string sensorId, double reading)
    {
        SensorId = sensorId;
        Reading = reading;
    }

    public static SensorMeter operator +(SensorMeter a, SensorMeter b)
    {
        return new SensorMeter($"{a.SensorId}+{b.SensorId}", a.Reading + b.Reading);
    }

    public static SensorMeter operator -(SensorMeter a, SensorMeter b)
    {
        return new SensorMeter($"{a.SensorId}-{b.SensorId}", a.Reading - b.Reading);
    }

    // used to compare two readings, e.g. checking a delta between two time points
    public static bool operator >(SensorMeter a, SensorMeter b) => a.Reading > b.Reading;
    public static bool operator <(SensorMeter a, SensorMeter b) => a.Reading < b.Reading;

    public override string ToString()
    {
        return $"{SensorId}: {Reading:F2}";
    }
}
