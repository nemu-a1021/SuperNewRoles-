﻿using HarmonyLib;
using Hazel;
using System.Collections.Generic;
using System.Linq;
using UnhollowerBaseLib;
using System.Collections;
using System;
using System.Text;
using UnityEngine;
using System.Reflection;
using SuperNewRoles.Roles;
using SuperNewRoles.Mode;
using SuperNewRoles.Patch;
using SuperNewRoles.CustomOption;

namespace SuperNewRoles.EndGame
{
    enum WinCondition
    {
        Default,
        HAISON,
        JesterWin,
        JackalWin,
        QuarreledWin,
        GodWin,
        BugEnd
    }
    static class AdditionalTempData
    {
        // Should be implemented using a proper GameOverReason in the future
        public static List<PlayerRoleInfo> playerRoles = new List<PlayerRoleInfo>();
        public static GameOverReason gameOverReason;
        public static WinCondition winCondition = WinCondition.Default;
        public static List<WinCondition> additionalWinConditions = new List<WinCondition>();
        public static List<PlayerControl> Allplayers = new List<PlayerControl>();

        public static Dictionary<int, PlayerControl> plagueDoctorInfected = new Dictionary<int, PlayerControl>();
        public static Dictionary<int, float> plagueDoctorProgress = new Dictionary<int, float>();

        public static void clear()
        {
            playerRoles.Clear();
            additionalWinConditions.Clear();
            winCondition = WinCondition.Default;
            Allplayers = new List<PlayerControl>();
        }

        internal class PlayerRoleInfo
        {
            public string PlayerName { get; set; }
            public string NameSuffix { get; set; }
            public List<Intro.IntroDate> Roles { get; set; }
            public string RoleString { get; set; }
            public int TasksCompleted { get; set; }
            public int TasksTotal { get; set; }
            public int PlayerId { get; set; }
            public FinalStatus Status { get; internal set; }
            public Intro.IntroDate IntroDate { get; set; }
        }
    }
    [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.SetEverythingUp))]
    public class EndGameManagerSetUpPatch
    {
        public static TMPro.TMP_Text textRenderer;
        [HarmonyPatch(typeof(EndGameNavigation), nameof(EndGameNavigation.ShowProgression))]
        public class ShowProgressionPatch
        {
            public static void Prefix()
            {
                if (textRenderer != null)
                {
                    textRenderer.gameObject.SetActive(false);
                }
            }
        }
        public static void Postfix(EndGameManager __instance)
        {
            foreach (PoolablePlayer pb in __instance.transform.GetComponentsInChildren<PoolablePlayer>())
            {
                UnityEngine.Object.Destroy(pb.gameObject);
            }
            int num = Mathf.CeilToInt(7.5f);
            List<WinningPlayerData> list = TempData.winners.ToArray().ToList().OrderBy(delegate (WinningPlayerData b)
            {
                if (!b.IsYou)
                {
                    return 0;
                }
                return -1;
            }).ToList<WinningPlayerData>();

            for (int i = 0; i < list.Count; i++)
            {
                WinningPlayerData winningPlayerData2 = list[i];
                int num2 = (i % 2 == 0) ? -1 : 1;
                int num3 = (i + 1) / 2;
                float num4 = (float)num3 / (float)num;
                float num5 = Mathf.Lerp(1f, 0.75f, num4);
                float num6 = (float)((i == 0) ? -8 : -1);
                PoolablePlayer poolablePlayer = UnityEngine.Object.Instantiate<PoolablePlayer>(__instance.PlayerPrefab, __instance.transform);

                poolablePlayer.transform.localPosition = new Vector3(1f * (float)num2 * (float)num3 * num5, FloatRange.SpreadToEdges(-1.125f, 0f, num3, num), num6 + (float)num3 * 0.01f) * 0.9f;

                float num7 = Mathf.Lerp(1f, 0.65f, num4) * 0.9f;
                Vector3 vector = new Vector3(num7, num7, 1f);
                poolablePlayer.transform.localScale = vector;
                poolablePlayer.UpdateFromPlayerOutfit(winningPlayerData2, winningPlayerData2.IsDead);

                if (winningPlayerData2.IsDead)
                {
                    poolablePlayer.Body.sprite = __instance.GhostSprite;
                    poolablePlayer.SetDeadFlipX(i % 2 == 0);
                }
                else
                {
                    poolablePlayer.SetFlipX(i % 2 == 0);
                }

                poolablePlayer.NameText.color = Color.white;
                poolablePlayer.NameText.lineSpacing *= 0.7f;
                poolablePlayer.NameText.transform.localScale = new Vector3(1f / vector.x, 1f / vector.y, 1f / vector.z);
                poolablePlayer.NameText.transform.localPosition = new Vector3(poolablePlayer.NameText.transform.localPosition.x, poolablePlayer.NameText.transform.localPosition.y, -15f);

                poolablePlayer.NameText.text = winningPlayerData2.PlayerName;

                foreach (var data in AdditionalTempData.playerRoles)
                {
                    if (data.PlayerName != winningPlayerData2.PlayerName) continue;
                    poolablePlayer.NameText.text += data.NameSuffix + $"\n<size=80%>{string.Join("\n", CustomOptions.cs(data.IntroDate.color, data.IntroDate.NameKey + "Name"))}</size>";
                }
            }
            GameObject bonusTextObject = UnityEngine.Object.Instantiate(__instance.WinText.gameObject);
            bonusTextObject.transform.position = new Vector3(__instance.WinText.transform.position.x, __instance.WinText.transform.position.y - 0.8f, __instance.WinText.transform.position.z);
            bonusTextObject.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
            textRenderer = bonusTextObject.GetComponent<TMPro.TMP_Text>();
            textRenderer.text = "";
            var text = "";
            if (AdditionalTempData.winCondition == WinCondition.GodWin)
            {
                text = "GodName";
                textRenderer.color = RoleClass.God.color;
                __instance.BackgroundBar.material.SetColor("_Color", Roles.RoleClass.God.color);
            }
            else if (AdditionalTempData.winCondition == WinCondition.HAISON)
            {
                //bonusText = "jesterWin";
                text = "HAISON";
                textRenderer.color = Color.white;
                __instance.BackgroundBar.material.SetColor("_Color", Color.white);
            }
            else if (AdditionalTempData.winCondition == WinCondition.BugEnd)
            {
                //bonusText = "jesterWin";
                text = "BUG";
                textRenderer.color = Color.white;
                __instance.BackgroundBar.material.SetColor("_Color", Color.white);
            }
            else if (AdditionalTempData.winCondition == WinCondition.JesterWin)
            {
                //bonusText = "jesterWin";
                text = "JesterName";
                textRenderer.color = Roles.RoleClass.Jester.color;
                __instance.BackgroundBar.material.SetColor("_Color", Roles.RoleClass.Jester.color);
            }
            else if (AdditionalTempData.winCondition == WinCondition.QuarreledWin)
            {
                text = "QuarreledName";
                textRenderer.color = Roles.RoleClass.Quarreled.color;
                __instance.BackgroundBar.material.SetColor("_Color", Roles.RoleClass.Quarreled.color);
            }
            else if (AdditionalTempData.gameOverReason == GameOverReason.HumansByTask || AdditionalTempData.gameOverReason == GameOverReason.HumansByVote)
            {
                text = "CrewMateName";
                textRenderer.color = Palette.White;
            }
            else if (AdditionalTempData.gameOverReason == GameOverReason.ImpostorByKill || AdditionalTempData.gameOverReason == GameOverReason.ImpostorBySabotage || AdditionalTempData.gameOverReason == GameOverReason.ImpostorByVote)
            {
                text = "ImpostorName";
                textRenderer.color = RoleClass.ImpostorRed;
            }
            else if (AdditionalTempData.winCondition == WinCondition.JackalWin)
            {
                text = "JackalName";
                textRenderer.color = Roles.RoleClass.Jackal.color;
                __instance.BackgroundBar.material.SetColor("_Color", Roles.RoleClass.Jackal.color);
            }

            if (ModeHandler.isMode(ModeId.BattleRoyal)) {
                foreach (PlayerControl p in PlayerControl.AllPlayerControls) {
                    if (p.isAlive())
                    {
                        text = p.nameText.text;
                        textRenderer.color = new Color32(116, 80, 48, byte.MaxValue);
                    }
                }
            }
            var haison = false;
            if (text == "HAISON") {
                haison = true;
                text = ModTranslation.getString("HaisonName");
            } else if (text == "BUG") {
                haison = true;
                text = "不具合が発生したので強制的に終了しました";
            }else{
                    text = ModTranslation.getString(text);
            }
            bool IsOpptexton = false;
            foreach (PlayerControl player in RoleClass.Opportunist.OpportunistPlayer) {
                if (player.isAlive()) { 
                    if (!IsOpptexton)
                    {
                        text = text + "&"+ModTranslation.getString("OpportunistName");
                    }

                }
            }
            if (!haison) {
                    textRenderer.text = string.Format(text + " " + ModTranslation.getString("WinName"));
            } else {
                    textRenderer.text = text;
            }
            if (true)
            {
                var position = Camera.main.ViewportToWorldPoint(new Vector3(0f, 1f, Camera.main.nearClipPlane));
                GameObject roleSummary = UnityEngine.Object.Instantiate(__instance.WinText.gameObject);
                roleSummary.transform.position = new Vector3(__instance.Navigation.ExitButton.transform.position.x + 0.1f, position.y - 0.1f, -14f);
                roleSummary.transform.localScale = new Vector3(1f, 1f, 1f);

                var roleSummaryText = new StringBuilder();
                roleSummaryText.AppendLine(ModTranslation.getString("最終結果"));

                SuperNewRolesPlugin.Logger.LogInfo("SAISYUUMAE");
                foreach (var data in AdditionalTempData.Allplayers)
                {
                    var (playerCompleted, playerTotal) = TaskCount.TaskDate(data.Data);
                    var taskInfo = playerTotal > 0 ? $"<color=#FAD934FF>{playerCompleted}/{playerTotal}</color>" : "";
                    string aliveDead = "";
                        string Suffix = "";
                    string result = $"{data.Data.PlayerName + Suffix}<pos=18.5%>{taskInfo}<pos=25%>{aliveDead}<pos=34%>{ModTranslation.getString(Intro.IntroDate.GetIntroDate(data.getRole(),data)+"Name")}";
                    SuperNewRolesPlugin.Logger.LogInfo("ADD:"+result);
                    roleSummaryText.AppendLine(result);
                }

                TMPro.TMP_Text roleSummaryTextMesh = roleSummary.GetComponent<TMPro.TMP_Text>();
                roleSummaryTextMesh.alignment = TMPro.TextAlignmentOptions.TopLeft;
                roleSummaryTextMesh.color = Color.white;
                roleSummaryTextMesh.outlineWidth *= 1.2f;
                roleSummaryTextMesh.fontSizeMin = 1.25f;
                roleSummaryTextMesh.fontSizeMax = 1.25f;
                roleSummaryTextMesh.fontSize = 1.25f;

                var roleSummaryTextMeshRectTransform = roleSummaryTextMesh.GetComponent<RectTransform>();
                roleSummaryTextMeshRectTransform.anchoredPosition = new Vector2(position.x + 3.5f, position.y - 0.1f);
                roleSummaryTextMesh.text = roleSummaryText.ToString();
            }
            AdditionalTempData.clear();
        }
    }
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
    public class OnGameEndPatch
    {

        public static PlayerControl WinnerPlayer;
        public static void Prefix(AmongUsClient __instance, [HarmonyArgument(0)] ref EndGameResult endGameResult)
        {
            AdditionalTempData.gameOverReason = endGameResult.GameOverReason;
            if ((int)endGameResult.GameOverReason >= 10) endGameResult.GameOverReason = GameOverReason.ImpostorByKill;
        }

            public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] ref EndGameResult endGameResult)
            {
                var gameOverReason = AdditionalTempData.gameOverReason;
                AdditionalTempData.clear();

            foreach (var p in GameData.Instance.AllPlayers)
            {
                //var p = pc.Data;
                var roles = Intro.IntroDate.GetIntroDate(p.Object.getRole(),p.Object);
                var (tasksCompleted, tasksTotal) = TaskCount.TaskDate(p);
                var finalStatus = FinalStatusPatch.FinalStatusData.FinalStatuses[p.PlayerId] =
                    p.Disconnected == true ? FinalStatus.Disconnected :
                    FinalStatusPatch.FinalStatusData.FinalStatuses.ContainsKey(p.PlayerId) ? FinalStatusPatch.FinalStatusData.FinalStatuses[p.PlayerId] :
                    p.IsDead == true ? FinalStatus.Dead :
                    gameOverReason == GameOverReason.ImpostorBySabotage && !p.Role.IsImpostor ? FinalStatus.Sabotage :
                    FinalStatus.Alive;

                AdditionalTempData.playerRoles.Add(new AdditionalTempData.PlayerRoleInfo()
                {
                    PlayerName = p.PlayerName,
                    PlayerId = p.PlayerId,
                    TasksTotal = tasksTotal,
                    TasksCompleted = gameOverReason == GameOverReason.HumansByTask ? tasksTotal : tasksCompleted,
                    Status = finalStatus,
                    IntroDate = roles
                });
            }
            // Remove Jester, Arsonist, Vulture, Jackal, former Jackals and Sidekick from winners (if they win, they'll be readded)
            List<PlayerControl> notWinners = new List<PlayerControl>();

                notWinners.AddRange(RoleClass.Jester.JesterPlayer);
                notWinners.AddRange(RoleClass.MadMate.MadMatePlayer);
                notWinners.AddRange(RoleClass.Jackal.JackalPlayer);
                notWinners.AddRange(RoleClass.Jackal.SidekickPlayer);
                notWinners.AddRange(RoleClass.JackalFriends.JackalFriendsPlayer);
                notWinners.AddRange(RoleClass.God.GodPlayer);
                notWinners.AddRange(RoleClass.Opportunist.OpportunistPlayer);

                List<WinningPlayerData> winnersToRemove = new List<WinningPlayerData>();
                foreach (WinningPlayerData winner in TempData.winners)
                {
                    if (notWinners.Any(x => x.Data.PlayerName == winner.PlayerName)) winnersToRemove.Add(winner);
                }
                foreach (var winner in winnersToRemove) TempData.winners.Remove(winner);
                // Neutral shifter can't win

                bool saboWin = gameOverReason == GameOverReason.ImpostorBySabotage;


                bool JesterWin = gameOverReason == (GameOverReason)CustomGameOverReason.JesterWin;
                bool QuarreledWin = gameOverReason == (GameOverReason)CustomGameOverReason.QuarreledWin;
                bool JackalWin = gameOverReason == (GameOverReason)CustomGameOverReason.JackalWin;
                bool HAISON = gameOverReason == (GameOverReason)CustomGameOverReason.HAISON;
            bool BUGEND = gameOverReason == (GameOverReason)CustomGameOverReason.BugEnd;

            if (JesterWin)
            {
                TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                WinnerPlayer.Data.IsDead = false;
                WinningPlayerData wpd = new WinningPlayerData(WinnerPlayer.Data);
                TempData.winners.Add(wpd);
                AdditionalTempData.winCondition = WinCondition.JesterWin;
            } else if (JackalWin)
            {
                TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                foreach (PlayerControl p in RoleClass.Jackal.JackalPlayer)
                {
                    WinningPlayerData wpd = new WinningPlayerData(p.Data);
                    TempData.winners.Add(wpd);
                }
                foreach (PlayerControl p in RoleClass.Jackal.SidekickPlayer)
                {
                    WinningPlayerData wpd = new WinningPlayerData(p.Data);
                    TempData.winners.Add(wpd);
                }
                foreach (PlayerControl p in RoleClass.JackalFriends.JackalFriendsPlayer) {
                    WinningPlayerData wpd = new WinningPlayerData(p.Data);
                    TempData.winners.Add(wpd);
                }

                AdditionalTempData.winCondition = WinCondition.JackalWin;
            }
            if (TempData.winners.ToArray().Any(x => x.IsImpostor))
            {
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    if (Roles.RoleClass.MadMate.MadMatePlayer.IsCheckListPlayerControl(p))
                    {
                        WinningPlayerData wpd = new WinningPlayerData(p.Data);
                        TempData.winners.Add(wpd);
                    }
                }
            }

            if (ModeHandler.isMode(ModeId.BattleRoyal)) {
                TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    if (p.isAlive())
                    {
                        WinningPlayerData wpd = new WinningPlayerData(p.Data);
                        TempData.winners.Add(wpd);
                    }
                }
                AdditionalTempData.winCondition = WinCondition.Default;
            }
            var godalive = false;
            foreach (PlayerControl p in RoleClass.God.GodPlayer) {
                if (p.isAlive())
                {
                    godalive = true;
                    TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                    WinningPlayerData wpd = new WinningPlayerData(p.Data);
                    TempData.winners.Add(wpd);
                    AdditionalTempData.winCondition = WinCondition.GodWin;
                }
            }

            foreach (PlayerControl player in RoleClass.Opportunist.OpportunistPlayer)
            {
                if (player.isAlive())
                {
                    TempData.winners.Add(new WinningPlayerData(player.Data));
                }
            }

            foreach (List<PlayerControl> players in RoleClass.Quarreled.QuarreledPlayer)
            {
                notWinners.AddRange(players);
            }
            if (QuarreledWin)
            {
                TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                var winplays = new List<PlayerControl>() { WinnerPlayer };
                winplays.Add(WinnerPlayer.GetOneSideQuarreled());
                foreach (PlayerControl p in winplays)
                {
                    p.Data.IsDead = false;
                    WinningPlayerData wpd = new WinningPlayerData(p.Data);
                    TempData.winners.Add(wpd);
                }
                AdditionalTempData.winCondition = WinCondition.QuarreledWin;
            }
            else if (HAISON)
            {
                TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    WinningPlayerData wpd = new WinningPlayerData(p.Data);
                    TempData.winners.Add(wpd);
                }
                AdditionalTempData.winCondition = WinCondition.HAISON;
            }
            else if (BUGEND)
            {
                TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    if (p.isImpostor() || p.isRole(CustomRPC.RoleId.Jackal) || RoleClass.Jackal.SidekickPlayer.IsCheckListPlayerControl(p) || p.isRole(CustomRPC.RoleId.JackalFriends))
                    {
                        WinningPlayerData wpd = new WinningPlayerData(p.Data);
                        TempData.winners.Add(wpd);
                    }
                }
                AdditionalTempData.winCondition = WinCondition.BugEnd;
            }
        }
        
    }
    [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
    public class CheckEndGamePatch
    {
        public static void Prefix(ExileController __instance)
        {
            try
            {
                WrapUpClass.WrapUpPostfix(__instance.exiled);
            }
            catch (Exception e)
            {
                SuperNewRolesPlugin.Logger.LogInfo("CHECKERROR:" + e);
            }
        }
    }
    [HarmonyPatch(typeof(AirshipExileController), nameof(AirshipExileController.WrapUpAndSpawn))]
    public class CheckAirShipEndGamePatch
    {
        public static void Prefix(AirshipExileController __instance)
        {
            try
            {
                WrapUpClass.WrapUpPostfix(__instance.exiled);
            }
            catch (Exception e)
            {
                SuperNewRolesPlugin.Logger.LogInfo("CHECKERROR:"+e);
            }
        }
    }
    [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString), new Type[] { typeof(StringNames), typeof(Il2CppReferenceArray<Il2CppSystem.Object>) })]
    class ExileControllerMessagePatch
    {
        static void Postfix(ref string __result, [HarmonyArgument(0)] StringNames id)
        {
            try
            {
                if (ExileController.Instance != null && ExileController.Instance.exiled != null)
                {
                    PlayerControl player = ModHelpers.playerById(ExileController.Instance.exiled.Object.PlayerId);
                    if (player == null) return;
                    // Exile role text
                    if (id == StringNames.ExileTextPN || id == StringNames.ExileTextSN || id == StringNames.ExileTextPP || id == StringNames.ExileTextSP)
                    {
                        __result = player.Data.PlayerName + " は " + ModTranslation.getString(Intro.IntroDate.GetIntroDate(player.getRole()).NameKey+"Name")+" だった！";
                    }
                }
            }
            catch
            {
                // pass - Hopefully prevent leaving while exiling to softlock game
            }
        }
    }
    public class WrapUpClass {
        public static void WrapUpPostfix(GameData.PlayerInfo exiled)
        {
            Buttons.CustomButton.MeetingEndedUpdate();
            if (exiled == null) return;
            exiled.Object.Exiled();
            var Player = ModHelpers.playerById(exiled.PlayerId);
            if (RoleHelpers.IsQuarreled(Player))
            {
                var Side = RoleHelpers.GetOneSideQuarreled(Player);
                if (Side.isDead())
                {
                    CustomRPC.RPCProcedure.ShareWinner(Player.PlayerId);

                    MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.ShareWinner, Hazel.SendOption.Reliable, -1);
                    Writer.Write(Player.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(Writer);
                    Roles.RoleClass.Quarreled.IsQuarreledWin = true;
                    ShipStatus.RpcEndGame((GameOverReason)CustomGameOverReason.QuarreledWin, false);
                }
            }
            if (Roles.RoleClass.Jester.JesterPlayer.IsCheckListPlayerControl(Player))
            {
                
                if (!Roles.RoleClass.Jester.IsJesterTaskClearWin || (Roles.RoleClass.Jester.IsJesterTaskClearWin && Patch.TaskCount.TaskDateNoClearCheck(Player.Data).Item2 - Patch.TaskCount.TaskDateNoClearCheck(Player.Data).Item1 == 0))
                {
                    CustomRPC.RPCProcedure.ShareWinner(Player.PlayerId);

                    MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.ShareWinner, Hazel.SendOption.Reliable, -1);
                    Writer.Write(Player.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(Writer);
                    Roles.RoleClass.Jester.IsJesterWin = true;
                    ShipStatus.RpcEndGame((GameOverReason)CustomGameOverReason.JesterWin, false);
                }
            }
        }
    }
    
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CheckEndCriteria))]
    class CheckGameEndPatch
    {
        public static bool Prefix(ShipStatus __instance)
        {
            if (!GameData.Instance) return false;
            if (DestroyableSingleton<TutorialManager>.InstanceExists) return true;
            if (Patch.DebugMode.IsDebugMode()) return false;
            var statistics = new PlayerStatistics(__instance);
            if (!ModeHandler.isMode(ModeId.Default))
                {
                    ModeHandler.EndGameChecks(__instance, statistics);
                }
                else
                {
                if (CheckAndEndGameForSabotageWin(__instance)) return false;
                if (CheckAndEndGameForJackalWin(__instance, statistics)) return false;
                if (CheckAndEndGameForImpostorWin(__instance, statistics)) return false;
                if (CheckAndEndGameForCrewmateWin(__instance, statistics)) return false;
                if (CheckAndEndGameForTaskWin(__instance)) return false;
                //if (CheckAndEndGameForBugEnd(__instance,statistics)) return false;
            }
            return false;
        }
        public static void CustomEndGame(GameOverReason reason,bool showAd) {
            ShipStatus.RpcEndGame(reason, showAd);
        }
        private static bool CheckAndEndGameForSabotageWin(ShipStatus __instance)
        {
            if (__instance.Systems == null) return false;
            ISystemType systemType = __instance.Systems.ContainsKey(SystemTypes.LifeSupp) ? __instance.Systems[SystemTypes.LifeSupp] : null;
            if (systemType != null)
            {
                LifeSuppSystemType lifeSuppSystemType = systemType.TryCast<LifeSuppSystemType>();
                if (lifeSuppSystemType != null && lifeSuppSystemType.Countdown < 0f)
                {
                    EndGameForSabotage(__instance);
                    lifeSuppSystemType.Countdown = 10000f;
                    return true;
                }
            }
            ISystemType systemType2 = __instance.Systems.ContainsKey(SystemTypes.Reactor) ? __instance.Systems[SystemTypes.Reactor] : null;
            if (systemType2 == null)
            {
                systemType2 = __instance.Systems.ContainsKey(SystemTypes.Laboratory) ? __instance.Systems[SystemTypes.Laboratory] : null;
            }
            if (systemType2 != null)
            {
                ICriticalSabotage criticalSystem = systemType2.TryCast<ICriticalSabotage>();
                if (criticalSystem != null && criticalSystem.Countdown < 0f)
                {
                    EndGameForSabotage(__instance);
                    criticalSystem.ClearSabotage();
                    return true;
                }
            }
            return false;
        }

        private static bool CheckAndEndGameForTaskWin(ShipStatus __instance)
        {
            if (GameData.Instance.TotalTasks <= GameData.Instance.CompletedTasks)
            {
                __instance.enabled = false;
                CustomEndGame(GameOverReason.HumansByTask, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForImpostorWin(ShipStatus __instance, PlayerStatistics statistics)
        {
            if (statistics.TeamImpostorsAlive >= statistics.TotalAlive - statistics.TeamImpostorsAlive && statistics.TeamJackalAlive == 0)
            {
                __instance.enabled = false;
                GameOverReason endReason;
                switch (TempData.LastDeathReason)
                {
                    case DeathReason.Exile:
                        endReason = GameOverReason.ImpostorByVote;
                        break;
                    case DeathReason.Kill:
                        endReason = GameOverReason.ImpostorByKill;
                        break;
                    default:
                        endReason = GameOverReason.ImpostorByVote;
                        break;
                }
                CustomEndGame(endReason, false);
                return true;
            }
            return false;
        }
        private static bool CheckAndEndGameForJackalWin(ShipStatus __instance, PlayerStatistics statistics)
        {
            if (statistics.TeamJackalAlive >= statistics.TotalAlive - statistics.TeamJackalAlive && statistics.TeamImpostorsAlive == 0)
            {
                __instance.enabled = false;
                CustomEndGame((GameOverReason)CustomGameOverReason.JackalWin, false);
                return true;
            }
            return false;
        }
        private static bool CheckAndEndGameForBugEnd(ShipStatus __instance, PlayerStatistics statistics)
        {
            if (statistics.CrewAlive == 0 && statistics.TotalAlive == 2 && statistics.TeamJackalAlive == 1 && statistics.TeamImpostorsAlive == 1 )
            {
                __instance.enabled = false;
                CustomEndGame((GameOverReason)CustomGameOverReason.BugEnd, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForCrewmateWin(ShipStatus __instance, PlayerStatistics statistics)
        {
            if (statistics.CrewAlive > 0 && statistics.TeamImpostorsAlive == 0 && statistics.TeamJackalAlive == 0)
            {
                __instance.enabled = false;
                CustomEndGame(GameOverReason.HumansByVote, false);
                return true;
            }
            return false;
        }
        private static void EndGameForSabotage(ShipStatus __instance)
        {
            __instance.enabled = false;
            CustomEndGame(GameOverReason.ImpostorBySabotage, false);
            return;
        }
        internal class PlayerStatistics
        {
            public int TeamImpostorsAlive { get; set; }
            public int CrewAlive { get; set; }
            public int TotalAlive { get; set; }
            public int TeamJackalAlive { get; set; }
            public PlayerStatistics(ShipStatus __instance)
            {
                GetPlayerCounts();
            }
            private void GetPlayerCounts()
            {
                int numImpostorsAlive = 0;
                int numCrewAlive = 0;
                int numTotalAlive = 0;
                int numTotalJackalTeam = 0;

                for (int i = 0; i < GameData.Instance.PlayerCount; i++)
                {
                    GameData.PlayerInfo playerInfo = GameData.Instance.AllPlayers[i];
                    if (!playerInfo.Disconnected)
                    {
                        if (playerInfo.Object.isAlive())
                        {
                            numTotalAlive++;

                            if (playerInfo.Role.IsImpostor)
                            {
                                numImpostorsAlive++;
                            }
                            else if (!playerInfo.Object.isNeutral())
                            {
                                numCrewAlive++;
                            }
                            else if (playerInfo.Object.isNeutral()) { 
                                if(playerInfo.Object.isRole(CustomRPC.RoleId.Jackal) || RoleClass.Jackal.SidekickPlayer.IsCheckListPlayerControl(playerInfo.Object))
                                {
                                    numTotalJackalTeam++;
                                }
                            }
                        }
                    }
                }

                TeamImpostorsAlive = numImpostorsAlive;
                TotalAlive = numTotalAlive;
                CrewAlive = numCrewAlive;
                TeamJackalAlive = numTotalJackalTeam;
            }
        }
    }
}
