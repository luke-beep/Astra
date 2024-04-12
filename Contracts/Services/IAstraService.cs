namespace Astra.Contracts.Services;

public interface IAstraService
{
    Task CreateCommitAsync(string folderPath, string message, string? branch = null);
    Task<bool> IsAstraRepositoryAsync(string folderPath);
    Task InitializeAstraRepositoryAsync(string folderPath);
}