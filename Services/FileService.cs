using System.Security.Cryptography;
using System.Text;
using Astra.Contracts.Services;
using Newtonsoft.Json;

namespace Astra.Services;

public class FileService : IFileService
{
    public T Read<T>(string folderPath, string fileName)
    {
        var path = Path.Combine(folderPath, fileName);
        if (!File.Exists(path)) return default;

        var json = File.ReadAllText(path);
        return JsonConvert.DeserializeObject<T>(json);
    }

    public void Save<T>(string folderPath, string fileName, T content)
    {
        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

        var fileContent = JsonConvert.SerializeObject(content, Formatting.Indented);
        File.WriteAllText(Path.Combine(folderPath, fileName), fileContent, Encoding.UTF8);
    }

    public void Delete(string folderPath, string fileName)
    {
        if (fileName != null && File.Exists(Path.Combine(folderPath, fileName)))
            File.Delete(Path.Combine(folderPath, fileName));
    }

    public bool Exists(string folderPath, string fileName)
    {
        return File.Exists(Path.Combine(folderPath, fileName));
    }

    public string GetFileHash(string filePath)
    {
        using var stream = File.Open
            (
                           filePath,
                                      FileMode.Open,
                                      FileAccess.Read,
                                      FileShare.ReadWrite
                                  );
        using var md5 = MD5.Create();
        var hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }

    public async Task CopyFileAsync(string sourceFilePath, string destinationFilePath)
    {
        await Task.Run(() => File.Copy(sourceFilePath, destinationFilePath, true));
    }

    public async Task<bool> CompareFilesAsync(string sourceFilePath, string destinationFilePath)
    {
        return await Task.Run(() => File.ReadAllBytes(sourceFilePath).SequenceEqual(File.ReadAllBytes(destinationFilePath)));
    }
}