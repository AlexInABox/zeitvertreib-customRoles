using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features.DamageHandlers;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using Footprinting;
using InventorySystem.Items;
using InventorySystem.Items.Firearms.Modules;
using InventorySystem.Items.Firearms.ShotEvents;
using LabApi.Features.Wrappers;
using MEC;
using Mirror;
using PlayerStatsSystem;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.Extensions;
using UnityEngine;
using UserSettings.ServerSpecific;
using ZeitvertreibCustomRoles.Modules;
using Cassie = Exiled.API.Features.Cassie;
using Item = Exiled.API.Features.Items.Item;
using Logger = LabApi.Features.Console.Logger;
using Player = Exiled.API.Features.Player;
using Server = Exiled.Events.Handlers.Server;
using Warhead = Exiled.Events.Handlers.Warhead;

namespace ZeitvertreibCustomRoles;

public static class EventHandlers
{
    private static readonly Dictionary<int, long> EffectCooldowns = new();
    private static readonly List<Player> DeathSquadPlayersToDisintegrateOnDeath = [];
    private static CoroutineHandle _detonatedCoroutineHandle;

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
        Exiled.Events.Handlers.Player.Dying += OnDying;
        Exiled.Events.Handlers.Player.Died += OnDied;
        Warhead.Detonated += OnDetonated;


        Server.WaitingForPlayers += OnWaitingForPlayers;
        Server.RoundEnded += OnRoundEnded;

        Timing.KillCoroutines(_detonatedCoroutineHandle);
    }

    public static void UnRegisterEvents()
    {
        ServerSpecificSettingsSync.ServerOnSettingValueReceived -= OnSSSReceived;
        Exiled.Events.Handlers.Player.Spawned -= OnSpawned;
        Exiled.Events.Handlers.Player.Spawning -= OnSpawning;
        Exiled.Events.Handlers.Player.Dying -= OnDying;
        Exiled.Events.Handlers.Player.Died -= OnDied;
        Warhead.Detonated -= OnDetonated;
        Server.WaitingForPlayers -= OnWaitingForPlayers;
        Server.RoundEnded -= OnRoundEnded;
    }

    private static void OnWaitingForPlayers()
    {
        EffectCooldowns.Clear();
        DeathSquadPlayersToDisintegrateOnDeath.Clear();
        Timing.KillCoroutines(_detonatedCoroutineHandle);
    }

    private static void OnRoundEnded(RoundEndedEventArgs ev)
    {
        EffectCooldowns.Clear();
        DeathSquadPlayersToDisintegrateOnDeath.Clear();
        Timing.KillCoroutines(_detonatedCoroutineHandle);
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

            if (role.TryGetModule(out Medic medicModule))
                medicModule.Spawned();

            if (role.TryGetModule(out PinkCandy pinkCandyModule))
                pinkCandyModule.Execute();

            if (role.TryGetModule(out Deathsquad deathSquadModule))
                deathSquadModule.Execute();
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

    private static void OnDying(DyingEventArgs ev)
    {
        if (!ev.Player.TryGetSummonedInstance(out SummonedCustomRole role))
            return;
        if (role.TryGetModule(out Deathsquad _))
        {
            ev.Player.ClearInventory();
            DeathSquadPlayersToDisintegrateOnDeath.Add(ev.Player);
        }
    }

    private static void OnDied(DiedEventArgs ev)
    {
        if (DeathSquadPlayersToDisintegrateOnDeath.Contains(ev.Player))
        {
            DeathSquadPlayersToDisintegrateOnDeath.Remove(ev.Player);
            ev.Ragdoll.DamageHandler = new CustomDamageHandler(ev.Player,
                new DisruptorDamageHandler(
                    new DisruptorShotEvent(ItemIdentifier.None, new Footprint(ev.Attacker.ReferenceHub),
                        DisruptorActionModule.FiringState.FiringSingle), Vector3.up, 999f));

            ev.Ragdoll.IsConsumed = true;
        }
    }

    private static void OnDetonated()
    {
        Timing.KillCoroutines(_detonatedCoroutineHandle);
        _detonatedCoroutineHandle = Timing.CallDelayed(100f, () =>
        {
            int mtfWaveTokens = RespawnWaves.PrimaryMtfWave!.RespawnTokens;
            int chaosWaveTokens = RespawnWaves.PrimaryChaosWave!.RespawnTokens;
            if (!Player.List.Any(player => player.IsDead)) return;

            if (mtfWaveTokens > 0 && chaosWaveTokens > 0)
            {
                Logger.Info("Would've spawned Deathsquad but this is the Ticket status:");
                Logger.Info("MTF Tokens: " + mtfWaveTokens + " | Chaos Tokens: " + chaosWaveTokens);
                return;
            }

            Cassie.Message(
                "pitch_0,8 THE jam_50_9 CASSIESYSTEM HAS BEEN jam_50_3 DEACTIVATED BY THE O5 jam_60_4 KILL SQUAD",
                isSubtitles: false, isNoisy: true);

            foreach (Player player in Player.List)
            {
                if (player.IsAlive || player.IsHost || !player.IsConnected) continue;
                player.SetCustomRole(4050);
            }
        });
    }

    private static void UseMedicAbility(Player player)
    {
        if (!player.TryGetSummonedInstance(out SummonedCustomRole role))
            return;

        if (!role.TryGetModule(out Medic module))
            return;

        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        if (EffectCooldowns.TryGetValue(player.Id, out long cooldownMs) && cooldownMs > nowMs)
        {
            double remaining = (cooldownMs - nowMs) / 1000.0;
            player.ShowHint($"<color=yellow>Medic Fähigkeit ist gerade im Cooldown! ({remaining:F1}s)</color>", 1.5f);
            return;
        }

        module.Execute();
        player.ShowHint(Plugin.Instance.Translation.AbilityUsed, 1.5f);
        EffectCooldowns[player.Id] = nowMs + 70000; // 70s in ms
    }
}