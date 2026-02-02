namespace OnlineCompiler.Models;

public class CompileResult
{
    public bool Success {get; set;}
    public byte[]? Assembly {get; set;}
    public List<string> Errors {get; set;} = new();
    public List<string> Warnings {get; set;} = new();
    public long CompilationTimeMs {get; set;}
}
