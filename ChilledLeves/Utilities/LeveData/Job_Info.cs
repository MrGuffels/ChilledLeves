using ChilledLeves.Enums;
using Dalamud.Interface.Textures;
using ECommons.ExcelServices;
using Lumina.Excel.Sheets;
using System.Collections.Generic;

namespace ChilledLeves.Utilities.LeveData;

public static partial class LeveInfo
{
    private static HashSet<AssignmentType> Assignment_Crafters = new()
    { 
        AssignmentType.Carpenter, AssignmentType.Blacksmith, 
        AssignmentType.Armorer, AssignmentType.Goldsmith, 
        AssignmentType.Leatherworker, AssignmentType.Weaver, 
        AssignmentType.Alchemist, AssignmentType.Culinarian, 
        AssignmentType.Fisher 
    };
    private static HashSet<AssignmentType> Assignment_Gathering = new() { AssignmentType.Miner, AssignmentType.Botanist};
    private static HashSet<AssignmentType> Assignment_Battle = new() { AssignmentType.Battlecraft, AssignmentType.Maelstorm, AssignmentType.TwinAdder, AssignmentType.ImmortalFlames };

    public static readonly List<Job> LeveJobs_Material = new() { Job.CRP, Job.BSM, Job.ARM, Job.GSM, Job.WVR, Job.LTW, Job.ALC, Job.CUL, Job.FSH };
    public static readonly List<Job> LeveJobs_Gathering = new() { Job.MIN, Job.BTN };

    public static Dictionary<Leve_Status, uint> Leve_State = new()
    {
        [Leve_Status.NotGrabbed] = 71041,
        [Leve_Status.NotComplete] = 71045,
        [Leve_Status.Complete] = 71055,
    };

    public class Info_Assignment
    {
        public ISharedImmediateTexture ColorIcon { get; set; } = null;
        public uint IconId { get; set; } = 0;
        public string Name { get; set; } = "???";
    }

    public static Dictionary<AssignmentType, Info_Assignment> Assignment_IconDict = new();

    public static void UpdateJobIcons()
    {
        var LeveAssignmentSheet = Svc.Data.GetExcelSheet<LeveAssignmentType>();
        for (uint i = 1; i < 16; i++)
        {
            AssignmentType type = (AssignmentType)i;
            var row = LeveAssignmentSheet.GetRow(i);
            var iconId = row.Icon;
            var name = row.Name.ToString();

            if (Svc.Texture.TryGetFromGameIcon(iconId, out var iconTexture))
            {
                Assignment_IconDict[type] = new()
                {
                    Name = name,
                    IconId = (uint)iconId,
                    ColorIcon = iconTexture
                };
            }
        }

        // for specifically the "all" category. Just that way I have it
        int allIcon = 71061;

        if (Svc.Texture.TryGetFromGameIcon(allIcon, out var allTexture))
        {
            Assignment_IconDict[AssignmentType.None] = new()
            {
                Name = "All",
                IconId = (uint)allIcon,
                ColorIcon = allTexture
            };
        }
    }
}
