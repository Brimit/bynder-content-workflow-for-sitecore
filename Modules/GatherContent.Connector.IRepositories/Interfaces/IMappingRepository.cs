﻿using System.Collections.Generic;
using GatherContent.Connector.IRepositories.Models.New.Mapping;

namespace GatherContent.Connector.IRepositories.Interfaces
{
    public interface IMappingRepository
    {
        List<TemplateMapping> GetMappings();
        List<TemplateMapping> GetMappingsByGcProjectId(string projectId);
        List<TemplateMapping> GetMappingsByGcTemplateId(string gcTemplateId); 
        TemplateMapping GetMappingById(string id);

        void CreateMapping(TemplateMapping templateMapping);
        void UpdateMapping(TemplateMapping templateMapping);
        void DeleteMapping(string id);
    }
}
