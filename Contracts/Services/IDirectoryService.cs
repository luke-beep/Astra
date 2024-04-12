namespace Astra.Contracts.Services;

public interface IDirectoryService
{
    Task<bool> IsDirectoryAsync(string folderPath);

    Task CreateDirectoryAsync(string folderPath);

    Task CopyDirectoryAsync(string sourceFolderPath, string destinationFolderPath, string exclude = "");

    Task CompareAndDeleteDirectoryAsync(string sourceFolderPath, string destinationFolderPath, string exclude);

    Task MoveDirectoryAsync(string sourceFolderPath, string destinationFolderPath, string exclude);

    Task DeleteDirectoryAsync(string folderPath);

    Task<IEnumerable<string>> GetDirectoriesAsync(string folderPath);

    Task<IEnumerable<string>> GetFilesAsync(string folderPath);
}