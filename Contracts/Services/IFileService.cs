namespace Astra.Contracts.Services;

public interface IFileService
{
    T Read<T>(string folderPath, string fileName);

    void Save<T>(string folderPath, string fileName, T content);

    void Delete(string folderPath, string fileName);

    bool Exists(string folderPath, string fileName);

    string GetFileHash(string filePath);

    Task CopyFileAsync(string sourceFilePath, string destinationFilePath);

    Task<bool> CompareFilesAsync(string sourceFilePath, string destinationFilePath);
}