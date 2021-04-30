using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
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

        [Range(0, 100)] public int InitialCurrency = 36;

        [HorizontalGroup("Split")] [VerticalGroup("Split/Left")] [LabelText("Shop has cost")]
        public bool ShopCost = true;

        [VerticalGroup("Split/Right")] [LabelText("Unit could cost")]
        public bool UnitCost = true;

        [Space] public LevelType levelType;

        [Header("Detail")]
        public AdditionalGameSetup AdditionalGameSetup;

        [ShowIf("levelType", LevelType.Career)]
        public UnitGist[] InitalBoard;

        [ShowIf("levelType", LevelType.Career)] [OnValueChanged("HasBossChanged")]
        public bool HasBossRound;

        [ShowIf("levelType", LevelType.Career)] [HideIf("HasBossRound")]
        public bool Endless;

        [ShowIf("levelType", LevelType.Career)] [ShowIf("HasBossRound")] [OnValueChanged("BossTypeChanged")]
        public BossStageType BossStage;

        public List<RoundData> RoundLib;

        [Header("Tutorial")]
        [ShowIf("levelType", LevelType.Tutorial)]
        [TableList(DrawScrollView = true, MinScrollViewHeight = 500, MaxScrollViewHeight = 1000)]
        public TutorialActionData[] Actions;

        [ShowIf("levelType", LevelType.Tutorial)]
        [Button("Reorder Tutorial Actions")]
        public void ReorderTutorialActions() => Actions = Actions.OrderBy(GetOrderingKeyOfTutorialAction).ToArray();

        [ShowIf("levelType", LevelType.Tutorial)]
        [Button("Shrink Tutorial Actions")]
        public void ShrinkTutorialActions() => throw new NotImplementedException();//TODO
        
        [ShowIf("levelType", LevelType.Tutorial)]
        [Button("Insert Tutorial Action")]
        public void InsertTutorialActions() => throw new NotImplementedException();//TODO
        
        private int GetOrderingKeyOfTutorialAction(TutorialActionData data)
        {
            return data.ActionIdx * 10 + data.ActionSubIdx;
        }

        public TutorialQuadDataPack TutorialQuadDataPack => new TutorialQuadDataPack(TitleTerm, "Play", Thumbnail);

        public BossStageType? GetBossStage => HasBossRound ? BossStage : (BossStageType?) null;

        public BossStageType BossStageVal
        {
            get
            {
                if (GetBossStage.HasValue) return GetBossStage.Value;
                throw new ArgumentException("this lib has no bossStage.");
            }
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
            try
            {
                BossSetup.BossStageTypeVal = BossStageVal;
            }
            catch (ArgumentException)
            {
                Debug.Log("Has no boss");
            }
        }

        public (int, bool, bool) GameStartingData => (InitialCurrency, ShopCost, UnitCost);

        [Obsolete("Why?")] public Vector2Int[] StationaryRateList => null;

        [Obsolete("Why?")] public List<SignalType> ShopExcludedType => null;

        [Obsolete("Why?")] [HideInInspector] public bool ExcludedShop = false;
    }
}