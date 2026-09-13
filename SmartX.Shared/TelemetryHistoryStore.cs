namespace SmartX.Shared;

// keeps raw telemetry batches as a jagged array per sensor since each batch
// coming in can be a different size, then GetHistory moves it all into a
// proper List<double> once we actually need to work with the data
public class TelemetryHistoryStore
{
    private readonly Dictionary<string, double[][]> _rawBatches = new();

    public void AddBatch(string sensorId, double[] batch)
    {
        if (!_rawBatches.TryGetValue(sensorId, out var existing))
        {
            _rawBatches[sensorId] = new double[][] { batch };
            return;
        }

        // grow the jagged array by one slot and put the new batch on the end
        var grown = new double[existing.Length + 1][];
        Array.Copy(existing, grown, existing.Length);
        grown[existing.Length] = batch;
        _rawBatches[sensorId] = grown;
    }

    public List<double> GetHistory(string sensorId)
    {
        var history = new List<double>();

        if (!_rawBatches.TryGetValue(sensorId, out var batches))
        {
            return history;
        }

        foreach (double[] batch in batches)
        {
            history.AddRange(batch);
        }

        return history;
    }

    public int GetBatchCount(string sensorId)
    {
        return _rawBatches.TryGetValue(sensorId, out var batches) ? batches.Length : 0;
    }

    public IEnumerable<string> GetTrackedSensorIds()
    {
        return _rawBatches.Keys;
    }
}
