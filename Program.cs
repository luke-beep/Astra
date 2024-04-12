using Astra.Contracts.Services;
using Astra.Models;
using Astra.Services;

namespace Astra
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            IFileService fileService = new FileService();
            IDirectoryService directoryService = new DirectoryService();
            ISettingsService settingsService = new SettingsService(fileService);
            IAuthorService authorService = new AuthorService(fileService);

            await settingsService.SaveSettingAsync("RemoteRepository", @"Z:\AstraTest");

            var author = await authorService.GetAuthorAsync();

            if (null == author)
            {
                Console.WriteLine("Please specify name and email...");
                var name = Console.ReadLine();
                var email = Console.ReadLine();
                author = new Author { Name = name, Email = email };
                await authorService.SaveAuthorAsync(author);
            }
            IAstraService astraService = new AstraService(fileService, directoryService, settingsService, author);
            if (args.Length == 0)
            {
                Console.WriteLine("Please specify a command...");
            }
            else switch (args[0])
            {
                case "init":
                {
                    var path = args.Length > 1 ? args[1] : Directory.GetCurrentDirectory();
                    await astraService.InitializeAstraRepositoryAsync(path);
                    Console.WriteLine(await astraService.IsAstraRepositoryAsync(path));
                    break;
                }
                case "commit":
                {
                    var path = args.Length > 1 ? args[1] : Directory.GetCurrentDirectory();
                    var message = args.Length > 2 ? args[2] : "No message";
                    await astraService.CreateCommitAsync(path, message);
                    break;
                }
            }
        }
    }
}
