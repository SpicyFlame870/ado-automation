using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Common;

namespace Helpers
{
    public partial class AdoHelper : IAdoHelper
    {
        public async Task<GitRepository> GetRepository(string projectName, string repositoryName)
        {
            GitRepository repo = null;
            try
            {
                repo = await Git.GetRepositoryAsync(projectName, repositoryName);
            }
            catch (VssServiceException)
            {
            }

            return repo;
        }

        public async Task<GitRepository> CreateRepository(string projectName, string repositoryName, bool initialise)
        {
            GitRepository repo = null;
            try
            {
                GitRepositoryCreateOptions options = new GitRepositoryCreateOptions
                {
                    Name = repositoryName
                };
                repo = await Git.CreateRepositoryAsync(options, projectName, null, null, default);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create new repository: {repositoryName}", ex);
            }

            if (initialise)
            {
                GitRefUpdate newBranch = new GitRefUpdate
                {
                    Name = "refs/heads/master",
                    OldObjectId = new string('0', 40)
                };

                GitCommitRef newCommit = new GitCommitRef()
                {
                    Comment = "Add a sample file",
                    Changes = new GitChange[]
                    {
                        new GitChange()
                        {
                            ChangeType = VersionControlChangeType.Add,
                            Item = new GitItem() { Path = "/readme.md" },
                            NewContent = new ItemContent()
                            {
                                Content = "Empty file for initial commit",
                                ContentType = ItemContentType.RawText,
                            },
                        }
                    },
                };

                try
                {
                    _ = await Git.CreatePushAsync(new GitPush()
                    {
                        RefUpdates = new GitRefUpdate[] { newBranch },
                        Commits = new GitCommitRef[] { newCommit },
                    }, repo.Id);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Could not initialise repository: {repositoryName}", ex);
                }
            }

            return repo;
        }

        public async Task CreateBranch(string projectName, string repositoryName, string branchName)
        {
            string refId = new string('0', 40);
            string defaultBranchName = "heads/master";
            GitRepository repo = null;

            try
            {
                repo = await GetRepository(projectName, repositoryName);
            }
            catch (Exception ex)
            {
                if (repo == null)
                {
                    throw new Exception($"Repository does not exist: {repositoryName}", ex);
                }                
            }

            try
            {
                List<GitRef> refs = await Git.GetRefsAsync(repo.Id, filter: defaultBranchName);
                if (refs.Count > 0)
                {
                    GitRef defaultBranch = refs.First();
                    refId = defaultBranch.ObjectId;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Could not get reference to default branch; repositoryName = {repositoryName}, branchName = {defaultBranchName}", ex);
            }

            GitRefUpdate refUpdate = new GitRefUpdate()
            {
                Name = $"refs/heads/{branchName}",
                NewObjectId = refId,
                OldObjectId = new string('0', 40),
                IsLocked = false,
                RepositoryId = repo.Id
            };

            try
            {
                await Git.UpdateRefsAsync(new GitRefUpdate[] { refUpdate } , repositoryId: repo.Id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Could not create new branch; repositoryName = {repositoryName}, branchName = {branchName}", ex);
            }
        }

        public async Task DeleteRepository(string projectName, string repositoryName, bool permanently)
        {
            GitRepository repo = null;
            
            try
            {
                repo = await GetRepository(projectName, repositoryName);
            }
            catch (Exception)
            {
                return;
            }

            if (repo != null)
            {
                await Git.DeleteRepositoryAsync(repo.Id);

                if (permanently)
                {
                    await Git.DeleteRepositoryFromRecycleBinAsync(projectName, repo.Id);
                }
            }
        }

        public async Task CopyFolderToRepository(GitRepository sourceRepository, string directoryName, GitRepository targetRepository, ContentReplacer replacer)
        {
            string refId = new string('0', 40);
            string defaultBranchName = "heads/master";
            List<GitRef> refs = null;
            try
            {
                refs = await Git.GetRefsAsync(targetRepository.Id, filter: defaultBranchName);
                if (refs.Count > 0)
                {
                    GitRef defaultBranch = refs.First();
                    refId = defaultBranch.ObjectId;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Could not get master ref", ex);
            }

            GitRefUpdate refUpdate = new GitRefUpdate()
            {
                Name = "refs/heads/master",
                OldObjectId = refId
            };

            List<GitChange> changes = new List<GitChange>();

            List<GitItem> items = null;
            try
            {
                items = await Git.GetItemsAsync(sourceRepository.Id, scopePath: directoryName, recursionLevel: VersionControlRecursionType.Full);

                foreach (GitItem item in items)
                {
                    if (item.GitObjectType == GitObjectType.Blob)
                    {
                        Stream stream = await Git.GetItemContentAsync(repositoryId: sourceRepository.Id, path: item.Path);
                        StreamReader reader = new StreamReader(stream);
                        string content = reader.ReadToEnd();
                        string newPath = item.Path.Remove(0, directoryName.Length);

                        if (replacer != null)
                        {
                            content = replacer.ReplaceContent(newPath, content);
                        }

                        GitChange change = new GitChange
                        {
                            ChangeType = VersionControlChangeType.Add,
                            Item = new GitItem() { Path = newPath },
                            NewContent = new ItemContent()
                            {
                                Content = content,
                                ContentType = ItemContentType.RawText,
                            }
                        };
                        changes.Add(change);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error copying files", ex);
            }

            GitCommitRef newCommit = new GitCommitRef()
            {
                Comment = $"Copying files from {sourceRepository.Name}",
                Changes = changes.ToArray()
            };

            try
            {
                _ = await Git.CreatePushAsync(new GitPush()
                {
                    RefUpdates = new GitRefUpdate[] { refUpdate },
                    Commits = new GitCommitRef[] { newCommit },
                }, targetRepository.Id);
            }
            catch (Exception ex)
            {
                throw new Exception("Error pushing commit", ex);
            }
        }

        public async Task<List<GitCommitRef>> GetLatestCommits(string projectName, string repositoryName, int commits)
        {
            GitRepository repo = await GetRepository(projectName, repositoryName);
            if (repo == null)
            {
                throw new ModuleRepositoryNotFoundException();
            }

            GitQueryCommitsCriteria criteria = new GitQueryCommitsCriteria
            {
                Top = commits
            };
            List<GitCommitRef> refs = await Git.GetCommitsAsync(repositoryId: repo.Id, searchCriteria: criteria);
            return refs;
        }
    }
}
