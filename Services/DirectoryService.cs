using Astra.Contracts.Services;

namespace Astra.Services;

public class DirectoryService : IDirectoryService
{
    public async Task<bool> IsDirectoryAsync(string folderPath)
    {
        return await Task.Run(() => Directory.Exists(folderPath));
    }

    public async Task CreateDirectoryAsync(string folderPath)
    {
        if (await IsDirectoryAsync(folderPath)) return;
        await Task.Run(() => Directory.CreateDirectory(folderPath));
    }

    public async Task CopyDirectoryAsync(string sourceFolderPath, string destinationFolderPath, string exclude = "")
    {
        if (!await IsDirectoryAsync(sourceFolderPath)) return;

        await CreateDirectoryAsync(destinationFolderPath);

        foreach (var file in Directory.GetFiles(sourceFolderPath))
        {
            var fileName = Path.GetFileName(file);
            var destFile = Path.Combine(destinationFolderPath, fileName);
            await Task.Run(() => File.Copy(file, destFile, true));

            var fileAttributes = File.GetAttributes(file);
            File.SetAttributes(destFile, fileAttributes);
        }

        foreach (var folder in Directory.GetDirectories(sourceFolderPath))
        {
            var folderName = Path.GetFileName(folder);
            var destFolder = Path.Combine(destinationFolderPath, folderName);
            if (folderName == exclude) continue;
            await CopyDirectoryAsync(folder, destFolder, exclude);
            var folderAttributes = File.GetAttributes(folder);
            File.SetAttributes(destFolder, folderAttributes);
        }
    }

    public Task CompareAndDeleteDirectoryAsync(string sourceFolderPath, string destinationFolderPath, string exclude)
    {
        var destinationAstraFolder = Path.Combine(destinationFolderPath, exclude);

        var destinationFiles = Directory.GetFiles(destinationFolderPath, "*", SearchOption.AllDirectories)
                                        .Where(file => !file.StartsWith(destinationAstraFolder)).ToList();
        var destinationDirectories = Directory.GetDirectories(destinationFolderPath, "*", SearchOption.AllDirectories)
                                              .Where(dir => !dir.StartsWith(destinationAstraFolder)).ToList();

        foreach (var destinationFile in destinationFiles)
        {
            var relativePath = destinationFile[(destinationFolderPath.Length + 1)..];
            var sourceFilePath = Path.Combine(sourceFolderPath, relativePath);

            if (!File.Exists(sourceFilePath))
            {
                File.Delete(destinationFile);
            }
        }

        foreach (var destinationDirectory in destinationDirectories)
        {
            var relativePath = destinationDirectory[(destinationFolderPath.Length + 1)..];
            var sourceDirectoryPath = Path.Combine(sourceFolderPath, relativePath);

            if (Directory.Exists(sourceDirectoryPath)) continue;
            if(Directory.Exists(destinationDirectory)) Directory.Delete(destinationDirectory, true);
        }

        return Task.CompletedTask;
    }

    public async Task MoveDirectoryAsync(string sourceFolderPath, string destinationFolderPath, string exclude)
    {
        await CopyDirectoryAsync(sourceFolderPath, destinationFolderPath, exclude);
        await DeleteDirectoryAsync(sourceFolderPath);
    }

    public async Task DeleteDirectoryAsync(string folderPath)
    {
        await Task.Run(() => Directory.Delete(folderPath, true));
    }

    public async Task<IEnumerable<string>> GetDirectoriesAsync(string folderPath)
    {
        return await Task.Run(() => Directory.GetDirectories(folderPath));
    }

    public async Task<IEnumerable<string>> GetFilesAsync(string folderPath)
    {
        return await Task.Run(() => Directory.GetFiles(folderPath));
    }
}