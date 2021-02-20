using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace ROOT
{
    public class RoundDatas:IList<NeoRoundData>
    {
        [NonSerialized]
        [OdinSerialize]
        private List<NeoRoundData> core;

        public StageType GetCurrentType(int step)
        {
            return GetCurrentType(step, out var A);
        }

        public StageType GetCurrentType(int step,out int truncatedStep)
        {
            var tmpStep = step;
            var currentRoundData = core[0];
            truncatedStep = 0;
            foreach (var neoRoundData in core)
            {
                tmpStep -= neoRoundData.TotalLength;
                if (tmpStep<0)
                {
                    currentRoundData = neoRoundData;
                    truncatedStep = tmpStep + currentRoundData.TotalLength;
                    break;
                }
            }
            return currentRoundData.GetCurrentType(truncatedStep);
        }
        
        public RoundDatas()
        {
            core = new List<NeoRoundData>();
        }
        
        #region INTERFACE

        public IEnumerator<NeoRoundData> GetEnumerator()
        {
            return core.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(NeoRoundData item)
        {
            core.Add(item);
        }

        public void Clear()
        {
            core.Clear();
        }

        public bool Contains(NeoRoundData item)
        {
            return core.Contains(item);
        }

        public void CopyTo(NeoRoundData[] array, int arrayIndex)
        {
            core.CopyTo(array, arrayIndex);
        }

        public bool Remove(NeoRoundData item)
        {
            return core.Remove(item);
        }

        public int Count => core.Count;
        public bool IsReadOnly => false;
        public int IndexOf(NeoRoundData item)
        {
            return core.IndexOf(item);
        }

        public void Insert(int index, NeoRoundData item)
        {
            core.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            core.RemoveAt(index);
        }

        public NeoRoundData this[int index]
        {
            get => core[index];
            set => core[index] = value;
        }
        #endregion
    }
    
    [Serializable]
    public struct NeoRoundData
    {
        [ReadOnly]
        public int ID;
        
        [ReadOnly]
        public RoundType RoundTypeData;
        
        [Range(0,60)]
        [HideIf("@RoundTypeData == RoundType.Boss")]
        public int ShopLength;

        [Space]
        [Range(0, 30)]
        [HideIf("@RoundTypeData == RoundType.Boss")]
        public int RequireLength;
        [HorizontalGroup("Split")]
        [VerticalGroup("Split/Left")]
        [HideIf("@RoundTypeData == RoundType.Boss")]
        public int TypeARequirement;
        [VerticalGroup("Split/Right")]
        [HideIf("@RoundTypeData == RoundType.Boss")]
        public int TypeBRequirement;

        [Space]
        [Range(0, 60)]
        [HideIf("@RoundTypeData == RoundType.Boss")]
        public int HeatSinkLength;
        
        [ReadOnly]
        [ShowIf("@RoundTypeData == RoundType.Boss")]
        public StageType bossStageType;//这里还要做一个Filter但是现在先不用。
        
        [Range(0, 100)]
        [ShowIf("@RoundTypeData == RoundType.Boss")]
        public int bossStageLength;
        
        [ShowIf("@RoundTypeData == RoundType.Boss&&bossStageType==StageType.Telemetry")]
        public int InfoCount;
        
        [ShowIf("@RoundTypeData == RoundType.Boss&&bossStageType==StageType.Acquiring")]
        public int AcquiringTarget;

        [ShowInInspector]
        public int TotalLength => RoundTypeData == RoundType.Normal
                ? ShopLength + RequireLength + HeatSinkLength
                : bossStageLength;

        public (StageType,int) this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return (StageType.Shop,ShopLength);
                    case 1:
                        return (StageType.Require,RequireLength);
                    case 2:
                        return (StageType.Destoryer,HeatSinkLength);
                    case 3:
                        return (bossStageType,bossStageLength);
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }

        public StageType GetCurrentType(int truncatedStep)
        {
            if (truncatedStep<=TotalLength)
            {
                if (RoundTypeData == RoundType.Normal)
                {
                    var tmpTStep = truncatedStep;
                    for (int i = 0; i < 3; i++)
                    {
                        tmpTStep -= this[i].Item2;
                        if (tmpTStep<0)
                        {
                            return this[i].Item1;
                        }
                    }
                    throw new ArgumentException();
                }
                else
                {
                    return bossStageType;
                }
            }
            throw new ArgumentException();
        }
    }
    
    [Serializable]
    [CreateAssetMenu(fileName = "NewNeoActionAsset", menuName = "Neo ActionAsset/New Neo ActionAsset")]
    public class NeoLevelActionAsset :SerializedScriptableObject
    {
        [Header("Basic Data")]
        public string TitleTerm;
        [AssetSelector(Filter = "t:Sprite",Paths= "Assets/Resources/UIThumbnail/TutorialThumbnail")]
        public Sprite Thumbnail;
        [Required]
        [AssetSelector(Filter = "t:Prefab", Paths = "Assets/Resources/LevelLogicPrefab")]
        public GameObject LevelLogic;
        [EnumToggleButtons]
        public LevelType levelType;
        
        [Header("Career")] 
        [ShowIf("levelType", LevelType.Career)]
        public AdditionalGameSetup AdditionalGameSetup;
        
        [ShowIf("levelType", LevelType.Career)]
        public RoundDatasGist RoundDatasGist;
        
        [ShowIf("levelType", LevelType.Career)]
        [Button("Create New RoundDatas From Gist")]
        public void CreateRoundDatasFromGist()
        {
            if (RoundDatasGist.NormalRoundCount<=0)
            {
                Debug.LogError("Can't create zero length Rounds");
            }
            else
            {
                RoundDatas = new RoundDatas();
                for (int i = 0; i < RoundDatasGist.NormalRoundCount; i++)
                {
                    RoundDatas.Add(new NeoRoundData{ID=i});
                }

                if (RoundDatasGist.HasBossRound)
                {
                    RoundDatas.Add(new NeoRoundData
                    {
                        ID=RoundDatas.Count,
                        RoundTypeData = RoundType.Boss,
                        bossStageType = RoundDatasGist.BossStage
                    });
                }
            }
        }
        
        [OdinSerialize]
        [ShowIf("levelType", LevelType.Career)]
        public RoundDatas RoundDatas;

        public StageType GetStageType(int step) => RoundDatas.GetCurrentType(step);
    }
}