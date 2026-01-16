namespace Persistence;

public class TestLog
{
    public int Id { get; set; }
    public Guid TestRunId { get; set; }
    public string TestName { get; set; } = string.Empty;
    public string TestType { get; set; } = string.Empty; // "functional", "concurrency", etc.
    public string? Command { get; set; }
    public string? ExpectedResponse { get; set; }
    public string? ActualResponse { get; set; }
    public string Status { get; set; } = string.Empty; // "PASS", "FAIL"
    public string? ErrorMessage { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
