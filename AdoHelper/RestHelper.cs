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
    public class RestHelper
    {
        public static void AllowPipelinesToUseServiceConnection(string organisation, string project, Guid endpointId)
        {
            EndpointPatchDocument patch = new EndpointPatchDocument();
            patch.Resource.ID = endpointId;
            await CallDevOpsRest("patch", $"https://{organisation}.visualstudio.com/{Uri.EscapeUriString(project)}/_apis/pipelines/pipelinePermissions/endpoint/{endpointId}", JsonConvert.SerializeObject(patch), "application/json", _personalAccessToken);
        }

        public static async Task<string> CallDevOpsRest(string method, string url, string body, string contentType, string accessToken)
        {
            StringContent content = null;
            string response = null;
            using (var httpClient = new HttpClient())
            {
                string header = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($":{accessToken}"));
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {header}");
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json;api-version=5.1-preview.1");
                HttpRequestMessage request;
                HttpResponseMessage responseMessage;
                switch (method)
                {
                    case "get":
                        response = await httpClient.GetStringAsync(new Uri(url));
                        break;
                    case "post":
                        if (body != null)
                        {
                            content = new StringContent(body, System.Text.Encoding.UTF8, contentType);
                        }
                        responseMessage = await httpClient.PostAsync(url, content);
                        response = await responseMessage.Content.ReadAsStringAsync();
                        break;
                    case "put":
                        if (body != null)
                        {
                            content = new StringContent(body, System.Text.Encoding.UTF8, contentType);
                        }
                        responseMessage = await httpClient.PutAsync(url, content);
                        response = await responseMessage.Content.ReadAsStringAsync();
                        break;
                    case "patch":
                        request = new HttpRequestMessage(HttpMethod.Patch, url);
                        if (body != null)
                        {
                            request.Content = new StringContent(body, System.Text.Encoding.UTF8, contentType);
                        }
                        responseMessage = await httpClient.SendAsync(request);
                        response = await responseMessage.Content.ReadAsStringAsync();
                        break;
                    case "delete":
                        request = new HttpRequestMessage(HttpMethod.Delete, url);
                        if (body != null)
                        {
                            request.Content = new StringContent(body, System.Text.Encoding.UTF8, contentType);
                        }
                        responseMessage = await httpClient.SendAsync(request);
                        response = await responseMessage.Content.ReadAsStringAsync();
                        break;
                }
            }
            return response;
        }
    }
}
