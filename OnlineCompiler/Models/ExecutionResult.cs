namespace OnlineCompiler.Models;

public class ExecutionResult
{
    public bool Success {get; set;}
    public string Output {get; set;} = string.Empty;
    public string? Error {get; set;}
    public long ExecutionTimeMs {get; set;}
}