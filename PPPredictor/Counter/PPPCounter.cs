﻿using PPPredictor.Utilities;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Zenject;

namespace PPPredictor.Counter
{
    public class PPPCounter : CountersPlus.Counters.Custom.BasicCustomCounter
    {
        [Inject] private readonly ScoreController scoreController;
        [Inject] private readonly GameplayCoreSceneSetupData setupData;
        private List<CounterInfoHolder> lsCounterInfoHolder;
        private int maxPossibleScore = 0;
#if DEBUG
        private TMP_Text debugPercentage;
#endif
        private readonly float originalLineOffset = 0.15f;

        public override void CounterInit()
        {
            try
            {
                if (setupData.practiceSettings == null)
                {
                    SetupCounter();
                }
            }
            catch (Exception ex)
            {
                Plugin.Log?.Error($"CounterInit Error: {ex.Message}");
            }
            
        }

        private void SetupCounter()
        {
            try
            {
                
#if DEBUG
                debugPercentage = CanvasUtility.CreateTextFromSettings(Settings, new Vector3(0, 0, 0));
                debugPercentage.alignment = TextAlignmentOptions.Center;
#endif
                var canvas = CanvasUtility.GetCanvasFromID(this.Settings.CanvasID);
                float positionScale = CanvasUtility.GetCanvasSettingsFromCanvas(canvas).PositionScale;
                lsCounterInfoHolder = new List<CounterInfoHolder>();
                int scoreboardCount = GetActiveScoreboardsCount();
                float lineOffset = (originalLineOffset * (scoreboardCount / 2)) + (originalLineOffset * (scoreboardCount % 2));
                if (Plugin.ProfileInfo.IsScoreSaberEnabled && ShowCounter(Leaderboard.ScoreSaber))
                {
                    lsCounterInfoHolder.Add(new CounterInfoHolder(Leaderboard.ScoreSaber, Settings, "PPPredictor.Resources.LeaderBoardLogos.ScoreSaber.png", canvas, CanvasUtility, lineOffset, positionScale));
                    lineOffset -= originalLineOffset * 2;
                }
                if (Plugin.ProfileInfo.IsBeatLeaderEnabled && ShowCounter(Leaderboard.BeatLeader))
                {
                    lsCounterInfoHolder.Add(new CounterInfoHolder(Leaderboard.BeatLeader, Settings, "PPPredictor.Resources.LeaderBoardLogos.BeatLeader.png", canvas, CanvasUtility, lineOffset, positionScale));
                    lineOffset -= originalLineOffset * 2;
                }

                maxPossibleScore = ScoreModel.ComputeMaxMultipliedScoreForBeatmap(setupData.transformedBeatmapData);
                scoreController.scoreDidChangeEvent += ScoreController_scoreDidChangeEvent;
                CalculatePercentages();
            }
            catch (Exception ex)
            {
                Plugin.Log?.Error($"SetupCounter Error: {ex.Message}");
            }
        }

        private void ScoreController_scoreDidChangeEvent(int arg1, int arg2)
        {
            CalculatePercentages();
        }

        private void CalculatePercentages()
        {
            try
            {
                double percentage = 0;
                switch (Plugin.ProfileInfo.CounterScoringType)
                {
                    case CounterScoringType.Global:
                        percentage = maxPossibleScore > 0 ? ((double)scoreController.multipliedScore / maxPossibleScore) * 100.0 : 0;
                        break;
                    case CounterScoringType.Local:
                        percentage = scoreController.immediateMaxPossibleMultipliedScore > 0 ? ((double)scoreController.multipliedScore / scoreController.immediateMaxPossibleMultipliedScore) * 100.0 : 0;
                        break;
                }
                DisplayCounterText(percentage);
            }
            catch (Exception ex)
            {
                Plugin.Log?.Error($"CalculatePercentages Error: {ex.Message}");
            }
        }

        public override void CounterDestroy()
        {
            scoreController.scoreDidChangeEvent -= ScoreController_scoreDidChangeEvent;
        }

        private void DisplayCounterText(double percentage)
        {
#if DEBUG
            debugPercentage.text = $"{Plugin.ProfileInfo.CounterScoringType} {percentage:F2}%";
#endif
            lsCounterInfoHolder.ForEach(item => item.UpdateCounterText(percentage));
        }

        //Stupid way to do it but works
        private int GetActiveScoreboardsCount()
        {
            int reVal = 0;
            if (ShowScoreSaber()) reVal++;
            if (ShowBeatLeader()) reVal++;
            return reVal;
        }

        private bool ShowCounter(Leaderboard leaderboard)
        {
            return Plugin.pppViewController.ppPredictorMgr.IsRanked(leaderboard) || !Plugin.ProfileInfo.CounterHideWhenUnranked;
        }

        private bool ShowScoreSaber()
        {
            return Plugin.ProfileInfo.IsScoreSaberEnabled && ShowCounter(Leaderboard.ScoreSaber);
        }

        private bool ShowBeatLeader()
        {
            return Plugin.ProfileInfo.IsBeatLeaderEnabled && ShowCounter(Leaderboard.BeatLeader);
        }
    }
}
