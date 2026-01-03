using System;
using System.Runtime.InteropServices;
using Dalamud.Game.Gui.ContextMenu;
using Dalamud.Plugin.Services;
using ECommons.Automation;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace Snowfall.Service;

public unsafe class MassMateriaRetrievalService
{
    private static IPluginLog? Logger;
    private static bool IsProcessing;
    private static DateTime LastAction = DateTime.MinValue;

    private static InventoryType TargetContainer;
    private static int TargetSlot;
    private static uint OwnerAddonId;
    private static MenuTargetInventory? CurrentTarget;

    public static void Init(IPluginLog logger)
    {
        Logger = logger; 
        Svc.Framework.Update += OnUpdate;
    }

    public static void Start(
        InventoryType targetContainer, int targetSlot, uint ownerAddonId, MenuTargetInventory? currentTarget = null)
    {
        TargetContainer = targetContainer;
        TargetSlot = targetSlot;
        OwnerAddonId = ownerAddonId;
        CurrentTarget = currentTarget;

        IsProcessing = true;
    }
    
    private static void OnUpdate(IFramework framework)
    {
        if (!IsProcessing || CurrentTarget == null) return;
        
        var targetMateriaCount = CurrentTarget?.TargetItem?.MateriaEntries.Count ?? 0;

        if (targetMateriaCount == 0)
        {
            Logger?.Info("No materia left to retrieve. Ending process.");
            IsProcessing = false;
            return;
        }

        // wait to not spam actions
        if (DateTime.Now < LastAction) return;

        // Handle "MateriaRetrieve" Window
        var retrievePtr = Svc.GameGui.GetAddonByName("MateriaRetrieveDialog"); 
        if (retrievePtr.Address != nint.Zero)
        {
            var addon = (AtkUnitBase*)retrievePtr.Address;
            if (addon->IsVisible)
            {
                Logger?.Debug("Attempting to press 'Begin'...");
                    
                Callback.Fire(addon, true, 0);
                
                Logger?.Debug("Begin pressed. Waiting 3 seconds for animation...");
                LastAction = DateTime.Now.AddMilliseconds(3000);
                return;
            }
        }

        // If no windows are open, try to re-open the context menu
        // We only reach this if windows are closed but materia still exists
        Logger?.Debug("No windows open. Attempting to re-spawn context menu...");
        OpenNativeRetrieveMenu();
        LastAction = DateTime.Now.AddMilliseconds(1200);
    }

    private static void OpenNativeRetrieveMenu()
    {
        var contextMenuPtr = Svc.GameGui.GetAddonByName("ContextMenu");

        if (contextMenuPtr.Address == nint.Zero || !((AtkUnitBase*)contextMenuPtr.Address)->IsVisible)
        {
            Logger?.Debug("Context menu not visible, re-opening via agent...");
            var agent = AgentInventoryContext.Instance();
            if (agent != null)
            {
                agent->OpenForItemSlot(TargetContainer, TargetSlot, 0, OwnerAddonId);
            }

            return;
        }

        var contextMenu = (AtkUnitBase*)contextMenuPtr.Address;
        int index = GetRetrieveMateriaIndex();
        if (index != -1)
        {
            Logger?.Debug($"Clicking 'Retrieve Materia' at index {index}");
            // Use 0 as the "click" action, and index as the row
            Callback.Fire(contextMenu, true, 0, index, 0, 0);
        }
        else
        {
            Logger?.Warning("Could not find 'Retrieve Materia' in the current context menu.");
        }
    }

    private static int GetRetrieveMateriaIndex()
    {
        var addonPtr = Svc.GameGui.GetAddonByName("ContextMenu");
        if (addonPtr.Address == nint.Zero) return -1;

        var addon = (AtkUnitBase*)addonPtr.Address;

        // We scan from index 7 (where the menu items start)
        for (var i = 7; i < addon->AtkValuesCount; i++)
        {
            var atkValue = addon->AtkValues[i];

            if (atkValue.Type 
                is FFXIVClientStructs.FFXIV.Component.GUI.ValueType.String 
                or FFXIVClientStructs.FFXIV.Component.GUI.ValueType.ManagedString)
            {
                byte* textPtr = atkValue.String;
                if (textPtr == null) continue;

                string name = Marshal.PtrToStringUTF8((nint)textPtr) ?? "";

                if (IsRetrieveMateriaString(name))
                {
                    // Visual row index = (AtkValue index - 7) - 1 adjustment
                    var callbackIndex = i - 8;

                    Logger?.Debug($"Match Found: '{name}' at AtkValue[{i}]. Firing Callback Index: {callbackIndex}");
                    return callbackIndex;
                }
            }
        }

        return -1;
    }

    private static bool IsRetrieveMateriaString(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        
        return name.Contains("Retrieve Materia", StringComparison.OrdinalIgnoreCase) ||
               name.Contains("マテリア回収") ||
               name.Contains("Materia zurückgewinnen") ||
               name.Contains("Retirer des matérias");
    }
    
    public static void Dispose()
    {
        IsProcessing = false;
        Svc.Framework.Update -= OnUpdate;
    }
}
