﻿using PPPredictor.Utilities;

namespace PPPredictor.Data.DisplayInfos
{
    internal class DisplayPPInfo
    {
        internal string PPRaw { get; set; } = string.Empty;
        internal string PPGain { get; set; } = string.Empty;
        internal string PPGainDiffColor { get; set; } = DisplayHelper.ColorWhite;
        internal string PredictedRank { get; set; } = string.Empty;
        internal string PredictedRankDiff { get; set; } = string.Empty;
        internal string PredictedRankDiffColor { get; set; } = DisplayHelper.ColorWhite;
        internal string PredictedCountryRank { get; set; } = string.Empty;
        internal string PredictedCountryRankDiff { get; set; } = string.Empty;
        internal string PredictedCountryRankDiffColor { get; set; } = DisplayHelper.ColorWhite;
    }
}
