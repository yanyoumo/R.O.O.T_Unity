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

        [Space] [EnumToggleButtons] public LevelType levelType;

        [Header("Career")] [ShowIf("levelType", LevelType.Career)]
        public AdditionalGameSetup AdditionalGameSetup;

        [ShowIf("levelType", LevelType.Career)]
        public UnitGist[] InitalBoard;

        [ShowIf("levelType", LevelType.Career)] [OnValueChanged("HasBossChanged")]
        public bool HasBossRound;

        [ShowIf("levelType", LevelType.Career)] [HideIf("HasBossRound")]
        public bool Endless;

        [ShowIf("levelType", LevelType.Career)] [ShowIf("HasBossRound")] [OnValueChanged("BossTypeChanged")]
        public BossStageType BossStage;

        [ShowIf("levelType", LevelType.Career)]
        public List<RoundData> RoundLib;
        //TODO 这里就是下一个硬骨头、主要是这个RoundLib一直被设置成"静态的"，下一个工作就是搞一个API把这个东西变成动态的。
        //这个东西本身估计没法直接硬拆、估计还是加一个夹层结构、有一个基础RoundLib，中间插一个可调整的东西。
        //其中一个比较有问题的就是需要下面相关读取代码提出来。有可能想办法把"静态"版注入Asset里面去、然后相关的处理直接放在FSM里面。

        [Header("Tutorial")]
        [ShowIf("levelType", LevelType.Tutorial)]
        [TableList(DrawScrollView = true, MinScrollViewHeight = 500, MaxScrollViewHeight = 1000)]
        public TutorialActionData[] Actions;

        [ShowIf("levelType", LevelType.Tutorial)]
        [Button("Reorder Tutorial Actions")]
        public void ReorderTutorialActions() => Actions = Actions.OrderBy(GetOrderingKeyOfTutorialAction).ToArray();

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