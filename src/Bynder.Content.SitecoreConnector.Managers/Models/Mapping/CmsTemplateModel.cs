using System.Collections.Generic;

namespace Bynder.Content.SitecoreConnector.Managers.Models.Mapping
{
    public class CmsTemplateModel
    {
        public CmsTemplateModel()
        {
            Fields = new List<CmsTemplateFieldModel>();
        }
        public string Name { get; set; }
        public string Id { get; set; }
        public List<CmsTemplateFieldModel> Fields { get; set; }
    }
}
