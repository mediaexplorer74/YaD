using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ya.D.Models
{
    public class ItemList
    {
        public uint ItemID { get; set; }
        public virtual DiskItem Item { get; set; }

        public uint PlayListID { get; set; }
        public virtual PlayList PlayList { get; set; }
    }

    public class PlayList
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public uint ID { get; set; }
        public string Name { get; set; }
        public uint TypeID { get; set; }
        public PlayListType Type { get; set; }
        public List<ItemList> Items { get; set; }
        [NotMapped]
        public List<DiskItem> DiskItems { get; set; } = new List<DiskItem>();

        public PlayList()
        {
            Items = new List<ItemList>();
        }
    }

    public class PlayListType
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public uint ID { get; set; }
        public string Name { get; set; }
    }
}
