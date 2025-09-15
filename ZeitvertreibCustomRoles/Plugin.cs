using System;
using Exiled.API.Enums;
using Exiled.API.Features;

namespace ZeitvertreibCustomRoles;

// ReSharper disable once ClassNeverInstantiated.Global
public class Plugin : Plugin<Config, Translation>
{
    public static Plugin Instance { get; } = new();

    public override string Name { get; } = "zeitvertreib-custom-roles";
    public override string Author { get; } = "AlexInABox";
    public override Version Version { get; } = new(1, 3, 2);
    public override PluginPriority Priority { get; } = PluginPriority.Last;

    /// <inheritdoc />
    public override void OnEnabled()
    {
        EventHandlers.RegisterEvents();
        base.OnEnabled();
    }

    /// <inheritdoc />
    public override void OnDisabled()
    {
        EventHandlers.UnRegisterEvents();
        base.OnDisabled();
    }
}