using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.Policy.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Models;
using Newtonsoft.Json.Linq;
using System;

namespace Helpers
{
    public partial class AdoHelper : IAdoHelper
    {
        public async Task<List<PolicyType>> GetPolicyTypes(string projectName)
        {
            List<PolicyType> types = await Policy.GetPolicyTypesAsync(projectName);
            return types;
        }
        
        public async Task<PolicyConfiguration> AddMinimumReviewersPolicy(string projectName, string repositoryName)
        {
            PolicyConfiguration config = new PolicyConfiguration
            {
                IsBlocking = true,
                IsEnabled = true
            };

            List<PolicyType> types = await Policy.GetPolicyTypesAsync(projectName);
            config.Type = types.First(pt => pt.DisplayName == "Minimum number of reviewers");

            GitRepository repo = await Git.GetRepositoryAsync(projectName, repositoryName);

            MinimumReviewersPolicySettings settings = new MinimumReviewersPolicySettings();

            settings.Scope.Add(new MinimumReviewersPolicySettings.ScopeEntry()
            {
                RefName = "refs/heads/master",
                MatchKind = "Exact",
                RepositoryID = repo.Id.ToString()
            });
            config.Settings = (JObject)JToken.FromObject(settings);

            return await Policy.CreatePolicyConfigurationAsync(config, projectName);
        }

        public async Task<PolicyConfiguration> AddRequiredReviewersPolicy(string projectName, string repositoryName, List<Guid> reviewers)
        {
            PolicyConfiguration config = new PolicyConfiguration
            {
                IsBlocking = true,
                IsEnabled = true
            };

            List<PolicyType> types = await Policy.GetPolicyTypesAsync(projectName);
            config.Type = types.First(pt => pt.DisplayName == "Required reviewers");

            GitRepository repo = await Git.GetRepositoryAsync(projectName, repositoryName);

            RequiredReviewersPolicySettings settings = new RequiredReviewersPolicySettings();
            settings.RequiredReviewerIds.AddRange(reviewers);

            settings.Scope.Add(new RequiredReviewersPolicySettings.ScopeEntry()
            {
                RefName = "refs/heads/master",
                MatchKind = "Exact",
                RepositoryID = repo.Id.ToString()
            });
            config.Settings = (JObject)JToken.FromObject(settings);

            return await Policy.CreatePolicyConfigurationAsync(config, projectName);
        }

        public async Task<PolicyConfiguration> AddBuildPolicy(string projectName, string repositoryName, string pipelineName)
        {
            PolicyConfiguration config = new PolicyConfiguration
            {
                IsBlocking = true,
                IsEnabled = true
            };

            List<PolicyType> types = await Policy.GetPolicyTypesAsync(projectName);
            config.Type = types.First(pt => pt.DisplayName == "Build");

            GitRepository repo = await Git.GetRepositoryAsync(projectName, repositoryName);
            
            BuildPolicySettings settings = new BuildPolicySettings("build policy");
            
            BuildDefinition definition = await GetPipeline(projectName, pipelineName);
            settings.BuildDefinitionId = definition.Id;

            settings.FilenamePatterns.Add("/*");
            settings.FilenamePatterns.Add("!**/*.md");
            settings.Scope.Add(new BuildPolicySettings.ScopeEntry() {
                RefName = "refs/heads/master",
                MatchKind = "Exact",
                RepositoryID = repo.Id.ToString()
            });
            config.Settings = (JObject)JToken.FromObject(settings);

            return await Policy.CreatePolicyConfigurationAsync(config, projectName);
        }
    }
}
