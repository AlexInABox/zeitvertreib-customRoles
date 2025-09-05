using System.Collections.Generic;
using MEC;
using UncomplicatedCustomRoles.API.Features.CustomModules;
using UnityEngine;
using Light = Exiled.API.Features.Toys.Light;
using Logger = LabApi.Features.Console.Logger;


namespace ZeitvertreibCustomRoles.Modules;

// ReSharper disable once ClassNeverInstantiated.Global
public class Deathsquad : CustomModule
{
    public override void Execute()
    {
        Timing.RunCoroutine(ShowAura());
    }

    private IEnumerator<float> ShowAura()
    {
        Logger.Info("Showing Deathsquad Aura");
        Light auraLight = Light.Create(CustomRole.Player.Position, null, null, true, Color.blue);
        auraLight.Range = 2f;
        auraLight.Intensity = 0.5f;
        auraLight.ShadowType = LightShadows.None;
        auraLight.ShadowStrength = 0f;
        auraLight.GameObject.transform.SetParent(CustomRole.Player.GameObject.transform);

        while (CustomRole.Player.IsAlive) yield return Timing.WaitForOneFrame;
        auraLight.Destroy();
    }
}