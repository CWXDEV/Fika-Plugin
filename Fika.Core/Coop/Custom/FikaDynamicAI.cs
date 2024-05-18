﻿// © 2024 Lacyway All Rights Reserved

using EFT;
using Fika.Core.Coop.Components;
using Fika.Core.Coop.Players;
using System.Collections.Generic;
using UnityEngine;

namespace Fika.Core.Coop.Custom
{
    internal class FikaDynamicAI : MonoBehaviour
    {
        private int fpsCounter = 0;
        private int resetCount = 150;
        private CoopBot bot;
        private BotOwner botOwner;
        private List<CoopPlayer> humanPlayers;

        protected void Start()
        {
            humanPlayers = [];

            if (CoopHandler.TryGetCoopHandler(out CoopHandler coopHandler))
            {
                // Add all human players to this Limiter
                foreach (CoopPlayer player in coopHandler.Players.Values)
                {
                    if (player.IsYourPlayer || player is ObservedCoopPlayer)
                    {
                        humanPlayers.Add(player);
                    }
                }

                bot = GetComponent<CoopBot>();
                botOwner = GetComponent<BotOwner>();

                if (bot == null || botOwner == null || humanPlayers == null)
                {
                    FikaPlugin.Instance.FikaLogger.LogError("DynamicAI::Start: bot, botOwner or humanPlayers was null!");
                    Destroy(this);
                }

                resetCount = FikaPlugin.DynamicAIRate.Value switch
                {
                    FikaPlugin.DynamicAIRates.Low => 600,
                    FikaPlugin.DynamicAIRates.Medium => 300,
                    FikaPlugin.DynamicAIRates.High => 120,
                    _ => 300,
                };
            }
            else
            {
                FikaPlugin.Instance.FikaLogger.LogError("DynamicAI::Start: CoopHandler was null!");
                Destroy(this);
            }
        }

        protected void FixedUpdate()
        {
            fpsCounter++;

            if (fpsCounter % resetCount == 0)
            {
                fpsCounter = 0;
                if (!bot.HealthController.IsAlive)
                {
                    Destroy(this);
                    return;
                }

                CheckForPlayers();
            }
        }

        private void DeactivateBot()
        {
            bot.PacketSender.Enabled = false;
            botOwner.BotState = EBotState.NonActive;
            botOwner.ShootData.EndShoot();
            botOwner.ShootData.SetCanShootByState(false);
            botOwner.DecisionQueue.Clear();
            botOwner.Memory.GoalEnemy = null;
        }

        private void ActivateBot()
        {
            bot.PacketSender.Enabled = true;
            botOwner.BotState = EBotState.Active;
            botOwner.ShootData.SetCanShootByState(true);
        }

        private void CheckForPlayers()
        {
            int notInRange = 0;

            foreach (CoopPlayer player in humanPlayers)
            {
                if (player == null)
                {
                    notInRange++;
                    continue;
                }
                if (!player.HealthController.IsAlive)
                {
                    notInRange++;
                    continue;
                }
                float distance = Vector3.SqrMagnitude(bot.Position - player.Position);
                float range = FikaPlugin.DynamicAIRange.Value;
                if (distance > range * range)
                {
                    notInRange++;
                }
            }

            if (notInRange >= humanPlayers.Count && botOwner.BotState is EBotState.Active or EBotState.ActiveFail)
            {
                DeactivateBot();
            }
            else if (notInRange < humanPlayers.Count && botOwner.BotState == EBotState.NonActive)
            {
                ActivateBot();
            }
        }
    }
}
