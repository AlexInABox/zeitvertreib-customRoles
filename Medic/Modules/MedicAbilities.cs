using System.Collections.Generic;
using AdminToys;
using CommandSystem.Commands.RemoteAdmin.Dummies;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.API.Features.Toys;
using UncomplicatedCustomRoles.API.Features.CustomModules;
using MEC;
using NetworkManagerUtils.Dummies;
using PlayerRoles;
using RemoteAdmin.Communication;
using UnityEngine;
using UnityEngine.Experimental.Playables;
using UnityEngine.Rendering;
using Light = Exiled.API.Features.Toys.Light;
using PrimitiveObjectToy = LabApi.Features.Wrappers.PrimitiveObjectToy;

namespace Medic.Modules;

public class MedicAbilities : CustomModule
{
    private bool abilityActive = false;
    public override void Execute()
    {
        CustomRole.Player.ShowHint(Plugin.Instance.Translation.AbilityUsed, 1.5f);
        Timing.RunCoroutine(HealNearbyAlliesOverTime());
        Timing.RunCoroutine(ShowHealingBubble());
    }
    
    private IEnumerator<float> HealNearbyAlliesOverTime()
    {
        abilityActive = true;
        const float HEAL_AMOUNT_MAX = 80f;
        const float HEAL_DURATION = 10f;
        
        for (int i = 0; i < HEAL_AMOUNT_MAX; i++)
        {
            HealNearbyAllies(HEAL_AMOUNT_MAX, HEAL_DURATION);
            yield return Timing.WaitForSeconds(HEAL_DURATION / HEAL_AMOUNT_MAX);
        }
        abilityActive = false;
    }

    private void HealNearbyAllies(float healAmountMax, float healDuration)
    {
        if (!CustomRole.Player.IsAlive) return;
        Collider[] hitColliders = Physics.OverlapSphere(CustomRole.Player.Position, 8f); //TODO: Switch to OverlapSphereNonAlloc for garbage collection
        foreach (Collider collider in hitColliders)
        {
            if (!Player.TryGet(collider.gameObject, out Player nearbyPlayer))
            {
                continue;
            }

            if (nearbyPlayer.Role.Side != CustomRole.Player.Role.Side)
            {
                continue;
            }
                
            nearbyPlayer.Heal(1F);
            nearbyPlayer.ShowHint($"<color=green>Du wirst gerade von {CustomRole.Player.Nickname} geheilt!</color>", (healAmountMax / healDuration) + 1f);
            Log.Debug($"Healed {nearbyPlayer.Nickname} for 1 HP");
        }
            
        CustomRole.Player.Heal(1f);
    }
    
    private IEnumerator<float> ShowHealingBubble()
    {
        Primitive bubble = Primitive.Create(PrimitiveType.Sphere, PrimitiveFlags.Visible, CustomRole.Player.Position, Vector3.zero, new Vector3(8f, 8f, 8f), true, new Color(0f, 1f, 0f, 0.1f));
        bubble.Collidable = false;
        bubble.Base._renderer.shadowCastingMode = ShadowCastingMode.Off;
        bubble.Base._renderer.receiveShadows = false;
        bubble.Base._renderer.staticShadowCaster = false;
        Light bubbleLight = Light.Create(bubble.Position, null, null, true, Color.green);
        bubbleLight.Range = 10f;
        bubbleLight.ShadowType = LightShadows.None;
        bubbleLight.ShadowStrength = 0f;

        while (abilityActive)
        {
            bubble.Position = CustomRole.Player.Position;
            bubbleLight.Position = CustomRole.Player.Position;
            yield return Timing.WaitForOneFrame;
        }

        bubble.Destroy();
    }
 }
