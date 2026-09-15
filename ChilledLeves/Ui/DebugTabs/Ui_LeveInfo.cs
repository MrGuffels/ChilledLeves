using ChilledLeves.Gui;
using ChilledLeves.Utilities;
using ChilledLeves.Utilities.LogInfo;
using Dalamud.Interface.Utility.Raii;
using ECommons.Automation.NeoTaskManager;
using ECommons.ExcelServices;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using System.Collections.Generic;

namespace ChilledLeves.Ui.DebugTabs;

internal static unsafe class Ui_LeveInfo
{
    public static void Draw()
    {
        using var child = ImRaii.Child("Leve List Info", default, true);

        var handler = GetHandler();
        if (handler == null)
        {
            ImGui.TextDisabled("No active GuildleveAssignmentEventHandler found.");
            return;
        }

        ImGui.Text($"Current type: {handler->CurrentLeveType}");
        ImGui.Text($"Selected leve id: {handler->SelectedLeveId}");
        ImGui.Separator();

        if (GenericHelpers.TryGetAddonMaster<GuildLeve>(out var guildLeve) && guildLeve.IsAddonReady)
        {
            ImGui.Text($"Current job: {guildLeve.CurrentJob}");
            if (ImGui.Button("Refresh List"))
            {
                Refresh_List(guildLeve);
            }
            ImGui.SameLine();
            if (ImGui.Button("Log Handler State"))
            {
                if (handler != null)
                    LogHandlerState(handler);
            }
        }

        DrawLeveTable(handler);
    }

    // GuildLeveAssignment event handler ID, discovered via HaselDebug plugin
    // EventFramework -> EventHandlers tab was where tf this existed (thank you hasel...)
    // Might need to double check this on a major patch to see if it breaks?
    private static readonly uint GuildleveAssignmentHandlerId = 0x60023;
    private static GuildleveAssignmentEventHandler* GetHandler()
    {
        var framework = EventFramework.Instance();
        if (framework == null)
            return null;

        var handler = framework->GetEventHandlerById(GuildleveAssignmentHandlerId);
        return (GuildleveAssignmentEventHandler*)handler;
    }

    private static List<GuildleveAssignmentEventHandler.GuildleveAssignmentLeve> GetVisibleLeves(GuildleveAssignmentEventHandler* handler)
    {
        var result = new List<GuildleveAssignmentEventHandler.GuildleveAssignmentLeve>();

        foreach (var categoryList in handler->AssignmentLists)
            foreach (var group in categoryList.Groups)
                foreach (var subList in group.SubLists)
                    foreach (var leve in subList.Leves)
                        result.Add(leve);

        return result;
    }

    private static void DrawLeveTable(GuildleveAssignmentEventHandler* handler)
    {
        var leves = GetVisibleLeves(handler);

        if (!ImGui.BeginTable("LeveTable", 4, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.Sortable | ImGuiTableFlags.SizingFixedFit))
            return;

        ImGui.TableSetupColumn("Leve ID");
        ImGui.TableSetupColumn("Name");
        ImGui.TableSetupColumn("Level");
        ImGui.TableSetupColumn("Genre Icon");
        ImGui.TableHeadersRow();

        foreach (var leve in leves)
        {
            ImGui.TableNextRow();

            ImGui.TableNextColumn();
            ImGui.Text(leve.LeveId.ToString());

            ImGui.TableNextColumn();
            ImGui.TextUnformatted(leve.Name.ToString());

            ImGui.TableNextColumn();
            ImGui.Text(leve.ClassJobLevel.ToString());

            ImGui.TableNextColumn();
            GameIcons.DrawInlineOrIcon(leve.GenreIcon, FontAwesomeIcon.Book);
        }

        ImGui.EndTable();
    }

    private static unsafe HashSet<ushort> _seenLeveIds = new();
    private static List<GuildleveAssignmentEventHandler.GuildleveAssignmentLeve> _allLeves = new();

    private static readonly List<Job> ValidJobs = new()
    {
        Job.CRP, Job.BSM, Job.ARM, Job.GSM, Job.WVR, Job.LTW, Job.ALC, Job.CUL,
        Job.MIN, Job.BTN, Job.FSH
    };

    private static int _throttle = 0;

    private static void Refresh_List(GuildLeve guildLeve)
    {
        _seenLeveIds.Clear();
        _allLeves.Clear();

        const string tag = "Tab Scan";

        bool correctJob(Job job)
        {
            if (guildLeve.CurrentJob == job)
            {
                IceLogging.Verbose($"Job button clicked for: {job}", tag);
                return true;
            }
            else
            {
                guildLeve.SelectJob(job);

                IceLogging.Verbose($"Still switching category for {job}", tag);
                return false;
            }
        }
        bool throttleCount()
        {
            if (EzThrottler.Throttle("Counter throttle", 50))
                _throttle += 1;

            if (_throttle == 2)
            {
                _throttle = 0;
                return true;
            }

            return false;
        }

        bool addLeveInfo(Job job)
        {
            if (guildLeve.CurrentJob != job)
            {
                guildLeve.SelectJob(job);

                IceLogging.Verbose($"Waiting for {job}, currently {guildLeve.CurrentJob?.ToString() ?? "null"}", tag);
                return false;
            }

            var handler = GetHandler();
            if (handler == null)
                return false;

            var newLeves = GetVisibleLeves(handler);
            if (newLeves.Count == 0)
                return false;

            IceLogging.Verbose($"Capturing leves for: {job}", tag);

            foreach (var leve in newLeves)
                if (_seenLeveIds.Add(leve.LeveId))
                    _allLeves.Add(leve);

            return true;
        }

        foreach (var job in ValidJobs)
        {
            P.taskManager.EnqueueMulti
            (
                new(() => correctJob(job), $"Selecting: {job}"),
                new(() => throttleCount(), "Waiting a sec"),
                new(() => addLeveInfo(job), $"Adding leve info for: {job}")
            );
        }
    }
    public static unsafe void LogHandlerState(GuildleveAssignmentEventHandler* handler)
    {
        const string tag = "Handler State";

        IceLogging.Verbose($"CurrentLeveType: {handler->CurrentLeveType}", tag);
        IceLogging.Verbose($"SelectedLeveId: {handler->SelectedLeveId}", tag);
        IceLogging.Verbose($"SelectedGatheringLeveId: {handler->SelectedGatheringLeveId}", tag);
        IceLogging.Verbose($"CategorySelection: [{string.Join(", ", handler->CategorySelection.ToArray())}]", tag);
        IceLogging.Verbose($"ListLeveId: [{string.Join(", ", handler->ListLeveId.ToArray())}]", tag);
    }
}