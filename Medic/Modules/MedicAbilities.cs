using UncomplicatedCustomRoles.API.Features.CustomModules;

namespace Medic.Modules;

public class MedicAbilities : CustomModule
{
    public override void Execute()
    {
        CustomRole.Player.Health = CustomRole.Player.Health + 50;
    }
 }
