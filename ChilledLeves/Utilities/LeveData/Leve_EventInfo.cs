using FFXIVClientStructs.FFXIV.Client.Game.Event;
using System;
using System.Collections.Generic;

namespace ChilledLeves.Utilities.LeveData;

public static partial class LeveInfo
{
    private static unsafe GuildleveAssignmentEventHandler* GetHandler()
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

    private static unsafe List<GuildleveAssignmentEventHandler.GuildleveAssignmentLeve> GetVisibleLeves(GuildleveAssignmentEventHandler* handler)
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
        List<uint> leveList = new();

        var handler = GetHandler();
        if (handler == null)
            return leveList;

        foreach (var leve in GetVisibleLeves(handler))
        {
            leveList.Add(leve.LeveId);
        }
        return leveList;
    }
}