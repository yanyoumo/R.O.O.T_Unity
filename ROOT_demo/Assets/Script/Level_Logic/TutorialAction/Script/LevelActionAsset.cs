using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace ROOT.SetupAsset
{
    [Serializable]
    public struct TutorialActionData
    {
        [HorizontalGroup("Split")]
        [VerticalGroup("Split/Left")]
        [LabelWidth(80)]
        public int ActionIdx;
        [EnumPaging]
        [VerticalGroup("Split/Right")]
        public TutorialActionType ActionType;
        [ShowIf("ActionType", TutorialActionType.Text)]
        [LabelWidth(30)]
        public string Text;
        [ShowIf("ActionType", TutorialActionType.Text)]
        [HorizontalGroup("Doppelganger")]
        [LabelWidth(135)]
        public bool DoppelgangerToggle;
        [ShowIf("@this.ActionType==TutorialActionType.Text&&this.DoppelgangerToggle")]
        [HorizontalGroup("Doppelganger")]
        [LabelWidth(135)]
        public string DoppelgangerText;
        [ShowIf("@this.ActionType==TutorialActionType.CreateUnit||this.ActionType==TutorialActionType.CreateCursor")]
        public Vector2Int Pos;
        [ShowIf("ActionType", TutorialActionType.CreateUnit)]
        [VerticalGroup("Split/Left")]
        public SignalType Core;
        [ShowIf("ActionType", TutorialActionType.CreateUnit)]
        [VerticalGroup("Split/Left")]
        public HardwareType HardwareType;
        [ShowIf("ActionType", TutorialActionType.CreateUnit)]
        [VerticalGroup("Split/Right")]
        public SideType[] Sides;
        [ShowIf("ActionType", TutorialActionType.CreateUnit)]
        [Range(1,5)]
        public int Tier;
    }

    [Serializable] [CreateAssetMenu(fileName = "NewActionAsset", menuName = "ActionAsset/New ActionAsset")]
    public class LevelActionAsset : SerializedScriptableObject
    {
        public BossAssetLib BossLib;
        
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

        [ShowIf("levelType", LevelType.Career)]
        public int NormalRoundCount;
        [ShowIf("levelType", LevelType.Career)]
        public bool HasBossRound;
        [ShowIf("levelType", LevelType.Career)]
        [HideIf("HasBossRound")] 
        public bool Endless;
        [ShowIf("levelType", LevelType.Career)]
        [ShowIf("HasBossRound")]
        [ValueDropdown("BossStageFilter")]
        public StageType BossStage;
        //下面Boss的数量提出来放成一个好配置的。
        private static IEnumerable<StageType> BossStageFilter = Enumerable.Range((int)StageType.Telemetry, 2).Cast<StageType>();
        
        [ShowIf("levelType", LevelType.Career)]
        [Button("Create New RoundDatas")]
        public void CreateRoundDatasFromGist()
        {
            if (NormalRoundCount <= 0)
            {
                Debug.LogError("Can't create zero length Rounds");
            }
            else
            {
                RoundLib = new RoundLib(Endless);
                for (int i = 0; i < NormalRoundCount; i++)
                {
                    RoundLib.Add(new RoundData {ID = i});
                }

                if (HasBossRound)
                {
                    RoundLib.Add(new RoundData
                    {
                        ID = RoundLib.Count,
                        RoundTypeData = RoundType.Boss,
                        bossStageType = BossStage
                    });
                    //TODO
                    BossSetup = new TelemetryAdditionalData();
                }
            }
        }

        [OdinSerialize] [ShowIf("levelType", LevelType.Career)]
        public RoundLib RoundLib;

        [Header("Tutorial")] [ShowIf("levelType", LevelType.Tutorial)]
        public TutorialActionData[] Actions;

        public StageType GetStageType(int step) => RoundLib.GetCurrentType(step);

        public TutorialQuadDataPack TutorialQuadDataPack => new TutorialQuadDataPack(TitleTerm, "Play", Thumbnail);

        private bool IsEndless => !RoundLib.HasBossRound && RoundLib.Endless;

        public int PlayableCount => IsEndless ? int.MaxValue : RoundLib.Sum(round => round.TotalLength);

        public bool HasEnded(int StepCount)
        {
            if (IsEndless)
            {
                return false;
            }

            return StepCount >= PlayableCount;
        }
        
        public int GetTruncatedCount(int totalCount) => RoundLib.GetTruncatedStep(totalCount);

        public RoundGist GetRoundGistByStep(int stepCount) => RoundLib.GetCurrentRoundGist(stepCount);

        public RoundGist PeekBossRoundGistVal => RoundLib.PeekBossRoundGistVal;

        [OdinSerialize] public AdditionalBossSetupBase BossSetup;
        
        [Obsolete("Why?")] public Vector2Int[] StationaryRateList => null;

        [Obsolete("Why?")] public List<SignalType> ShopExcludedType => null;

        [Obsolete("Why?")] public bool ExcludedShop = false;
    }
}