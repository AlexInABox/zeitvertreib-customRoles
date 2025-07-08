using Exiled.API.Features;
using Medic.Modules;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.Extensions;
using UserSettings.ServerSpecific;
using UnityEngine;

namespace Medic;

public static class EventHandlers
{
    public static void RegisterEvents()
    {
        ServerSpecificSettingsSync.ServerOnSettingValueReceived += OnSSSReceived;
        
        ServerSpecificSettingsSync.DefinedSettings =
        [
            new SSGroupHeader("Medic Fähigkeiten"),
            new SSKeybindSetting(Plugin.Instance.Config.KeybindId, Plugin.Instance.Translation.KeybindSettingLabel, KeyCode.None, false, false,
                Plugin.Instance.Translation.KeybindSettingHintDescription)];
        ServerSpecificSettingsSync.SendToAll();
    }
    public static void UnRegisterEvents()
    {
        ServerSpecificSettingsSync.ServerOnSettingValueReceived -= OnSSSReceived;
    }

    private static void OnSSSReceived(ReferenceHub hub, ServerSpecificSettingBase ev)
    {
        if (!Player.TryGet(hub.networkIdentity, out Player player))
            return;
        
        // Check if the setting is the keybind setting and if it is pressed
        if (ev is SSKeybindSetting keybindSetting &&
            keybindSetting.SettingId == Plugin.Instance.Config.KeybindId &&
            keybindSetting.SyncIsPressed)
            UseMedicAbility(player);
    }
    
    private static void UseMedicAbility(Player player)
    {
        if (!player.TryGetSummonedInstance(out SummonedCustomRole role)) 
            return;

        if (role.TryGetModule(out MedicAbilities module))
            module.Execute();
    }
}