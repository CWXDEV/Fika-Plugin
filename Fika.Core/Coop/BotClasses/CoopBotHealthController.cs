﻿// © 2024 Lacyway All Rights Reserved

using EFT;
using EFT.HealthSystem;
using Fika.Core.Coop.Players;

namespace Fika.Core.Coop.ClientClasses
{
    public sealed class CoopBotHealthController(Profile.GClass1767 healthInfo, Player player, InventoryControllerClass inventoryController, SkillManager skillManager, bool aiHealth)
        : PlayerHealthController(healthInfo, player, inventoryController, skillManager, aiHealth)
    {
        private readonly CoopBot coopBot = (CoopBot)player;
        public override bool _sendNetworkSyncPackets
        {
            get
            {
                return true;
            }
        }

        public override void SendNetworkSyncPacket(GStruct347 packet)
        {
            if (packet.SyncType == GStruct347.ESyncType.IsAlive && !packet.Data.IsAlive.IsAlive)
            {
                coopBot.PacketSender.HealthSyncPackets.Enqueue(coopBot.SetupDeathPacket(packet));
                return;
            }

            coopBot.PacketSender.HealthSyncPackets.Enqueue(new(coopBot.NetId)
            {
                Packet = packet
            });
        }
    }
}
