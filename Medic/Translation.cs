using Exiled.API.Interfaces;

namespace Medic;

public class Translation : ITranslation
{
    public string KeybindSettingLabel { get; set; } = "Use your Medic ability!";

    public string KeybindSettingHintDescription { get; set; } =
        "Pressing this will activate your Medic ability. I have no idea what it does, but it sounds cool!";
    
    public string AbilityOnCooldown { get; set; } = "<color=yellow>Your Medic ability is on cooldown! Please wait before using it again.</color>";
    
    public string AbilityUsed { get; set; } = "<color=green>You have used your Medic ability!</color>";
}