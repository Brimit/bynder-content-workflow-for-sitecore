﻿using System.Collections.Generic;
using GatherContent.Connector.Entities.Entities;

namespace GatherContent.Connector.IRepositories.Models.Import
{
    public class ImportCMSField
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }
        public string Value { get; set; }

        public List<Option> Options { get; set; }

        public List<FileOld> Files { get; set; }

        public ImportCMSField(string type, string name, string label, string value, List<Option> options, List<FileOld> files)
        {
            Type = type;
            Name = name;
            Label = label;
            Value = value;
            Options = options;
            Files = files;
        }
    }
}
