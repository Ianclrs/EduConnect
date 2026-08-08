using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;

namespace EduGestor.Infrastructure.Storage;

public class LocalFileStorage : IFileStorage
{
    private readonly string _rootPath;

    public LocalFileStorage(IOptions<FileStorageOptions> options)
    {
        _rootPath = Path.GetFullPath(options.Value.RootPath);
    }

    public async Task<string> SaveAsync(Guid tenantId, Guid studentId, string fileName, Stream content)
    {
        var safeName = SanitizeFileName(fileName);
        var uniqueName = $"{Guid.NewGuid():N}_{safeName}";
        var dir = Path.Combine(_rootPath, tenantId.ToString(), studentId.ToString());
        Directory.CreateDirectory(dir);

        var filePath = Path.Combine(dir, uniqueName);
        await using var fileStream = File.Create(filePath);
        await content.CopyToAsync(fileStream);

        return filePath;
    }

    public Task<Stream> GetAsync(string filePath)
    {
        var fullPath = Path.GetFullPath(filePath);

        // Prevent path traversal: verify file is within root
        if (!fullPath.StartsWith(_rootPath + Path.DirectorySeparatorChar, StringComparison.Ordinal) && fullPath != _rootPath)
            throw new UnauthorizedAccessException("File path is outside storage root.");

        if (!File.Exists(fullPath))
            throw new FileNotFoundException("File not found.", fullPath);

        return Task.FromResult<Stream>(File.OpenRead(fullPath));
    }

    public Task DeleteAsync(string filePath)
    {
        var fullPath = Path.GetFullPath(filePath);

        if (!fullPath.StartsWith(_rootPath + Path.DirectorySeparatorChar, StringComparison.Ordinal) && fullPath != _rootPath)
            throw new UnauthorizedAccessException("File path is outside storage root.");

        if (File.Exists(fullPath))
            File.Delete(fullPath);

        return Task.CompletedTask;
    }

    private static string SanitizeFileName(string fileName)
    {
        // Allow only alphanumeric, underscore, dot, hyphen
        var safe = Regex.Replace(fileName, @"[^a-zA-Z0-9._-]", "_");
        if (string.IsNullOrWhiteSpace(safe))
            safe = "unnamed";
        return safe;
    }
}

public class FileStorageOptions
{
    public const string SectionName = "FileStorage";
    public string RootPath { get; set; } = "uploads";
}
