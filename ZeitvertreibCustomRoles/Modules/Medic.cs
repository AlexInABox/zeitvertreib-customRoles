using System.Collections.Generic;
using Exiled.API.Features;
using MEC;
using PlayerRoles;
using UncomplicatedCustomRoles.API.Features.CustomModules;
using UnityEngine;
using Light = Exiled.API.Features.Toys.Light;
using static Exiled.API.Extensions.MirrorExtensions;

namespace ZeitvertreibCustomRoles.Modules;

// ReSharper disable once ClassNeverInstantiated.Global
public class Medic : CustomModule
{
    private bool _abilityActive;

    public override void Execute()
    {
        CustomRole.Player.ShowHint(Plugin.Instance.Translation.AbilityUsed, 1.5f);
        Timing.RunCoroutine(HealNearbyAlliesOverTime());
        Timing.RunCoroutine(ShowHealingBubble());
    }

    public void Spawned()
    {
        Timing.RunCoroutine(ShowHealthStats());
    }

    private IEnumerator<float> HealNearbyAlliesOverTime()
    {
        _abilityActive = true;
        const float HEAL_AMOUNT_MAX = 80f;
        const float HEAL_DURATION = 10f;

        for (int i = 0; i < HEAL_AMOUNT_MAX; i++)
        {
            HealNearbyAllies(HEAL_AMOUNT_MAX, HEAL_DURATION);
            yield return Timing.WaitForSeconds(HEAL_DURATION / HEAL_AMOUNT_MAX);
        }

        _abilityActive = false;
    }

    private void HealNearbyAllies(float healAmountMax, float healDuration)
    {
        if (!CustomRole.Player.IsAlive) return;
        Collider[]
            hitColliders =
                Physics.OverlapSphere(CustomRole.Player.Position,
                    8f); //TODO: Switch to OverlapSphereNonAlloc for garbage collection
        foreach (Collider collider in hitColliders)
        {
            if (!Player.TryGet(collider.gameObject, out Player nearbyPlayer)) continue;

            if (nearbyPlayer.Role.Side != CustomRole.Player.Role.Side) continue;

            nearbyPlayer.Heal(1F);

            if (nearbyPlayer == CustomRole.Player) continue;
            nearbyPlayer.ShowHint($"<color=green>Du wirst gerade von {CustomRole.Player.Nickname} geheilt!</color>",
                healAmountMax / healDuration + 1f);
        }

        CustomRole.Player.Heal(1f);
    }

    private IEnumerator<float> ShowHealingBubble()
    {
        Light bubbleLight = Light.Create(CustomRole.Player.Position, null, null, true, Color.green);
        bubbleLight.Range = 10f;
        bubbleLight.ShadowType = LightShadows.None;
        bubbleLight.ShadowStrength = 0f;
        bubbleLight.GameObject.transform.SetParent(CustomRole.Player.GameObject.transform);

        while (_abilityActive)
            //bubbleLight.Position = CustomRole.Player.Position;
            yield return Timing.WaitForOneFrame;

        bubbleLight.Destroy();
    }

    private IEnumerator<float> ShowHealthStats()
    {
        while (CustomRole.Player.IsAlive)
        {
            foreach (Player player in Player.List)
            {
                if (!player.IsAlive || player.IsNPC || player.IsHost || player == CustomRole.Player) continue;

                if (player.Role == RoleTypeId.Scp3114)
                {
                    CustomRole.Player.SetPlayerInfoForTargetOnly(player, "100/100 HP");
                    continue;
                }
                
                CustomRole.Player.SetPlayerInfoForTargetOnly(player, $"{player.Health}/{player.MaxHealth} HP");
            }

            yield return Timing.WaitForSeconds(1f);
        }
    }
}