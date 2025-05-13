using Vintagestory.API.Config;

namespace EntityNametag;

public static class Util
{
    public static string LangString(string key)
    {
        return Lang.Get($"entitynametag:{key}");
    }
}