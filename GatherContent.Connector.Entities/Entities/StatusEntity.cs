﻿using Newtonsoft.Json;

namespace GatherContent.Connector.Entities.Entities
{
    public class StatusEntity
    {
        [JsonProperty(PropertyName = "data")]
        public Status Data { get; set; }
    }

    public class Status
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "is_default")]
        public bool IsDefault { get; set; }

        [JsonProperty(PropertyName = "position")]
        public string Position { get; set; }

        [JsonProperty(PropertyName = "color")]
        public string Color { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "can_edit")]
        public bool CanEdit { get; set; }
    }
}