namespace Bynder.Content.SitecoreConnector.Core.Entities
{
    using System.Collections.Generic;

    using Newtonsoft.Json;

    public class ProjectsEntity
    {
        [JsonProperty(PropertyName = "data")]
        public List<Project> Data { get; set; }

    }
}
