using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Models
{
    public class MinimumReviewersPolicySettings
    {
        [JsonProperty("minimumApproverCount")]
        public int MinimumApproverCount { get; set; }

        [JsonProperty("creatorVoteCounts")]
        public bool CreatorVoteCounts { get; set; }

        [JsonProperty("allowDownvotes")]
        public bool AllowDownvotes { get; set; }

        [JsonProperty("resetOnSourcePush")]
        public bool ResetOnSourcePush { get; set; }

        [JsonProperty("blockLastPusherVote")]
        public bool BlockLastPusherVote { get; set; }

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

        public MinimumReviewersPolicySettings()
        {
            MinimumApproverCount = 1;
            CreatorVoteCounts = true;
            AllowDownvotes = false;
            ResetOnSourcePush = false;
            BlockLastPusherVote = false;
            Scope = new List<ScopeEntry>();
        }
    }
}
