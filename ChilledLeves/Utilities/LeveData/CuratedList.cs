using System.Collections.Generic;

namespace ChilledLeves.Utilities.LeveData;

public static partial class LeveInfo
{
    public class SavedList
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<Info_List> Playlist { get; set; } = new();
    }

    public class Info_List
    {
        public uint Id { get; set; }
        public int Amount { get; set; }
    }

    public static readonly List<SavedList> Curated_Levepacks = new();

    public static SavedList? FindList(int id) => Curated_Levepacks.Find(c => c.Id == id);
}
