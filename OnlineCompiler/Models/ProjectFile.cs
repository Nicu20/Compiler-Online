namespace OnlineCompiler.Models;

public class ProjectFile
{
    public string FileName {get; set;} = string.Empty;
    public string Content {get; set;} = string.Empty;
    public DateTime UploadedAt {get; set;} = DateTime.Now;
}
