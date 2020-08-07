using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Build.WebApi;

namespace Helpers
{
    public partial class AdoHelper : IAdoHelper
    {
        public async Task<BuildDefinition> GetPipeline(string projectName, string pipelineName)
        {
            List<BuildDefinition> definitions = await Build.GetFullDefinitionsAsync(project: projectName, name: pipelineName);
            if (definitions == null || definitions.Count == 0)
            {
                return null;
            }

            return definitions[0];
        }

        public async Task<BuildDefinition> CreatePipeline(string name, string projectName, string repositoryName, string filename, string folderPath)
        {
            BuildDefinition definition = new BuildDefinition
            {
                Name = name,
                Description = $"A pipeline for {name} based on {filename}"
            };

            BuildRepository repository = new BuildRepository
            {
                Name = repositoryName,
                Type = "tfsgit",
                DefaultBranch = "master"
            };
            definition.Repository = repository;
            definition.Path = folderPath;

            var process = new YamlProcess
            {
                YamlFilename = filename
            };
            definition.Process = process;

            definition.Queue = new AgentPoolQueue
            {
                Id = await GetAgentQueueByHeuristic(projectName)
            };

            ContinuousIntegrationTrigger ciTrigger = new ContinuousIntegrationTrigger();
            ciTrigger.SettingsSourceType = 2;
            definition.Triggers.Add(ciTrigger);

            BuildDefinition pipeline = null;
            try
            {
                pipeline = await Build.CreateDefinitionAsync(definition, projectName);
            }
            catch (DefinitionExistsException)
            {
                pipeline = await GetPipeline(projectName, name);
            }

            return pipeline;
        }
    }
}
