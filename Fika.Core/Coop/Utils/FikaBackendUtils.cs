﻿using EFT;
using EFT.UI.Matchmaker;
using Fika.Core.Networking;
using Fika.Core.Networking.Http;
using Fika.Core.Networking.Http.Models;
using System;
using System.Reflection;
using Fika.Core.EssentialPatches;

namespace Fika.Core.Coop.Utils
{
    public enum EMatchmakerType
    {
        Single = 0,
        GroupPlayer = 1,
        GroupLeader = 2
    }

    public static class FikaBackendUtils
    {
        public static MatchMakerAcceptScreen MatchMakerAcceptScreenInstance;
        public static Profile Profile;
        public static string PMCName;
        public static EMatchmakerType MatchingType = EMatchmakerType.Single;
        public static bool IsServer => MatchingType == EMatchmakerType.GroupLeader;
        public static bool IsClient => MatchingType == EMatchmakerType.GroupPlayer;
        public static bool IsSinglePlayer => MatchingType == EMatchmakerType.Single;
        public static PlayersRaidReadyPanel PlayersRaidReadyPanel;
        public static MatchMakerGroupPreview MatchMakerGroupPreview;
        public static int HostExpectedNumberOfPlayers = 1;
        public static WeatherClass[] Nodes = null;
        public static string RemoteIp;
        public static int RemotePort;
        public static int LocalPort = 0;
        public static bool IsHostNatPunch = false;
        private static string groupId;
        private static long timestamp;
        public static bool IsReconnect = false;
        public static bool ReconnectPacketRecieved = false;
        public static ReconnectResponsePacket? ReconnectPacket; // change to not be nullable
        public static bool SpawnedPlayersComplete = false;

        public static MatchmakerTimeHasCome.GClass3187 ScreenController;

        public static string GetGroupId()
        {
            return groupId;
        }

        public static void SetGroupId(string newId)
        {
            groupId = newId;
        }

        public static bool JoinMatch(string profileId, string serverId, out CreateMatch result, out string errorMessage)
        {
            result = new CreateMatch();
            errorMessage = $"No server matches the data provided or the server no longer exists";

            if (MatchMakerAcceptScreenInstance == null)
            {
                return false;
            }

            MatchJoinRequest body = new(serverId, profileId);
            result = FikaRequestHandler.RaidJoin(body);

            if (result.GameVersion != FikaPlugin.EFTVersionMajor)
            {
                errorMessage = $"You are attempting to use a different version of EFT than what the server is running.\nClient: {FikaPlugin.EFTVersionMajor}\nServer: {result.GameVersion}";
                return false;
            }

            Version detectedFikaVersion = Assembly.GetExecutingAssembly().GetName().Version;
            if (result.FikaVersion != detectedFikaVersion)
            {
                errorMessage = $"You are attempting to use a different version of Fika than what the server is running.\nClient: {detectedFikaVersion}\nServer: {result.FikaVersion}";
                return false;
            }

            FikaVersionLabelUpdate_Patch.raidCode = result.RaidCode;

            return true;
        }

        public static void CreateMatch(string profileId, string hostUsername, RaidSettings raidSettings)
        {
            long timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
            string raidCode = GenerateRaidCode(6);
            CreateMatch body = new CreateMatch(raidCode, profileId, hostUsername, timestamp, raidSettings,
                HostExpectedNumberOfPlayers, raidSettings.Side, raidSettings.SelectedDateTime);

            FikaRequestHandler.RaidCreate(body);

            SetGroupId(profileId);
            MatchingType = EMatchmakerType.GroupLeader;

            FikaVersionLabelUpdate_Patch.raidCode = raidCode;
        }

        public static string GenerateRaidCode(int length)
        {
            Random random = new Random();
            char[] chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();
            string raidCode = "";
            for (int i = 0; i < length; i++)
            {
                int charIndex = random.Next(chars.Length);
                raidCode += chars[charIndex];
            }

            return raidCode;
        }
    }
}
