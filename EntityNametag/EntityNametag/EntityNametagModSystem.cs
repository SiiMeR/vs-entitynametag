global using static EntityNametag.Util;
using System;
using System.Linq;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Server;

namespace EntityNametag;

public class EntityNametagModSystem : ModSystem
{
    public static EntityNametagConfig Config;

    public static IClientNetworkChannel ClientNetworkChannel;
    public static IServerNetworkChannel ServerNetworkChannel;
    public static ICoreAPI Api;

    public override void Start(ICoreAPI api)
    {
        Api = api;


        api.RegisterItemClass("ItemEntityNametag", typeof(ItemEntityNametag));

        var harmony = new Harmony(Mod.Info.ModID);

        var original =
            AccessTools.Method(typeof(Entity), "GetName");
        var patch = AccessTools.Method(typeof(EntityNamePatch), nameof(EntityNamePatch.Postfix));

        harmony.Patch(original, postfix: new HarmonyMethod(patch));
    }

    public override void StartClientSide(ICoreClientAPI api)
    {
        Config = new EntityNametagConfig();

        ClientNetworkChannel = api.Network.RegisterChannel(Mod.Info.ModID)
            .RegisterMessageType<NameEntityPacket>()
            .RegisterMessageType<ConfigPacket>()
            .SetMessageHandler<ConfigPacket>(OnConfigPacketReceived);
    }

    public override void StartServerSide(ICoreServerAPI api)
    {
        TryToLoadConfig(api);

        ServerNetworkChannel = api.Network.RegisterChannel(Mod.Info.ModID)
            .RegisterMessageType<NameEntityPacket>()
            .RegisterMessageType<ConfigPacket>()
            .SetMessageHandler<NameEntityPacket>(OnNameEntityPacket);

        api.Event.PlayerNowPlaying += player =>
        {
            var configPacket = new ConfigPacket { NotApplicableToEntityClasses = Config.NotApplicableToEntityClasses };
            ServerNetworkChannel.SendPacket(
                configPacket, player);
        };
    }

    private void OnNameEntityPacket(IServerPlayer fromPlayer, NameEntityPacket packet)
    {
        var entity = Api.World.GetEntityById(packet.EntityId);
        if (entity == null)
        {
            return;
        }

        if (Config.NotApplicableToEntityClasses.Contains(entity.Class))
        {
            Api.Logger.Error(
                $"Nametag cannot be applied to {entity.Class}. This could mean someone is trying to cheat (there is client-side validation in place). Requesting player: {fromPlayer.PlayerUID}");
            return;
        }

        var originalName = entity.GetName();

        entity.WatchedAttributes.SetString("customName", packet.NewName);
        entity.WatchedAttributes.MarkPathDirty("customName");

        var slot = fromPlayer.InventoryManager.ActiveHotbarSlot;
        if (slot.Itemstack.Collectible is ItemEntityNametag)
        {
            slot.TakeOut(1);
            slot.MarkDirty();
        }
        else
        {
            fromPlayer.Entity.WalkInventory(s =>
            {
                if (s.Itemstack.Collectible is ItemEntityNametag)
                {
                    s.TakeOut(1);
                    s.MarkDirty();
                    return false;
                }

                return true;
            });
        }

        Api.Logger.Audit(
            $"Player {fromPlayer.PlayerName} ({fromPlayer.PlayerUID}) renamed entity {entity.Code} (at {entity.ServerPos.AsBlockPos}) from '{originalName}' to '{packet.NewName}'");
    }

    private void OnConfigPacketReceived(ConfigPacket packet)
    {
        Config.NotApplicableToEntityClasses = packet.NotApplicableToEntityClasses;
    }

    private static void TryToLoadConfig(ICoreAPI api)
    {
        //It is important to surround the LoadModConfig function in a try-catch. 
        //If loading the file goes wrong, then the 'catch' block is run.
        try
        {
            Config = api.LoadModConfig<EntityNametagConfig>("EntityNametagConfig.json") ?? new EntityNametagConfig();

            //Save a copy of the mod config.
            api.StoreModConfig(Config, "EntityNametagConfig.json");
        }
        catch (Exception e)
        {
            //Couldn't load the mod config... Create a new one with default settings, but don't save it.
            api.Logger.Error("Could not load config! Loading default settings instead.");
            api.Logger.Error(e);
            Config = new EntityNametagConfig();
        }
    }
}

public static class EntityNamePatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Entity), "GetName")]
    public static void Postfix(
        Entity __instance, ref string __result)
    {
        var customName = __instance?.WatchedAttributes.GetString("customName");
        if (!string.IsNullOrEmpty(customName))
        {
            __result = customName;
        }
    }
}