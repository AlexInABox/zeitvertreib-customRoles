using System.Collections.Generic;
using Exiled.API.Features;
using Medic.Modules;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.Extensions;
using UnityEngine;
using UserSettings.ServerSpecific;

namespace Medic;

public static class EventHandlers
{
    private static readonly Dictionary<int, float> EffectCooldowns = new();

    public static void RegisterEvents()
    {
        ServerSpecificSettingsSync.ServerOnSettingValueReceived += OnSSSReceived;

        ServerSpecificSettingBase[] extra = new ServerSpecificSettingBase[]
        {
            new SSGroupHeader("Medic Fähigkeiten"),
            new SSKeybindSetting(
                Plugin.Instance.Config.KeybindId,
                Plugin.Instance.Translation.KeybindSettingLabel,
                KeyCode.None, false, false,
                Plugin.Instance.Translation.KeybindSettingHintDescription)
        };

        ServerSpecificSettingBase[] existing = ServerSpecificSettingsSync.DefinedSettings ?? [];

        ServerSpecificSettingBase[] combined = new ServerSpecificSettingBase[existing.Length + extra.Length];
        existing.CopyTo(combined, 0);
        extra.CopyTo(combined, existing.Length);

        ServerSpecificSettingsSync.DefinedSettings = combined;
        ServerSpecificSettingsSync.UpdateDefinedSettings();
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

        if (!role.TryGetModule(out MedicAbilities module))
            return;

        if (EffectCooldowns.TryGetValue(player.Id, out float cooldown) && cooldown > Time.time)
        {
            player.ShowHint(Plugin.Instance.Translation.AbilityOnCooldown, 1.5f);
            return;
        }

        module.Execute();
        player.ShowHint(Plugin.Instance.Translation.AbilityUsed, 1.5f);
        EffectCooldowns[player.Id] = Time.time + Plugin.Instance.Config.CooldownDuration;
    }
}