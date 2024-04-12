using Astra.Contracts.Services;
using Astra.Enums;
using Astra.Models;
using Astra.Models.Configs;

namespace Astra.Services;

public class AstraService(
    IFileService fileService,
    IDirectoryService directoryService,
    ISettingsService settingsService,
    Author author)
    : IAstraService
{
    public Task<bool> IsAstraRepositoryAsync(string folderPath)
    {
        return Task.FromResult(Directory.Exists(Path.Combine(folderPath, ".astra")));
    }

    public async Task CreateCommitAsync(string folderPath, string message, string? branch = null)
    {
        var remoteFolder = await settingsService.ReadSettingAsync<string>("RemoteRepository");
        remoteFolder = Path.Combine(remoteFolder, Path.GetFileName(folderPath));
        if (string.IsNullOrEmpty(remoteFolder)) throw new Exception("Remote repository not set.");

        // Read the remote, commits, and branches configuration files
        var remote = fileService.Read<RemoteConfig>(Path.Combine(folderPath, ".astra"), "remotes");
        var commits = fileService.Read<CommitConfig>(Path.Combine(folderPath, ".astra"), "commits");
        var branches = fileService.Read<BranchConfig>(Path.Combine(folderPath, ".astra"), "branches");

        // Get the default branch
        var defaultBranch = branches.Branches.FirstOrDefault(b => b.Id == remote.Remote.DefaultBranchId) ??
                                                          throw new Exception("Branch not found.");
        if (!string.IsNullOrEmpty(branch)) {
            defaultBranch = branches.Branches.FirstOrDefault(b => b.Name == branch) ??
                            throw new Exception("Branch not found.");
        }

        // Create a new commit
        var commit = new Commit
        {
            Id = commits.Commits.Count,
            Author = author,
            Message = message,
            Date = DateTime.Now,
            Branch = defaultBranch,
            Hash = Guid.NewGuid().ToString(),
            DirectoryModifications = [],
            FileModifications = []
        };
        
        // Compare and copy changes
        await CompareAndCopyChangesAsync(folderPath, remoteFolder, commit);
        
        // Add the commit to the commits configuration file
        commits.Commits.Add(commit);
        fileService.Save(Path.Combine(folderPath, ".astra"), "commits", commits);

        // Update the branch configuration file
        // await directoryService.CompareAndDeleteDirectoryAsync(folderPath, remoteFolder, ".astra");
        // Copy the local repository to the remote repository
        // await directoryService.CopyDirectoryAsync(folderPath, remoteFolder);
    }

    public async Task CompareAndCopyChangesAsync(string folderPath, string remoteFolder, Commit commit)
    {
        var localAstraFolder = Path.Combine(folderPath, ".astra");

        var commitHashFolder = Path.Combine(localAstraFolder, "objects", "origin", "master", commit.Hash);

        var localFiles = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories)
                                  .Where(file => !file.StartsWith(localAstraFolder)).ToList();
        var localDirectories = Directory.GetDirectories(folderPath, "*", SearchOption.AllDirectories)
                                        .Where(dir => !dir.StartsWith(localAstraFolder)).ToList();

        var remoteFiles = Directory.GetFiles(remoteFolder, "*", SearchOption.AllDirectories)
                                   .Select(file => file[(remoteFolder.Length + 1)..]).ToList();
        var remoteDirectories = Directory.GetDirectories(remoteFolder, "*", SearchOption.AllDirectories)
                                         .Select(dir => dir[(remoteFolder.Length + 1)..]).ToList();

        foreach (var remoteDirectory in remoteDirectories)
        {
            var localDirectoryPath = Path.Combine(folderPath, remoteDirectory);

            if (!Directory.Exists(localDirectoryPath))
            {
                commit.DirectoryModifications[remoteDirectory] = DirectoryModificationStatus.Deleted;

                var destinationDirectoryPath = Path.Combine(commitHashFolder, remoteDirectory);
                if (Directory.Exists(destinationDirectoryPath)) Directory.CreateDirectory(destinationDirectoryPath);
            }
        }



        foreach (var remoteFile in remoteFiles)
        {
            var localFilePath = Path.Combine(folderPath, remoteFile);

            if (!File.Exists(localFilePath))
            {
                // The file doesn't exist locally, so it's been deleted
                commit.FileModifications[remoteFile] = FileModificationStatus.Deleted;

                var remoteFilePath = Path.Combine(remoteFolder, remoteFile);
                if (File.Exists(remoteFilePath))
                {
                    File.Delete(remoteFilePath);
                }

                // Copy the deleted file to the commitHash folder
                var destinationFilePath = Path.Combine(commitHashFolder, remoteFile);
                var destinationDirectory = Path.GetDirectoryName(destinationFilePath);
                if (destinationDirectory != null)
                {
                    Directory.CreateDirectory(destinationDirectory);
                    File.Copy(remoteFilePath, destinationFilePath);
                }
            }
        }


        foreach (var localDirectory in localDirectories)
        {
            var relativePath = localDirectory[(folderPath.Length + 1)..];
            var remoteDirectoryPath = Path.Combine(remoteFolder, relativePath);

            if (!Directory.Exists(remoteDirectoryPath))
            {
                var destinationDirectory = Path.Combine(commitHashFolder, relativePath);
                Directory.CreateDirectory(destinationDirectory);
                await directoryService.CopyDirectoryAsync(localDirectory, destinationDirectory);

                commit.DirectoryModifications[relativePath] = DirectoryModificationStatus.Created;
            }
            else if (Directory.GetLastWriteTime(localDirectory) > Directory.GetLastWriteTime(remoteDirectoryPath))
            {
                var destinationDirectory = Path.Combine(commitHashFolder, relativePath);
                Directory.CreateDirectory(destinationDirectory);
                await directoryService.CopyDirectoryAsync(localDirectory, destinationDirectory);

                commit.DirectoryModifications[relativePath] = DirectoryModificationStatus.Modified;
            }
        }

        foreach (var localFile in localFiles)
        {
            var relativePath = localFile[(folderPath.Length + 1)..];
            var remoteFilePath = Path.Combine(remoteFolder, relativePath);

            var destinationDirectory = Path.GetDirectoryName(remoteFilePath);
            if (destinationDirectory != null) Directory.CreateDirectory(destinationDirectory);

            File.Copy(localFile, remoteFilePath, true);

            if (File.Exists(remoteFilePath))
            {
                var localHash = fileService.GetFileHash(localFile);
                var remoteHash = fileService.GetFileHash(remoteFilePath);

                if (localHash == remoteHash) continue;

                commit.FileModifications[relativePath] = FileModificationStatus.Modified;
            }
            else
            {
                commit.FileModifications[relativePath] = FileModificationStatus.Created;
            }
        }

    }



    public async Task InitializeAstraRepositoryAsync(string folderPath)
    {
        var remoteFolder = await settingsService.ReadSettingAsync<string>("RemoteRepository");
        var folderPathName = Path.GetFileName(folderPath);
        if (string.IsNullOrEmpty(remoteFolder)) throw new Exception("Remote repository not set.");

        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

        if (!Directory.Exists(Path.Combine(folderPath, ".astra")))
            Directory.CreateDirectory(Path.Combine(folderPath, ".astra")).Attributes |= FileAttributes.Hidden;

        if (!Directory.Exists(Path.Combine(folderPath, ".astra", "info")))
            Directory.CreateDirectory(Path.Combine(folderPath, ".astra", "info"));

        if (!Directory.Exists(Path.Combine(folderPath, ".astra", "logs")))
            Directory.CreateDirectory(Path.Combine(folderPath, ".astra", "logs"));

        if (!Directory.Exists(Path.Combine(folderPath, ".astra", "logs", "origin")))
            Directory.CreateDirectory(Path.Combine(folderPath, ".astra", "logs", "origin"));

        if (!Directory.Exists(Path.Combine(folderPath, ".astra", "logs", "origin", "master")))
            Directory.CreateDirectory(Path.Combine(folderPath, ".astra", "logs", "origin", "master"));

        if (!Directory.Exists(Path.Combine(folderPath, ".astra", "objects")))
            Directory.CreateDirectory(Path.Combine(folderPath, ".astra", "objects"));

        if (!Directory.Exists(Path.Combine(folderPath, ".astra", "objects", "origin")))
            Directory.CreateDirectory(Path.Combine(folderPath, ".astra", "objects", "origin"));

        if (!Directory.Exists(Path.Combine(folderPath, ".astra", "objects", "origin", "master")))
            Directory.CreateDirectory(Path.Combine(folderPath, ".astra", "objects", "origin", "master"));

        if (!File.Exists(Path.Combine(folderPath, ".astra", "remotes")))
            await ConfigureRemotes(remoteFolder, folderPath);

        if (!File.Exists(Path.Combine(folderPath, ".astra", "branches"))) await ConfigureBranches(folderPath);

        if (!File.Exists(Path.Combine(folderPath, ".astra", "commits"))) await ConfigureCommits(folderPath);

        if (!File.Exists(Path.Combine(folderPath, ".astra", "description")))
            fileService.Save(Path.Combine(folderPath, ".astra"), "description",
                "Unnamed repository; edit this file 'description' to name the repository.");

        await directoryService.CopyDirectoryAsync(folderPath, Path.Combine(remoteFolder, folderPathName));
    }
    

    private Task ConfigureRemotes(string remoteFolder, string folderPath)
    {
        var content = new RemoteConfig
        {
            Remote =
                new Remote
                {
                    Id = 0,
                    Path = remoteFolder,
                    Name = "origin",
                    BranchIds = [0],
                    DefaultBranchId = 0
                }
        };
        fileService.Save(Path.Combine(folderPath, ".astra"), "remotes", content);
        return Task.CompletedTask;
    }

    private Task ConfigureBranches(string folderPath)
    {
        var branches = new BranchConfig
        {
            Branches =
            [
                new Branch
                {
                    Id = 0,
                    Name = "master",
                    RemoteId = 0
                }
            ]
        };
        fileService.Save(Path.Combine(folderPath, ".astra"), "branches", branches);
        return Task.CompletedTask;
    }

    private Task ConfigureCommits(string folderPath)
    {
        var commits = new CommitConfig
        {
            Commits = []
        };
        fileService.Save(Path.Combine(folderPath, ".astra"), "commits", commits);
        return Task.CompletedTask;
    }
}