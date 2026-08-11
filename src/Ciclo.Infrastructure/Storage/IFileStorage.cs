namespace Ciclo.Infrastructure.Storage;

public interface IFileStorage
{
    Task<string> SaveAsync(Guid tenantId, Guid studentId, string fileName, Stream content);
    Task<Stream> GetAsync(string filePath);
    Task DeleteAsync(string filePath);
}
