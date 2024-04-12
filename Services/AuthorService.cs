using Astra.Contracts.Services;
using Astra.Helpers;
using Astra.Models;

namespace Astra.Services;

public class AuthorService : IAuthorService
{
    private const string DefaultApplicationDataFolder = "Astra";
    private const string DefaultLocalAuthorFile = "author.json";

    private readonly string _applicationDataFolder;

    private readonly IFileService _fileService;

    private readonly string _localApplicationData =
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

    private readonly string _localauthorFile;

    private bool _isInitialized;

    private Author? _author;

    public AuthorService(IFileService fileService)
    {
        _fileService = fileService;

        _applicationDataFolder = Path.Combine(_localApplicationData, DefaultApplicationDataFolder);
        _localauthorFile = DefaultLocalAuthorFile;
    }

    public async Task<Author?> GetAuthorAsync()
    {
        await InitializeAsync();

        return _author ?? null;
    }

    public async Task SaveAuthorAsync(Author author)
    {
        await InitializeAsync();

         await Json.StringifyAsync(author);

         await Task.Run(() => _fileService.Save(_applicationDataFolder, _localauthorFile, author));
    }

    private async Task InitializeAsync()
    {
        if (!_isInitialized)
        {
            _author = await Task.Run(() =>
                _fileService.Read<Author>(_applicationDataFolder,
                    _localauthorFile));

            _isInitialized = true;
        }
    }
}