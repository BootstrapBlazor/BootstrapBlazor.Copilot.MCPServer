// Licensed to the BootstrapBlazor Community under one or more agreements.
// The BootstrapBlazor Community licenses this file to you under the Apache 2.0 License
// See the LICENSE file in the project root for more information.
// Maintainer: Argo Zhang(argo@live.ca) Website: https://www.blazor.zone

using LibGit2Sharp;

namespace BootstrapBlazor.MCPServer.Services;

public class GitRepositoryManager
{
    private readonly ILogger<GitRepositoryManager> _logger;
    private readonly string _repoUrl = "https://github.com/dotnetcore/BootstrapBlazor.git";
    private readonly string _localPath;
    
    public GitRepositoryManager(ILogger<GitRepositoryManager> logger, IConfiguration configuration)
    {
        _logger = logger;
        
        // Get repository path from configuration or use a default path
        _localPath = configuration["GitRepository:LocalPath"] ?? 
            Path.Combine(Path.GetTempPath(), "BootstrapBlazorRepo");
        
        // Ensure directory exists
        Directory.CreateDirectory(_localPath);
    }

    public bool IsRepositoryUpToDate()
    {
        try
        {
            if (!Repository.IsValid(_localPath))
            {
                _logger.LogInformation("Repository does not exist locally");
                return false;
            }

            using var repo = new Repository(_localPath);
            
            // Fetch from remote to update refs
            var remote = repo.Network.Remotes["origin"];
            Commands.Fetch(repo, remote.Name, new string[] { }, new FetchOptions(), null);
            
            // Compare local and remote branches
            var localBranch = repo.Head;
            var remoteBranch = repo.Branches[$"origin/{localBranch.FriendlyName}"];
            
            if (remoteBranch == null)
            {
                _logger.LogWarning("Remote branch not found");
                return false;
            }
            
            // Check if local is behind remote
            return localBranch.Tip.Sha == remoteBranch.Tip.Sha;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking repository status");
            return false;
        }
    }

    public async Task UpdateRepositoryAsync()
    {
        try
        {
            _logger.LogInformation("Updating repository at {LocalPath}", _localPath);
            
            if (Repository.IsValid(_localPath))
            {
                // Pull latest changes
                using var repo = new Repository(_localPath);
                
                // Get current branch name
                var branchName = repo.Head.FriendlyName;
                
                // Pull changes
                var options = new PullOptions
                {
                    FetchOptions = new FetchOptions()
                };
                
                var signature = new Signature("MCPServer", "mcpserver@example.com", DateTimeOffset.Now);
                
                Commands.Pull(repo, signature, options);
                _logger.LogInformation("Repository updated successfully");
            }
            else
            {
                // Clone the repository
                _logger.LogInformation("Cloning repository from {RepoUrl}", _repoUrl);
                
                // Delete directory if it exists but is not a valid repository
                if (Directory.Exists(_localPath))
                {
                    Directory.Delete(_localPath, true);
                }
                
                Directory.CreateDirectory(_localPath);
                
                // Clone asynchronously using LibGit2Sharp
                await Task.Run(() => Repository.Clone(_repoUrl, _localPath));
                
                _logger.LogInformation("Repository cloned successfully");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating repository");
            throw;
        }
    }

    public string GetRepositoryPath() => _localPath;
}