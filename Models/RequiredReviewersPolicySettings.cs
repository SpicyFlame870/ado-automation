using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Models
{
    public class RequiredReviewersPolicySettings
    {
        [JsonProperty("requiredReviewerIds")]
        public List<Guid> RequiredReviewerIds { get; set; }

        [JsonProperty("minimumApproverCount")]
        public int MinimumApproverCount { get; set; }

        [JsonProperty("creatorVoteCounts")]
        public bool CreatorVoteCounts { get; set; }

        public class ScopeEntry
        {
            [JsonProperty("refName")]
            public string RefName { get; set; }

            [JsonProperty("matchKind")]
            public string MatchKind { get; set; }

            [JsonProperty("repositoryId")]
            public string RepositoryID { get; set; }
        }

        [JsonProperty("scope")]
        public List<ScopeEntry> Scope { get; set; }

        public RequiredReviewersPolicySettings()
        {
            RequiredReviewerIds = new List<Guid>();
            MinimumApproverCount = 1;
            Scope = new List<ScopeEntry>();
        }
    }
}
