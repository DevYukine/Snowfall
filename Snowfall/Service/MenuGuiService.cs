using System.Linq;
using Dalamud.Game.Gui.ContextMenu;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Snowfall.Service;
using InventoryType = FFXIVClientStructs.FFXIV.Client.Game.InventoryType;

namespace Snowfall;

public static unsafe class MenuGuiService
{
    private static IContextMenu? ContextMenu;
    private static IPluginLog? Logger;

    public static void Init(IContextMenu contextMenu, IPluginLog logger)
    {
        ContextMenu = contextMenu;
        Logger = logger;
        ContextMenu.OnMenuOpened += AddMenu;
    }

    private static void AddMenu(IMenuOpenedArgs args)
    {
        if (args.MenuType != ContextMenuType.Inventory) return;

        var inventoryTarget = (MenuTargetInventory)args.Target;
        if (inventoryTarget.TargetItem is null) return;

        var item = inventoryTarget.TargetItem.Value;

        var hasMateria = item.MateriaEntries.Any(m => m.Type.RowId != 0 && !m.Type.Value.Item[0].Value.Name.IsEmpty);
        if (!hasMateria) return;

        args.AddMenuItem(new MenuItem
        {
            Name = "Retrieve All Materia",
            PrefixChar = 'M',
            PrefixColor = 510,
            OnClicked = _ =>
            {
                Logger?.Info("Starting Bulk Retrieval...");

                var targetContainer = (InventoryType)inventoryTarget.TargetItem.Value.ContainerType;
                var targetSlot = (int)inventoryTarget.TargetItem.Value.InventorySlot;

                var agent = AgentContext.Instance();
                var ownerAddonId = agent != null ? agent->OwnerAddon : 0;
                
                MassMateriaRetrievalService.Start(targetContainer, targetSlot, ownerAddonId, inventoryTarget);
            }
        });
    }
    
    public static void Dispose()
    {
        if (ContextMenu != null) ContextMenu.OnMenuOpened -= AddMenu;
    }
}
