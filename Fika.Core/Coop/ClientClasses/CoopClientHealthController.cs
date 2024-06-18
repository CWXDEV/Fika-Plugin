﻿// © 2024 Lacyway All Rights Reserved

using EFT;
using EFT.HealthSystem;
using Fika.Core.Coop.Players;

namespace Fika.Core.Coop.ClientClasses
{
    public sealed class CoopClientHealthController(Profile.GClass1768 healthInfo, Player player, InventoryControllerClass inventoryController, SkillManager skillManager, bool aiHealth)
        : PlayerHealthController(healthInfo, player, inventoryController, skillManager, aiHealth)
    {
        private readonly CoopPlayer coopPlayer = (CoopPlayer)player;
        public override bool _sendNetworkSyncPackets
        {
            get
            {
                return true;
            }
        }

        public override void SendNetworkSyncPacket(GStruct346 packet)
        {
            if (packet.SyncType == GStruct346.ESyncType.IsAlive && !packet.Data.IsAlive.IsAlive)
            {
                coopPlayer.PacketSender.HealthSyncPackets.Enqueue(coopPlayer.SetupDeathPacket(packet));
                return;
            }

            coopPlayer.PacketSender.HealthSyncPackets.Enqueue(new(coopPlayer.NetId)
            {
                Packet = packet
            });
        }

        /// <summary>
        /// Used for reconnecting only
        /// </summary>
        public void SyncBodyPartsState()
        {
            foreach (EBodyPart bodyPart in GClass2477.RealBodyParts)
            {
                if (GetBodyPartHealth(bodyPart, true).AtMinimum)
                {
                    DestroyBodyPart(bodyPart, EDamageType.Undefined);
                }
            }
        }
    }
}
