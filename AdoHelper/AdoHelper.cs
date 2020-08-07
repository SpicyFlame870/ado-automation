using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.DistributedTask.WebApi;
using Microsoft.TeamFoundation.Policy.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

namespace Helpers
{
    public partial class AdoHelper : IAdoHelper
    {
        private string _organisation;
        private string _pat;

        private VssCredentials _credentials;
        private VssConnection _connection;

        public GitHttpClient Git { get { return _connection.GetClient<GitHttpClient>(); }}
        public BuildHttpClient Build { get { return _connection.GetClient<BuildHttpClient>(); }}
        public TaskAgentHttpClient TaskAgent { get { return _connection.GetClient<TaskAgentHttpClient>(); }}
        public PolicyHttpClient Policy { get { return _connection.GetClient<PolicyHttpClient>(); }}

        public AdoHelper([FromServices]IOptions<ServiceOptions> options)
        {
            _pat = options.Value.PersonalAccessToken;

            _organisation = options.Value.Organisation;

            _credentials = new VssBasicCredential(string.Empty, _pat);

            _connection = new VssConnection(new Uri($"https://dev.azure.com/{_organisation}"), _credentials);
        }

        private async Task<int> GetAgentQueueByHeuristic(string projectName)
        {
            var queues = await TaskAgent.GetAgentQueuesAsync(project: projectName);

            var ubuntu = queues.First(a => a.Name == "Hosted Ubuntu 1604");
            if (ubuntu != null)
            {
                return ubuntu.Id;
            }

            var hosted = queues.First(a => a.Pool.IsHosted == true);
            if (hosted != null) 
            {
                return hosted.Id;
            }

            return -1;
        }
    }
}
