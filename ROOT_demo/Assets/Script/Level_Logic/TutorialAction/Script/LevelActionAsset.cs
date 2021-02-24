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

        [EnumToggleButtons] public LevelType levelType;

        [Required] [AssetSelector(Paths = "Assets/Resources/GameMode")]
        public GameModeAsset GameModeAsset;

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

        [OdinSerialize] [ShowIf("levelType", LevelType.Career)]
        public RoundLib RoundLibVal;

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
                if (RoundLibVal == null) return 0;
                if (!HasBossRound) return RoundLibVal.Sum(round => round.TotalLength);
                return RoundLibVal.Sum(round => round.TotalLength) + BossSetup.BossLength;
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
        
        [Obsolete("Why?")] public Vector2Int[] StationaryRateList => null;

        [Obsolete("Why?")] public List<SignalType> ShopExcludedType => null;

        [Obsolete("Why?")] [HideInInspector] public bool ExcludedShop = false;
    }
}