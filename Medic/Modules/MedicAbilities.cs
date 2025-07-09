using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using UncomplicatedCustomRoles.API.Features.CustomModules;
using MEC;

namespace Medic.Modules;

public class MedicAbilities : CustomModule
{
    public override void Execute()
    {
        Log.Debug("EXECUTING!! SCARYY!!!!");
        CustomRole.Player.EnableEffect(EffectType.RainbowTaste, 3, 15f, false);
        Timing.RunCoroutine(HealOverTime());
    }

    private IEnumerator<float> HealOverTime()
    {
        const float HEAL_AMOUNT_MAX = 80f;
        const float HEAL_DURATION = 10f;

        for (int i = 0; i < HEAL_AMOUNT_MAX; i++)
        {
            if (!CustomRole.Player.IsAlive) break;
            CustomRole.Player.Heal(1f);
            yield return Timing.WaitForSeconds(HEAL_DURATION / HEAL_AMOUNT_MAX);
        }
    }
 }
