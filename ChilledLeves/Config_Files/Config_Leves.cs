using ChilledLeves.Enums;
using ChilledLeves.Utilities.LeveData;
using System.Collections.Generic;

namespace ChilledLeves.Config_Files;

public partial class Config
{
    // Playlist Leve Settings
    public Dictionary<uint, int> LeveList { get; set; } = new();
    public List<uint> LeveOrder { get; set; } = new();
    public bool GrabMulti { get; set; } = true;
    public List<uint> FavoriteLeves { get; set; } = new();
    public bool AllowMultiTurnin { get; set; } = true;
    public bool IncreaseDelay { get; set; } = false;
    public bool RepeatLastLeve { get; set; } = false;
    // TODO: Actually re-wire this in, and maybe actually create default best leveling plans with it
    public List<LeveInfo.SavedList> Leve_SavedPlaylist { get; set; } = new();

    // ARR Leve Grind Settings
    public uint ARR_NpcId { get; set; } = 1000970;
    public AssignmentType ARR_SelectedAssignment { get; set; } = AssignmentType.Fisher;
    public StopConditions ARR_StopCondition { get; set; } = StopConditions.Level;
    public uint ARR_StopLevel { get; set; } = 100;
    public Dictionary<uint, List<uint>> Npc_LevePriority { get; set; } = new();

    public void SaveNewPlaylist(string Name, string description)
    {
        var rng = new Random();
        int profileId = rng.Next(1, 50000);
        while (FindPlaylist(profileId) != null)
            profileId = rng.Next(1, 50000);

        List<LeveInfo.Info_List> playlist = new();
        foreach (var leve in LeveOrder)
        {
            playlist.Add(new() { Id = leve, Amount = LeveList[leve] });
        }
        Leve_SavedPlaylist.Add(new()
        {
            Id = profileId,
            Name = Name,
            Description = description,
            Playlist = playlist
        });
        C.Save();
    }

    public LeveInfo.SavedList? FindPlaylist(int id) => Leve_SavedPlaylist.Find(c => c.Id == id);
}
