using System.Collections.Generic;
using LabApi.Features.Console;
using UncomplicatedCustomRoles.API.Features.CustomModules;
using Item = Exiled.API.Features.Items.Item;

namespace ZeitvertreibCustomRoles.Modules;

// ReSharper disable once ClassNeverInstantiated.Global
public class KeepPreviousInventory : CustomModule
{
    public void Execute(IReadOnlyCollection<Item> previousItems)
    {
        foreach (Item item in previousItems) Logger.Info("Attempting to add item: " + item.Type);
        Logger.Info(CustomRole.Player.Nickname);
    }
}