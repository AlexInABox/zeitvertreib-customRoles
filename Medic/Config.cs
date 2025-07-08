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

    [Description("The maximum size a player can spawn with.")]
    public Vector3 MaximumSize { get; set; } = new(1f, 1f, 1f);

    [Description("The minimum size a player can spawn with.")]
    public Vector3 MinimumSize { get; set; } = new(0.7f, 0.7f, 0.7f);
    public int KeybindId { get; set; } = 205;
}