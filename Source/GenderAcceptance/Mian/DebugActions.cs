using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using HarmonyLib;
using LudeonTK;
using RimWorld;
using Verse;

namespace GenderAcceptance.Mian;

public static class DebugActions
{
    private static readonly IEnumerable<FieldInfo> TransKnowledgeProperties =
        typeof(TransKnowledgeTracker).GetFields().Where(propertyInfo => propertyInfo.FieldType == typeof(bool));
    
    [DebugAction("Pawns", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
    private static void EditTransKnowledge(Pawn p)
    {
        foreach (var pawn in p.Map.mapPawns.AllPawns.Where(pawn => (pawn.RaceProps?.Humanlike ?? false) && pawn != p))
        {
            p.GetKnowledgeOnPawn(pawn); // create tracker for all of them
        }
        
        var list = p.GetTransgenderKnowledges(false);
        List<DebugMenuOption> options = list.Select(tracker => new DebugMenuOption(
            tracker.Pawn.LabelShort + " (" + tracker.Pawn.KindLabel + ")", DebugMenuOptionMode.Action, () =>
            {
                Func<List<DebugMenuOption>> createAttributes = null;
                createAttributes = () =>
                     TransKnowledgeProperties.Select(fieldInfo =>
                    {
                        var valueName = fieldInfo.Name;
                        var value = (bool) fieldInfo.GetValue(tracker);
                        var option = new DebugMenuOption(valueName + ": " + value, DebugMenuOptionMode.Action,
                            () =>
                            {
                                var newValue = !value;
                                fieldInfo.SetValue(tracker, newValue);

                                Find.WindowStack.Add(new Dialog_DebugOptionListLister(createAttributes()));
                            });

                        return option;
                    }).ToList();
                
                Find.WindowStack.Add(new Dialog_DebugOptionListLister(createAttributes()));
            })).ToList();
        Find.WindowStack.Add(new Dialog_DebugOptionListLister(options));
    }
}