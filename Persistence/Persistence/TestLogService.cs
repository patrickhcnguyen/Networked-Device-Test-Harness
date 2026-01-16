using Microsoft.EntityFrameworkCore;

namespace Persistence;

public class TestLogService : IDisposable
{
    private readonly TestRunnerDbContext _context;
    private readonly Guid _currentTestRunId;

    public TestLogService(string connectionString = "Data Source=test_logs.db")
    {
        var options = new DbContextOptionsBuilder<TestRunnerDbContext>()
            .UseSqlite(connectionString)
            .Options;

        _context = new TestRunnerDbContext(options);
        _currentTestRunId = Guid.NewGuid();

        _context.Database.EnsureCreated();
    }

    public async Task LogTestResultAsync(
        string testName,
        string testType,
        string status,
        string? command = null,
        string? expectedResponse = null,
        string? actualResponse = null,
        string? errorMessage = null)
    {
        var testLog = new TestLog
        {
            TestRunId = _currentTestRunId,
            TestName = testName,
            TestType = testType,
            Command = command,
            ExpectedResponse = expectedResponse,
            ActualResponse = actualResponse,
            Status = status,
            ErrorMessage = errorMessage,
            Timestamp = DateTime.UtcNow
        };

        _context.TestLogs.Add(testLog);
        await _context.SaveChangesAsync();
    }

    public async Task LogTestPassAsync(string testName, string testType, string? command = null, string? expectedResponse = null, string? actualResponse = null)
    {
        await LogTestResultAsync(testName, testType, "PASS", command, expectedResponse, actualResponse);
    }

    public async Task LogTestFailAsync(string testName, string testType, string? command = null, string? expectedResponse = null, string? actualResponse = null, string? errorMessage = null)
    {
        await LogTestResultAsync(testName, testType, "FAIL", command, expectedResponse, actualResponse, errorMessage);
    }

    public async Task<IEnumerable<TestLog>> GetTestLogsAsync(Guid? testRunId = null)
    {
        var query = _context.TestLogs.AsQueryable();

        if (testRunId.HasValue)
        {
            query = query.Where(t => t.TestRunId == testRunId.Value);
        }

        return await query.OrderByDescending(t => t.Timestamp).ToListAsync();
    }

    public async Task<IEnumerable<TestLog>> GetTestLogsByTypeAsync(string testType)
    {
        return await _context.TestLogs
            .Where(t => t.TestType == testType)
            .OrderByDescending(t => t.Timestamp)
            .ToListAsync();
    }

    public async Task<TestRunSummary> GetTestRunSummaryAsync(Guid testRunId)
    {
        var logs = await _context.TestLogs
            .Where(t => t.TestRunId == testRunId)
            .ToListAsync();

        return new TestRunSummary
        {
            TestRunId = testRunId,
            TotalTests = logs.Count,
            PassedTests = logs.Count(t => t.Status == "PASS"),
            FailedTests = logs.Count(t => t.Status == "FAIL"),
            StartTime = logs.Min(t => t.Timestamp),
            EndTime = logs.Max(t => t.Timestamp)
        };
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}

public class TestRunSummary
{
    public Guid TestRunId { get; set; }
    public int TotalTests { get; set; }
    public int PassedTests { get; set; }
    public int FailedTests { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}
