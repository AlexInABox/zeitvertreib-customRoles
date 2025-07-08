using Exiled.API.Interfaces;

namespace Medic;

public class Translation : ITranslation
{
    public string KeybindSettingLabel { get; set; } = "Use your Medic ability!";

    public string KeybindSettingHintDescription { get; set; } =
        "Pressing this will activate your Medic ability. I have no idea what it does, but it sounds cool!";
}