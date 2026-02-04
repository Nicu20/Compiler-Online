using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System.Reflection;
using System.Text;
using OnlineCompiler.Models;

namespace OnlineCompiler.Services;

public class CompilerService
{
    public async Task<CompileResult> CompileAsync(string code)
    {
        var result = new CompileResult();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            var tree = CSharpSyntaxTree.ParseText(code);

            var references = GetMetadataReferences();

            var compilation = CSharpCompilation.Create("DynamicAssembly")
                .WithOptions(new CSharpCompilationOptions(OutputKind.ConsoleApplication))
                .AddReferences(references)
                .AddSyntaxTrees(tree);
            
            using(var ms = new MemoryStream())
            {
                var emitResult = compilation.Emit(ms);

                stopwatch.Stop();
                result.CompilationTimeMs = stopwatch.ElapsedMilliseconds;

                if(emitResult.Success)
                {
                    result.Success = true;
                    result.Assembly = ms.ToArray();
                }
                else
                {
                    result.Success = false;

                    foreach(var diagnostic in emitResult.Diagnostics)
                    {
                        var message = $"Line {diagnostic.Location.GetLineSpan().StartLinePosition.Line + 1}: {diagnostic.GetMessage()}";

                        if(diagnostic.Severity == DiagnosticSeverity.Error)
                            result.Errors.Add(message);
                        else
                            result.Warnings.Add(message);
                    }
                }
            }
        }
        catch(Exception ex)
        {
            stopwatch.Stop();
            result.Success = false;
            result.Errors.Add($"Compilation exception: {ex.Message}");
            result.CompilationTimeMs = stopwatch.ElapsedMilliseconds;
        }

        return result;
    }

    public async Task<ExecutionResult> ExecuteAsync(byte[] assembly)
    {
        var result = new ExecutionResult();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            var loadedAssembly = Assembly.Load(assembly);

            var mainMethod = loadedAssembly.EntryPoint;

            if(mainMethod == null)
            {
                stopwatch.Stop();
                result.Success = false;
                result.Error = "No Main method found in the code.";
                result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
                return result;
            }

            var oldOut = Console.Out;
            using(var stringWriter = new StringWriter())
            {
                Console.SetOut(stringWriter);

                try
                {
                    mainMethod.Invoke(null, new object[] { Array.Empty<string>() });
                    result.Output = stringWriter.ToString();
                    result.Success = true;
                }
                catch(Exception ex)
                {
                    result.Success = false;
                    result.Error = ex.InnerException?.Message ?? ex.Message;
                    result.Output = stringWriter.ToString();
                }
                finally
                {
                    Console.SetOut(oldOut);
                }
            }

            stopwatch.Stop();
            result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
        }
        catch(Exception ex)
        {
            stopwatch.Stop();
            result.Success = false;
            result.Error = $"Execution exception: {ex.Message}";
            result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
        }

        return result;
    }

    private List<MetadataReference> GetMetadataReferences()
    {
        var references = new List<MetadataReference>();

        var tpa = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string;
        if(!string.IsNullOrEmpty(tpa))
        {
            var paths = tpa.Split(Path.PathSeparator);
            foreach(var path in paths)
            {
                if(path.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) && File.Exists(path))
                {
                    try
                    {
                    references.Add(MetadataReference.CreateFromFile(path));
                    }
                    catch{}
                }
            }
        }

        return references;
    }

    /*
    private bool ShouldIncludeReference(string path)
    {
        var fileName = Path.GetFileName(path).ToLower();

        var excludeList = new[] {"testhost", "vstest"};
        if(excludeList.Any(e => fileName.Contains(e))) return false;

        return true;
    }
    */
}