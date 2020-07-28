using System.Collections.Generic;

namespace BSTClient.API.Models.Response
{
    public class NavObj
    {
        public List<SectionObj> Sections { get; set; } = new List<SectionObj>();
    }
    public class SectionObj
    {

        public int Id { get; set; }
        public string Tag { get; set; }
        public string Name { get; set; }
        public string IconString { get; set; }
        public List<ItemObj> Items { get; set; } = new List<ItemObj>();
    }

    public class ItemObj
    {
        public int Id { get; set; }
        public string Tag { get; set; }
        public string Name { get; set; }
        public string IconString { get; set; }
    }
}
