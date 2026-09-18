using ChilledLeves.Utilities.LogInfo;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using System.Collections.Generic;

namespace ChilledLeves.Utilities.LeveData;

public static partial class LeveInfo
{
    public static unsafe GuildleveAssignmentEventHandler* GetHandler()
    {
        var framework = EventFramework.Instance();
        if (framework == null)
            return null;

        foreach (var kv in framework->EventHandlerModule.EventHandlerMap)
        {
            var eventHandler = kv.Item2.Value;
            if (eventHandler == null)
                continue;

            if (eventHandler->Info.EventId.ContentId == EventHandlerContent.GuildLeveAssignment)
                return (GuildleveAssignmentEventHandler*)eventHandler;
        }

        return null;
    }

    public static unsafe List<GuildleveAssignmentEventHandler.GuildleveAssignmentLeve> GetVisibleLeves(GuildleveAssignmentEventHandler* handler)
    {
        var result = new List<GuildleveAssignmentEventHandler.GuildleveAssignmentLeve>();

        foreach (var categoryList in handler->AssignmentLists)
            foreach (var group in categoryList.Groups)
                foreach (var subList in group.SubLists)
                    foreach (var leve in subList.Leves)
                        result.Add(leve);

        return result;
    }

    public static unsafe List<uint> LoadedList()
    {
        var handler = GetHandler();
        if (handler == null)
            return new List<uint>();

        var leveList = new List<uint>();
        foreach (var leve in GetVisibleLeves(handler))
            leveList.Add(leve.LeveId);

        return leveList;
    }

    public static unsafe void DebugDumpHandlerState()
    {
        const string tag = "Handler Dump";

        var framework = EventFramework.Instance();
        if (framework == null)
        {
            IceLogging.Warning("[LeveInfo] EventFramework.Instance() is null", tag);
            return;
        }

        var handler = GetHandler();
        foreach (var kv in framework->EventHandlerModule.EventHandlerMap)
        {
            var eventHandler = kv.Item2.Value;
            if (eventHandler == null)
                continue;

            IceLogging.Debug($"[LeveInfo] Handler key={kv.Item1:X} ContentId={eventHandler->Info.EventId.ContentId} EntryId={eventHandler->Info.EventId.EntryId:X}", tag);
        }

        if (handler == null)
        {
            IceLogging.Warning("[LeveInfo] No GuildLeveAssignment handler found in EventHandlerMap", tag);
            return;
        }

        var catCount = 0;
        var groupCount = 0;
        var subListCount = 0;
        var leveCount = 0;

        foreach (var categoryList in handler->AssignmentLists)
        {
            catCount++;
            foreach (var group in categoryList.Groups)
            {
                groupCount++;
                foreach (var subList in group.SubLists)
                {
                    subListCount++;
                    leveCount += subList.Leves.Count;
                }
            }
        }

        IceLogging.Debug($"[LeveInfo] Categories={catCount} Groups={groupCount} SubLists={subListCount} Leves={leveCount}", tag);
    }
}