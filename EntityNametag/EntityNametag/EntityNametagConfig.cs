using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace EntityNametag;

public class EntityNametagConfig
{
    public string[] NotApplicableToEntityClasses =
    {
        nameof(EntityPlayer), nameof(EntityTrader), nameof(EntityVillager), nameof(EntityHumanoid),
        nameof(EntityEidolon)
    };
}