using System.Collections.Generic;
using System.ComponentModel;

using Exiled.API.Features;
using Exiled.API.Interfaces;

using InventorySystem.Items.Firearms.Attachments;

using UnityEngine;

namespace Medic;

public sealed class Config : IConfig
{
    public bool IsEnabled { get; set; } = true;
    
    public bool Debug { get; set; } = false;

    [Description("The cooldown duration for the Medic ability in seconds.")]
    public float CooldownDuration { get; set; } = 300f;
    public int KeybindId { get; set; } = 205;
}