namespace SmartCalculator.Models;

public enum AngleMode
{
    Deg,
    Rad
}

public class HistoryEntry
{
    public string Expression { get; set; } = "";
    public string Result { get; set; } = "";
    public DateTime Timestamp { get; set; } = DateTime.Now;
}