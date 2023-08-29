﻿using CountersPlus.Custom;
using CountersPlus.Utils;
using HMUI;
using PPPredictor.Data;
using PPPredictor.Interfaces;
using PPPredictor.Utilities;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace PPPredictor.Counter
{
    class CounterInfoHolder
    {
        private readonly int id = 0;
        private readonly int fontSize = 3;
        private readonly TMP_Text ppText;
        private readonly TMP_Text ppGainText;
        private readonly TMP_Text headerText;
        private readonly ImageView icon;
        private readonly bool showInfo;
        private readonly bool useIcon;
        private readonly Leaderboard leaderboard;
        private readonly CustomConfigModel settings;
        private readonly CanvasUtility canvasUtility;
        private readonly GameplayModifiers gameplayModifiers;
        private readonly PPPBeatMapInfo modifiedBeatMapInfo;
        private readonly PPPBeatMapInfo failedBeatMapInfo;
        private readonly float positionScale;
        private double maxPP = -1;
        private readonly IPPPredictorMgr ppPredictorMgr;
        private readonly string ppSuffix;

        public double MaxPP { get => maxPP; }

        public CounterInfoHolder(int id, Leaderboard leaderboard, CustomConfigModel settings, IPPPredictorMgr ppPredictorMgr, Canvas canvas, CanvasUtility canvasUtility, float lineOffset, float offsetByLine, float positionScale, GameplayModifiers gameplayModifiers) //CHECK WHEN NO C+ is installed??
        {
            this.id = id;
            this.leaderboard = leaderboard;
            this.settings = settings;
            this.ppPredictorMgr = ppPredictorMgr;
            this.canvasUtility = canvasUtility;
            this.positionScale = positionScale;
            float positionScaleFactor = 10 / positionScale;
            lineOffset *= positionScaleFactor;
            TextAlignmentOptions gainAlignment = TextAlignmentOptions.BottomLeft;
            float centerOffset = GetCenterOffset();
            float iconTextOffset = (Plugin.ProfileInfo.CounterUseIcons ? -.9f : 0f);
            float displayTypeOffset = 0;
            if (Plugin.ProfileInfo.CounterDisplayType == CounterDisplayType.PPNoSuffix || Plugin.ProfileInfo.CounterDisplayType == CounterDisplayType.PPAndGainNoSuffix || Plugin.ProfileInfo.CounterDisplayType == CounterDisplayType.PPAndGainNoBracketsNoSuffix || Plugin.ProfileInfo.CounterDisplayType == CounterDisplayType.GainNoBracketsNoSuffix)
                displayTypeOffset = -.2f;
            if (Plugin.ProfileInfo.CounterDisplayType == CounterDisplayType.GainNoBrackets)
            {
                displayTypeOffset = -0.2f;
                gainAlignment = TextAlignmentOptions.BottomRight;
            }
            if(Plugin.ProfileInfo.CounterDisplayType == CounterDisplayType.GainNoBracketsNoSuffix)
            {
                displayTypeOffset = -0.4f;
                gainAlignment = TextAlignmentOptions.BottomRight;
            }
            useIcon = (canvas != null && Plugin.ProfileInfo.CounterUseIcons);
            showInfo = ppPredictorMgr.IsRanked(leaderboard) || !Plugin.ProfileInfo.CounterHideWhenUnranked;
            headerText = canvasUtility.CreateTextFromSettings(settings, new Vector3(((-1f + centerOffset) * positionScaleFactor), lineOffset, 0));
            ppText = canvasUtility.CreateTextFromSettings(settings, new Vector3((0.9f + iconTextOffset + displayTypeOffset + centerOffset) * positionScaleFactor, lineOffset, 0));
            ppGainText = canvasUtility.CreateTextFromSettings(settings, new Vector3((1.2f + iconTextOffset + displayTypeOffset + centerOffset) * positionScaleFactor, lineOffset, 0));
            headerText.alignment = TextAlignmentOptions.BottomLeft;
            ppGainText.alignment = gainAlignment;
            ppText.alignment = TextAlignmentOptions.BottomRight;
            headerText.fontSize = ppText.fontSize = ppGainText.fontSize = fontSize;
            string iconPath = ppPredictorMgr.GetMapPoolIcon(leaderboard);
            if (useIcon)
            {
                icon = CreateIcon(canvas, iconPath, new Vector3((-1f + centerOffset) * positionScaleFactor, lineOffset, 0), Math.Abs(offsetByLine));
                LoadImage(icon, iconPath);
            }
            ppGainText.enabled = !Plugin.ProfileInfo.IsCounterGainSilentModeEnabled;
            this.gameplayModifiers = gameplayModifiers;
            modifiedBeatMapInfo = ppPredictorMgr.GetModifiedBeatMapInfo(leaderboard, gameplayModifiers);
            failedBeatMapInfo = ppPredictorMgr.GetModifiedBeatMapInfo(leaderboard, gameplayModifiers);
            ppSuffix = ppPredictorMgr.GetPPSuffixForLeaderboard(leaderboard);
        }

        private float GetCenterOffset()
        {
            switch (Plugin.ProfileInfo.CounterDisplayType)
            {
                case CounterDisplayType.PP:
                    return 0.5f;
                case CounterDisplayType.PPNoSuffix:
                    return 0.6f;
                case CounterDisplayType.PPAndGain:
                    return 0f;
                case CounterDisplayType.PPAndGainNoSuffix:
                    return 0.3f;
                case CounterDisplayType.PPAndGainNoBrackets:
                    return 0f;
                case CounterDisplayType.PPAndGainNoBracketsNoSuffix:
                    return 0.3f;
                case CounterDisplayType.GainNoBrackets:
                    return 0.4f;
                case CounterDisplayType.GainNoBracketsNoSuffix:
                    return 0.6f;
                default:
                    return 0;
            }
        }

        public void UpdateCounterText(double percentage, bool levelFailed)
        {
            string percentageThresholdColor = DisplayHelper.GetDisplayColor(0, false);
            if (percentage > ppPredictorMgr.GetPercentage() && Plugin.ProfileInfo.CounterHighlightTargetPercentage)
            {
                percentageThresholdColor = DisplayHelper.GetDisplayColor(1, false);
            }

            if (showInfo && !Plugin.ProfileInfo.CounterUseIcons) headerText.text = $"<color=\"{percentageThresholdColor}\">{leaderboard}</color>";
            if (showInfo)
            {
                if (Plugin.ProfileInfo.CounterUseIcons) icon.enabled = true;
                double pp = ppPredictorMgr.GetPPAtPercentageForCalculator(leaderboard, percentage, levelFailed, levelFailed ? failedBeatMapInfo : modifiedBeatMapInfo);
                double ppGain = Math.Round(ppPredictorMgr.GetPPGainForCalculator(leaderboard, pp), 2);

                if (maxPP == -1) maxPP = ppPredictorMgr.GetMaxPPForCalculator(leaderboard);

                string maxPPReachedPrefix = string.Empty;
                string maxPPReachedSuffix = string.Empty;

                if(maxPP > 0 && pp >= maxPP)
                {
                    maxPPReachedPrefix = "<color=\"yellow\">";
                    maxPPReachedSuffix = "</color>";
                }
                switch (Plugin.ProfileInfo.CounterDisplayType)
                {
                    case CounterDisplayType.PP:
                        ppText.text = $"{maxPPReachedPrefix}{pp:F2}{ppSuffix}{maxPPReachedSuffix}";
                        break;
                    case CounterDisplayType.PPAndGain:
                        ppText.text = $"{maxPPReachedPrefix}{pp:F2}{ppSuffix}{maxPPReachedSuffix}";
                        ppGainText.text = $"[<color=\"{DisplayHelper.GetDisplayColor(ppGain, false, true)}\">{ppGain:F2}{ppSuffix}</color>]";
                        break;
                    case CounterDisplayType.PPAndGainNoBrackets:
                        ppText.text = $"{maxPPReachedPrefix}{pp:F2}pp{maxPPReachedSuffix}";
                        ppGainText.text = $"<color=\"{DisplayHelper.GetDisplayColor(ppGain, false, true)}\">{ppGain:F2}{ppSuffix}</color>";
                        break;
                    case CounterDisplayType.GainNoBrackets:
                        ppGainText.text = $"<color=\"{DisplayHelper.GetDisplayColor(ppGain, false, true)}\">{ppGain:F2}{ppSuffix}</color>";
                        break;
                    case CounterDisplayType.PPNoSuffix:
                        ppText.text = $"{maxPPReachedPrefix}{pp:F2}{maxPPReachedSuffix}";
                        break;
                    case CounterDisplayType.PPAndGainNoSuffix:
                        ppText.text = $"{maxPPReachedPrefix}{pp:F2}{maxPPReachedSuffix}";
                        ppGainText.text = $"[<color=\"{DisplayHelper.GetDisplayColor(ppGain, false, true)}\">{ppGain:F2}</color>]";
                        break;
                    case CounterDisplayType.PPAndGainNoBracketsNoSuffix:
                        ppText.text = $"{maxPPReachedPrefix}{pp:F2}{maxPPReachedSuffix}";
                        ppGainText.text = $"<color=\"{DisplayHelper.GetDisplayColor(ppGain, false, true)}\">{ppGain:F2}</color>";
                        break;
                    case CounterDisplayType.GainNoBracketsNoSuffix:
                        ppGainText.text = $"<color=\"{DisplayHelper.GetDisplayColor(ppGain, false, true)}\">{ppGain:F2}</color>";
                        break;
                    default:
                        break;
                }
            }
        }

        private ImageView CreateIcon(Canvas canvas, string imageIdent, Vector3 offset, float lineOffset)
        {
            GameObject imageGameObject = new GameObject(imageIdent, typeof(RectTransform));
            ImageView newImage = imageGameObject.AddComponent<ImageView>();
            newImage.rectTransform.SetParent(canvas.transform, false);
            newImage.rectTransform.anchoredPosition = positionScale * (canvasUtility.GetAnchoredPositionFromConfig(settings) + offset + new Vector3(0, (lineOffset / (positionScale * 0.125f)) + (0.15f / positionScale), 0));
            newImage.rectTransform.sizeDelta = new Vector2(2.5f, 2.5f);
            newImage.enabled = false;
            var noGlowMat = new Material(Resources.FindObjectsOfTypeAll<Material>().Where(m => m.name == "UINoGlow").First())
            {
                name = "UINoGlowCustom"
            };
            newImage.material = noGlowMat;
            return newImage;
        }

        private async void LoadImage(ImageView newImage, string imageIdent)
        {
            byte[] data = null;
            if (Plugin.ProfileInfo.CounterUseCustomMapPoolIcons && imageIdent.Contains("http"))
            {
                data = await ppPredictorMgr.GetLeaderboardIconData(leaderboard);
            }
            if(data == null)
            {
                var assembly = Assembly.GetExecutingAssembly();
                System.IO.Stream stream = assembly.GetManifestResourceStream(ppPredictorMgr.GetLeaderboardIcon(leaderboard));
                data = new byte[stream.Length];
                stream.Read(data, 0, (int)stream.Length);
            }
            Texture2D texture = new Texture2D(1, 1);
            texture.LoadImage(data);
            texture.Apply();
            newImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        }

        internal void MoveIconForLongMaxPP(int digits)
        {
            if(digits > 3)
            {
                icon.rectTransform.anchoredPosition -= new Vector2((digits - 3) * 1f, 0);
            }
        }

        #region animation stuff
        public async Task ShowPPGainWithAnimation()
        {
            Vector3 originalPPGainPosition = ppGainText.transform.position;
            await Task.Delay((int)(100f * id));
            ppGainText.enabled = true;
            int steps = 25;
            Vector3 offset = new Vector3(0, .3f, 0);
            ppGainText.transform.position = originalPPGainPosition - offset;
            for (int i = 0; i < steps; i++)
            {
                float t = (float)i / (float)steps;
                t = Mathf.Sin(t * Mathf.PI * 0.5f); //ease
                ppGainText.transform.position = originalPPGainPosition - Vector3.Lerp(offset, new Vector3(), t);
                ppGainText.alpha = t;
                await Task.Delay(10);
            }
            ppGainText.transform.position = originalPPGainPosition;
        }

        public async Task HidePPGainWithAnimation()
        {
            await Task.Delay((int)(100f * id));
            int steps = 25;
            for (int i = 0; i < steps; i++)
            {
                float t = (float)i / (float)steps;
                t = Mathf.Sin(t * Mathf.PI * 0.5f); //ease
                ppGainText.alpha = 1f - t;
                await Task.Delay(10);
            }
            ppGainText.enabled = false;
        }
        #endregion
    }
}
