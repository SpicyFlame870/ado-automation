using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Models
{
    public class EndpointPatchDocument
    {
        [JsonProperty("allPipelines")]
        public AllPipelinesObject AllPipelines { get; set; }
        public class AllPipelinesObject
        {
            [JsonProperty("authorized")]
            public bool Authorised { get; set; }
            [JsonProperty("authorizedBy")]
            public string AuthorisedBy { get; set; }
            [JsonProperty("authorizedOn")]
            public DateTime? AuthorisedOn { get; set; }
            public AllPipelinesObject()
            {
                Authorised = true;
                AuthorisedBy = null;
                AuthorisedOn = null;
            }
        }
        [JsonProperty("pipelines")]
        public string Pipelines { get; set; }
        [JsonProperty("resource")]
        public ResourceObject Resource { get; set; }
        public class ResourceObject
        {
            [JsonProperty("id")]
            public Guid ID { get; set; }
            [JsonProperty("type")]
            public string Type { get; set; }
            public ResourceObject()
            {
                Type = "endpoint";
            }
        }
        public EndpointPatchDocument()
        {
            AllPipelines = new AllPipelinesObject();
            Resource = new ResourceObject();
        }
    }
}