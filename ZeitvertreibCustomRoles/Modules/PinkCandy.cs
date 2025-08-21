using InventorySystem.Items.Usables.Scp330;
using UncomplicatedCustomRoles.API.Features.CustomModules;

namespace ZeitvertreibCustomRoles.Modules;

// ReSharper disable once ClassNeverInstantiated.Global
public class PinkCandy : CustomModule
{
    public override void Execute()
    {
        CustomRole.Player.TryAddCandy(CandyKindID.Pink);
    }
}