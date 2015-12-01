﻿using System.Collections.Generic;
using GatherContent.Connector.Entities.Entities;

namespace GatherContent.Connector.IRepositories.Models.Import
{
    public class ImportCMSFiled
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }

        public List<Option> Options { get; set; }

        public ImportCMSFiled(string type, string name, string value, List<Option> options)
        {
            Type = type;
            Name = name;
            Value = value;
            Options = options;
        }
    }
}
