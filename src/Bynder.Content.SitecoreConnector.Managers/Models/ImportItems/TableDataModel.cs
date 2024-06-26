using System.Collections.Generic;

namespace Bynder.Content.SitecoreConnector.Managers.Models.ImportItems
{
    public class TableDataModel
    {
        public List<ImportListItem> Items { get; set; }

        public TableDataModel()
        {
            Items = new List<ImportListItem>();
        }

        public TableDataModel(List<ImportListItem> items)
        {
            Items = new List<ImportListItem>();
            if (items != null)
            {
                Items = items;
            }
        }
    }
}
