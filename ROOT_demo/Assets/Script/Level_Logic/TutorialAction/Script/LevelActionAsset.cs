using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace ROOT.SetupAsset
{
    [Serializable]
    [CreateAssetMenu(fileName = "NewActionAsset", menuName = "ActionAsset/New ActionAsset")]
    public class LevelActionAsset : SerializedScriptableObject
    {
        [Header("Basic Data")] public string TitleTerm;

        [AssetSelector(Filter = "t:Sprite", Paths = "Assets/Resources/UIThumbnail/TutorialThumbnail")]
        public Sprite Thumbnail;

        [Required] [AssetSelector(Filter = "t:Prefab", Paths = "Assets/Resources/LevelLogicPrefab")]
        public GameObject LevelLogic;
        
        [Range(0, 100)]
        public int InitialCurrency = 36;

        [HorizontalGroup("Split")] [VerticalGroup("Split/Left")]
        [LabelText("Shop has cost")]
        public bool ShopCost = true;
        
        [VerticalGroup("Split/Right")]
        [LabelText("Unit could cost")]
        public bool UnitCost = true;
        
        [Space] [EnumToggleButtons] public LevelType levelType;
        
        [Header("Career")] [ShowIf("levelType", LevelType.Career)]
        public AdditionalGameSetup AdditionalGameSetup;

        [ShowIf("levelType", LevelType.Career)]
        public UnitGist[] InitalBoard;

        [ShowIf("levelType", LevelType.Career)] [OnValueChanged("HasBossChanged")]
        public bool HasBossRound;

        [ShowIf("levelType", LevelType.Career)] [HideIf("HasBossRound")]
        public bool Endless;

        [ShowIf("levelType", LevelType.Career)] [ShowIf("HasBossRound")]
        [OnValueChanged("BossTypeChanged")]
        public BossStageType BossStage;
        
        [ShowIf("levelType", LevelType.Career)]
        public List<RoundData> RoundLib;

        [Header("Tutorial")] [ShowIf("levelType", LevelType.Tutorial)]
        public TutorialActionData[] Actions;
        
        public TutorialQuadDataPack TutorialQuadDataPack => new TutorialQuadDataPack(TitleTerm, "Play", Thumbnail);
        
        public BossStageType? GetBossStage => HasBossRound ? BossStage : (BossStageType?) null;
        public BossStageType BossStageVal
        {
            get
            {
                if (GetBossStage.HasValue)
                {
                    return GetBossStage.Value;
                }
                throw new ArgumentException("this lib has no bossStage.");
            }
        }

        public bool GetEndless
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

        [ShowInInspector]
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
        
        [ShowInInspector] [ShowIf("HasBossRound")]
        public BossAdditionalSetupAsset BossSetup;

        private RoundData GetCurrentRound(int step, out int truncatedStep, out bool normalRoundEnded)
        {
            var loopedCount = 0;
            return GetCurrentRound(step, out truncatedStep, out normalRoundEnded, ref loopedCount);
        }

        private RoundData GetCurrentRound(int step, out int truncatedStep,out bool normalRoundEnded,ref int loopedCount)
        {
            var tmpStep = step;
            normalRoundEnded = false;
            foreach (var roundData in RoundLib)
            {
                tmpStep -= roundData.TotalLength;
                if (tmpStep<0)
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
            var round = GetCurrentRound(step,out var truncatedStep,out var normalRoundEnded);
            if (!normalRoundEnded)
            {
                var stage = GetCurrentType(step);
                return round.ExtractGist(stage);
            }
            return new RoundGist {owner = RoundLib[0], Type = StageType.Boss};
        }
        
        public StageType GetCurrentType(int step)
        {
            var currentRound=GetCurrentRound(step, out int truncatedStep,out var normalRoundEnded);
            return !normalRoundEnded ? currentRound.GetCurrentType(truncatedStep) : StageType.Boss;
        }

        public int GetTruncatedStep(int step)
        {
            GetCurrentRound(step, out var res, out var B);
            return res;
        }
        
        private void HasBossChanged()
        {
            if (HasBossRound)
            {
                Endless = false;
            }
        }
        
        private void BossTypeChanged()
        {
            BossSetup.BossStageTypeVal = BossStageVal;
        }
        
        public (int, bool, bool) GameStartingData => (InitialCurrency, ShopCost, UnitCost);

        [Obsolete("Why?")] public Vector2Int[] StationaryRateList => null;

        [Obsolete("Why?")] public List<SignalType> ShopExcludedType => null;

        [Obsolete("Why?")] [HideInInspector] public bool ExcludedShop = false;
    }
}