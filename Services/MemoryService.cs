namespace SmartCalculator.Services;

public class MemoryService
{
    public double Value { get; private set; } = 0;
    public bool HasValue { get; private set; } = false;

    public void Store(double v) { Value = v; HasValue = true; }
    public void Add(double v) { Value += v; HasValue = true; }
    public void Subtract(double v) { Value -= v; HasValue = true; }
    public void Clear() { Value = 0; HasValue = false; }
    public double Recall() => Value;
}