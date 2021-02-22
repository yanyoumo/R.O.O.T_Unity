using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace ROOT
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

        [ShowIf("levelType", LevelType.Career)]
        public RoundLibGist roundLibGist;

        [ShowIf("levelType", LevelType.Career)]
        [Button("Create New RoundDatas From Gist")]
        public void CreateRoundDatasFromGist()
        {
            if (roundLibGist.NormalRoundCount <= 0)
            {
                Debug.LogError("Can't create zero length Rounds");
            }
            else
            {
                RoundLib = new RoundLib();
                for (int i = 0; i < roundLibGist.NormalRoundCount; i++)
                {
                    RoundLib.Add(new RoundData {ID = i});
                }

                if (roundLibGist.HasBossRound)
                {
                    RoundLib.Add(new RoundData
                    {
                        ID = RoundLib.Count,
                        RoundTypeData = RoundType.Boss,
                        bossStageType = roundLibGist.BossStage
                    });
                }
            }
        }

        [OdinSerialize] [ShowIf("levelType", LevelType.Career)]
        public RoundLib RoundLib;

        [Header("Tutorial")] [ShowIf("levelType", LevelType.Tutorial)]
        public TutorialActionData[] Actions;

        public StageType GetStageType(int step) => RoundLib.GetCurrentType(step);

        public TutorialQuadDataPack TutorialQuadDataPack => new TutorialQuadDataPack(TitleTerm, "Play", Thumbnail);

        private bool IsEndless => !roundLibGist.HasBossRound && roundLibGist.Endless;

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

        public RoundGist? PeekBossRoundGist() => RoundLib.PeekBossRoundGist();
        
        [Obsolete] private bool IsTelemetry => roundLibGist.HasBossRound && roundLibGist.BossStage == StageType.Telemetry;
        [Obsolete] public int TelemetryCount => IsTelemetry ? RoundLib.Last().bossStageLength : 0;
        [Obsolete] public int InfoCount => IsTelemetry ? RoundLib.Last().InfoCount : 0;
        [Obsolete] public int InfoVariantRatio => IsTelemetry ? RoundLib.Last().InfoVariantRatio : 0;
        [Obsolete] public int InfoTargetRatio => IsTelemetry ? RoundLib.Last().InfoTargetRatio : 0;

        [Obsolete("Why?")] public Vector2Int[] StationaryRateList => null;

        [Obsolete("Why?")] public List<SignalType> ShopExcludedType => null;

        [Obsolete("Why?")] public bool ExcludedShop = false;
    }
}