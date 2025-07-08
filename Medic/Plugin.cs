using System;
using Exiled.API.Enums;
using Exiled.API.Features;
using YamlDotNet.Serialization.EventEmitters;

namespace Medic;

// ReSharper disable once ClassNeverInstantiated.Global
public class Plugin : Plugin<Config, Translation>
{
    private static readonly Plugin Singleton = new();
    public static Plugin Instance => Singleton;
    
    public override string Name { get; } = "Medic";
    public override string Author { get; } = "AlexInABox";
    public override Version Version { get; } = new(1, 0, 0);
    public override PluginPriority Priority { get; } = PluginPriority.Last;

    /// <inheritdoc/>
    public override void OnEnabled()
    {
        EventHandlers.RegisterEvents();
        base.OnEnabled();
    }

    /// <inheritdoc/>
    public override void OnDisabled()
    {
        EventHandlers.UnRegisterEvents();
        base.OnDisabled();
    }
    
}