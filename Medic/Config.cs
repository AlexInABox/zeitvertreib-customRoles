using System.ComponentModel;
using Exiled.API.Interfaces;

namespace Medic;

public sealed class Config : IConfig
{
    [Description("The cooldown duration for the Medic ability in seconds.")]
    public float CooldownDuration { get; set; } = 300f;

    public int KeybindId { get; set; } = 205;
    public bool IsEnabled { get; set; } = true;

    public bool Debug { get; set; } = false;
}