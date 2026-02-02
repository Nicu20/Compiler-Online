using OnlineCompiler.Models;

namespace OnlineCompiler.Services;

public class FileUploadService
{
    private readonly List<ProjectFile> _files = new();

    public Task AddFileAsync(ProjectFile file)
    {
        _files.Add(file);
        return Task.CompletedTask;
    }

    public Task<List<ProjectFile>> GetFilesAsync()
    {
        return Task.FromResult(_files.ToList());
    }

    public Task RemoveFileAsync(string fileName)
    {
        var file = _files.FirstOrDefault(f => f.FileName == fileName);
        if(file != null)
        {
            _files.Remove(file);
        }
        return Task.CompletedTask;
    }

    public Task ClearAsync()
    {
        _files.Clear();
        return Task.CompletedTask;
    }
}