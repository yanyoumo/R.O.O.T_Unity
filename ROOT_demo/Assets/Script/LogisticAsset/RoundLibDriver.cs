using System;
using System.Collections.Generic;
using System.Linq;
using ROOT.Consts;
using ROOT.SetupAsset;
using UnityEngine;

namespace ROOT
{
    public sealed class RoundLibDriver
    {
        public FSMLevelLogic owner;
        private LevelActionAsset _ActionAsset => owner.LevelAsset.ActionAsset;
        private RoundGist? GetRoundGistByStep(int step) => GetCurrentRoundGist(step);
        private StageType? Stage(int step) => GetCurrentType(step);

        public int StepCount => owner.LevelAsset.StepCount;
        public int LastStepCount => StepCount - 1;

        public RoundGist? PreCheckRoundGist => GetRoundGistByStep(StepCount + StaticNumericData.StageWarningThreshold);
        public RoundGist? CurrentRoundGist => GetRoundGistByStep(owner.LevelAsset.StepCount);
        public RoundGist? PreviousRoundGist => (LastStepCount >= 0) ? GetCurrentRoundGist(LastStepCount) : CurrentRoundGist;

        public StageType? CurrentStage => Stage(owner.LevelAsset.StepCount);

        public bool IsShopRound => CurrentStage == StageType.Shop;
        public bool IsRequireRound => CurrentStage == StageType.Require;
        public bool IsDestoryerRound => CurrentStage == StageType.Destoryer;
        public bool IsBossRound => CurrentStage == StageType.Boss;

        #region RoundData

        //TODO 这四个是要和实际的内容放一个夹层。
        private List<RoundData> RoundLib => _ActionAsset.RoundLib;
        private bool HasBossRound => _ActionAsset.HasBossRound;
        private bool Endless => _ActionAsset.Endless;
        private BossAdditionalSetupAsset BossSetup => _ActionAsset.BossSetup;

        private RoundData GetCurrentRound(int step, out int truncatedStep, out bool normalRoundEnded)
        {
            var loopedCount = 0;
            return GetCurrentRound(step, out truncatedStep, out normalRoundEnded, ref loopedCount);
        }

        private RoundData GetCurrentRound(int step, out int truncatedStep, out bool normalRoundEnded, ref int loopedCount)
        {
            var tmpStep = step;
            normalRoundEnded = false;
            foreach (var roundData in RoundLib)
            {
                tmpStep -= roundData.TotalLength;
                if (tmpStep < 0)
                {
                    truncatedStep = tmpStep + roundData.TotalLength;
                    loopedCount = 0;
                    return roundData;
                }
            }

            if (HasBossRound)
            {
                normalRoundEnded = true;
                truncatedStep = step - RoundLib.Sum(r => r.TotalLength);
                loopedCount = 0;
                return new RoundData();
            }

            if (Endless)
            {
                var extraStep = step - RoundLib.Sum(r => r.TotalLength);
                var res = GetCurrentRound(extraStep, out truncatedStep, out normalRoundEnded, ref loopedCount);
                loopedCount++;
                return res;
            }

            throw new ArgumentException("Round should have Ended");
        }

        public RoundGist GetCurrentRoundGist(int step)
        {
            var round = GetCurrentRound(step, out var truncatedStep, out var normalRoundEnded);
            if (!normalRoundEnded)
            {
                var stage = GetCurrentType(step);
                return round.ExtractGist(stage);
            }

            return new RoundGist {owner = RoundLib[0], Type = StageType.Boss};
        }

        public StageType GetCurrentType(int step)
        {
            var currentRound = GetCurrentRound(step, out int truncatedStep, out var normalRoundEnded);
            return !normalRoundEnded ? currentRound.GetCurrentType(truncatedStep) : StageType.Boss;
        }

        public int GetTruncatedStep(int step)
        {
            GetCurrentRound(step, out var res, out var B);
            return res;
        }

        private bool GetEndless
        {
            get
            {
                if (HasBossRound && Endless)
                {
#if UNITY_EDITOR
                    return false;
#else
                    throw new Exception("a round lib couldn't has boss and being endless");
#endif
                }

                return Endless;
            }
        }

        public int PlayableCount
        {
            get
            {
                if (GetEndless) return int.MaxValue;
                if (RoundLib == null) return 0;
                if (!HasBossRound) return RoundLib.Sum(round => round.TotalLength);
                return RoundLib.Sum(round => round.TotalLength) + BossSetup.BossLength;
            }
        }

        public bool HasEnded(int StepCount)
        {
            if (GetEndless)
            {
                return false;
            }

            return StepCount >= PlayableCount;
        }

        #endregion
    }
}
    
