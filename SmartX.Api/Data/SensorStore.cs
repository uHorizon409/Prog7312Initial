using System.Collections.Concurrent;
using SmartX.Shared;

namespace SmartX.Api.Data;

// in memory store for part 1, no database needed yet since the brief says
// this is fine for now, part 2 can swap this out for a real db later
public class SensorStore
{
    private readonly ConcurrentDictionary<string, SensorRegistration> _sensors = new();

    public SensorRegistration Add(SensorRegistration sensor)
    {
        _sensors[sensor.Id] = sensor;
        return sensor;
    }

    public List<SensorRegistration> GetAll()
    {
        return _sensors.Values.ToList();
    }

    public SensorRegistration? Get(string id)
    {
        _sensors.TryGetValue(id, out var sensor);
        return sensor;
    }

    public bool UpdateReading(string id, double reading)
    {
        if (_sensors.TryGetValue(id, out var sensor))
        {
            sensor.LastReading = reading;
            sensor.LastSeen = DateTime.UtcNow;
            return true;
        }
        return false;
    }
}
