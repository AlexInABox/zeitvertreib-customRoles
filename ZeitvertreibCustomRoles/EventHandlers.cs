using System.Collections.Generic;
using Exiled.Events.EventArgs.Player;
using MEC;
using Mirror;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.Extensions;
using UnityEngine;
using UserSettings.ServerSpecific;
using ZeitvertreibCustomRoles.Modules;
using Item = Exiled.API.Features.Items.Item;
using Player = Exiled.API.Features.Player;

namespace ZeitvertreibCustomRoles;

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


        Exiled.Events.Handlers.Player.Spawned += OnSpawned;
        Exiled.Events.Handlers.Player.Spawning += OnSpawning;
    }

    public static void UnRegisterEvents()
    {
        ServerSpecificSettingsSync.ServerOnSettingValueReceived -= OnSSSReceived;
        Exiled.Events.Handlers.Player.Spawned -= OnSpawned;
        Exiled.Events.Handlers.Player.Spawning -= OnSpawning;
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

    private static void OnSpawned(SpawnedEventArgs ev)
    {
        // We have to wait for UCR to finish initializing the players custom role
        Timing.CallDelayed(1f, () =>
        {
            if (!ev.Player.TryGetSummonedInstance(out SummonedCustomRole role))
                return;

            if (role.TryGetModule(out PinkCandy pinkCandyModule))
                pinkCandyModule.Execute();
        });
    }

    private static void OnSpawning(SpawningEventArgs ev)
    {
        List<Item> previousItems = [];
        ev.Player.Items.CopyTo(previousItems);
        // We have to wait for UCR to finish initializing the players custom role
        Timing.CallDelayed(2f, () =>
        {
            if (!ev.Player.TryGetSummonedInstance(out SummonedCustomRole role))
                return;

            if (role.TryGetModule(out KeepPreviousInventory keepPreviousInventoryModule))
                keepPreviousInventoryModule.Execute(previousItems);
        });
    }

    private static void UseMedicAbility(Player player)
    {
        if (!player.TryGetSummonedInstance(out SummonedCustomRole role))
            return;

        if (!role.TryGetModule(out Medic module))
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