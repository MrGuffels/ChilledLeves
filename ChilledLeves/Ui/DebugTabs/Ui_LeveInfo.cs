using ChilledLeves.Gui;
using ChilledLeves.Utilities;
using ChilledLeves.Utilities.LeveData;
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
        using (var child = ImRaii.Child("Leve List Info", default, true))
        {

            if (ImGui.Button("Check Handlers"))
            {
                LeveInfo.DebugDumpHandlerState();
            }

            var handler = LeveInfo.GetHandler();
            if (handler == null)
            {
                ImGui.TextDisabled("No active GuildleveAssignmentEventHandler found.");
                return;
            }

            ImGui.Text($"Current type: {handler->CurrentLeveType}");
            ImGui.Text($"Selected leve id: {handler->SelectedLeveId}");

            if (Svc.Targets.Target != null)
            {
                var target = Svc.Targets.Target;

                ImGui.Text($"Name: {target.Name} | ID: {target.BaseId}");
            }
            ImGui.Separator();

            DrawLeveTable(handler);
        }
    }

    private static void DrawLeveTable(GuildleveAssignmentEventHandler* handler)
    {
        var leves = LeveInfo.GetVisibleLeves(handler);

        if (leves.Count() != 0)
        {
            if (ImGui.Button("Copy missing ID's"))
            {
                ImGui.SetClipboardText(AddMissingLeves(leves));
            }
        }

        using (var table = ImRaii.Table("LeveTable", 4, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.Sortable | ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.ScrollY))
        {
            if (!table.Success)
                return;

            ImGui.TableSetupColumn("Leve ID");
            ImGui.TableSetupColumn("Name");
            ImGui.TableSetupColumn("Level");
            ImGui.TableSetupColumn("Genre Icon");

            ImGui.TableSetupScrollFreeze(0, 1);
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
        }
    }

    private static string AddMissingLeves(List<GuildleveAssignmentEventHandler.GuildleveAssignmentLeve> leves)
    {
        var target = Svc.Targets.Target;
        var baseId = target.BaseId;

        List<uint> newLeves = new();

        if (LeveInfo.Levemete_Info.TryGetValue(baseId, out var vendorInfo))
        {
            foreach (var leve in leves)
            {
                if (!vendorInfo.Leves.Contains(leve.LeveId))
                    newLeves.Add(leve.LeveId);
            }
        }

        return string.Join(", ", newLeves);
    }
}