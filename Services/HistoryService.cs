using SmartCalculator.Models;

namespace SmartCalculator.Services;

public class HistoryService
{
    public List<HistoryEntry> Entries { get; } = new();

    public void Add(string expression, string result)
    {
        Entries.Insert(0, new HistoryEntry { Expression = expression, Result = result });
        if (Entries.Count > 50) Entries.RemoveAt(Entries.Count - 1);
    }

    public void Clear() => Entries.Clear();
}