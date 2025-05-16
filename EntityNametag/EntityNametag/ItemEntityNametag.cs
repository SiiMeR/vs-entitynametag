using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace EntityNametag;

public class ItemEntityNametag : Item
{
    public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel,
        EntitySelection entitySel,
        bool firstEvent, ref EnumHandHandling handling)
    {
        if (!firstEvent || entitySel == null || byEntity is not EntityPlayer)
        {
            base.OnHeldInteractStart(slot, byEntity, blockSel, entitySel, firstEvent, ref handling);
            return;
        }


        var hasOffhandQuill =
            byEntity.LeftHandItemSlot?.Itemstack?.Collectible.Code.Path.Contains("inkandquill") ?? false;
        if (!hasOffhandQuill)
        {
            base.OnHeldInteractStart(slot, byEntity, blockSel, entitySel, firstEvent, ref handling);
            return;
        }

        var player = (EntityPlayer)byEntity;
        var entityBehaviorOwnable = entitySel.Entity.GetBehavior<EntityBehaviorOwnable>();
        if (entityBehaviorOwnable != null)
        {
            if (!entityBehaviorOwnable.IsOwner(player))
            {
                base.OnHeldInteractStart(slot, byEntity, blockSel, entitySel, firstEvent, ref handling);
                return;
            }
        }

        if (Enumerable.Contains(EntityNametagModSystem.Config.NotApplicableToEntityClasses, entitySel.Entity.Class))
        {
            base.OnHeldInteractStart(slot, byEntity, blockSel, entitySel, firstEvent, ref handling);
            return;
        }

        if (api.Side == EnumAppSide.Client)
        {
            new GuiDialogEntityNameEditor(api as ICoreClientAPI, entitySel.Entity, player,
                    (newName, shouldTakeOwnership) =>
                    {
                        EntityNametagModSystem.ClientNetworkChannel.SendPacket(
                            new NameEntityPacket
                            {
                                EntityId = entitySel.Entity.EntityId,
                                NewName = newName,
                                ShouldHaveOwnership = shouldTakeOwnership
                            });
                    })
                .TryOpen();
        }

        handling = EnumHandHandling.Handled;
    }

    public override WorldInteraction[] GetHeldInteractionHelp(ItemSlot inSlot)
    {
        return ObjectCacheUtil.GetOrCreate(api, "nametagInteractions", () =>
        {
            var interactions = new List<WorldInteraction>();
            var code = inSlot.Itemstack.Collectible.Code.Path;
            if (code is "nametag")
            {
                interactions.Add(new WorldInteraction
                {
                    ActionLangCode = "entitynametag:name",
                    MouseButton = EnumMouseButton.Right,
                    Itemstacks = new[]
                    {
                        new ItemStack(EntityNametagModSystem.Api?.World.GetItem(new AssetLocation("game:inkandquill")))
                    }
                });
            }

            return interactions.ToArray().Append(base.GetHeldInteractionHelp(inSlot));
        });
    }
}