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
        public bool UseStaticLib = true;
        public List<RoundData> DynamicRoundLib;//主要是现有框架下能不能处理为空的RoundLib。
        
        private LevelActionAsset _ActionAsset => owner.LevelAsset.ActionAsset;
        private RoundGist? GetRoundGistByStep(int step) => GetCurrentRoundGist(step);
        private StageType? Stage(int step) => GetCurrentType(step);
        private List<RoundData> RoundLib => UseStaticLib ? _ActionAsset.RoundLib : DynamicRoundLib;
        private bool HasBossRound => _ActionAsset.HasBossRound;
        private bool Endless => _ActionAsset.Endless;
        private BossAdditionalSetupAsset BossSetup => _ActionAsset.BossSetup;
        private bool GetEndless
        {
            get
            {
                if (HasBossRound && Endless)
                {
                    return false;
                }

                return Endless;
            }
        }

        
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
        
        public void StretchCurrentRound(int step)
        {
            if (UseStaticLib)
            {
                Debug.LogError("Try to stretch static lib, operation abort!!!");
                return;
            }

            var crtRound = GetCurrentRound(step, out var truncatedStep, out var normalRoundEnded);
            var crtRoundGist = GetCurrentRoundGist(step);

            //自己手动复制构造函数、因为是可序列化结构、这个东西不要在构造里面搞。
            var newRound = new RoundData
            {
                ID = DynamicRoundLib[crtRound.ID].ID,
                ShopLength = DynamicRoundLib[crtRound.ID].ShopLength,
                RequireLength = DynamicRoundLib[crtRound.ID].RequireLength,
                HeatSinkLength = DynamicRoundLib[crtRound.ID].HeatSinkLength,
                TypeARequirement = DynamicRoundLib[crtRound.ID].TypeARequirement,
                TypeBRequirement = DynamicRoundLib[crtRound.ID].TypeBRequirement,
            };

            switch (crtRoundGist.Type)
            {
                case StageType.Shop:
                    newRound.ShopLength++;
                    break;
                case StageType.Require:
                    newRound.RequireLength++;
                    break;
                case StageType.Destoryer:
                    newRound.HeatSinkLength++;
                    break;
                default:
                    Debug.LogError("Try to stretch non-stretchable round, operation abort!!!");
                    return;
            }
            DynamicRoundLib[crtRound.ID] = newRound;
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

        public int GetCurrentStageRemainingStep(int step)
        {
            var crtRound = GetCurrentRound(step, out var truncatedStep, out var normalRoundEnded);
            switch (GetCurrentType(step))
            {
                case StageType.Shop:
                    return crtRound[0].Item2 - truncatedStep;
                case StageType.Require:
                    return crtRound[0].Item2 + crtRound[1].Item2 - truncatedStep;
                case StageType.Destoryer:
                    return crtRound[0].Item2 + crtRound[1].Item2 + crtRound[2].Item2 - truncatedStep;
                case StageType.Boss:
                    throw new NotImplementedException("Boss round Remaining not implemented, yet.");
                default: //Ending
                    return 0;
            }
        }

        public int GetTruncatedStep(int step)
        {
            GetCurrentRound(step, out var res, out var B);
            return res;
        }
        
        public int TotalPlayableCount
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

            return StepCount >= TotalPlayableCount;
        }

        public bool IsLastNormalRound(int StepCount)
        {
            if (GetEndless)
            {
                return false;
            }

            var round = GetCurrentRound(StepCount, out var tStep, out var data);
            return round.ID == RoundLib.Count() - 1;
        }
        
        #endregion
    }
}
    
