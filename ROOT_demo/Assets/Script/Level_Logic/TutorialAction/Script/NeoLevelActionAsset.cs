using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ROOT
{
    [Serializable]
    public struct NeoRoundData
    {
        public int ID;

        public RoundType RoundType;
        
        [Range(0,60)]
        public int ShopLength;

        [Space]
        [Range(0, 30)]
        public int RequireLength;
        [HorizontalGroup("Split")]
        [VerticalGroup("Split/Left")]
        public int TypeARequirement;
        [VerticalGroup("Split/Right")]
        public int TypeBRequirement;

        [Space]
        [Range(0, 100)]
        public int HeatSinkLength;

        [ShowInInspector]
        public int TotalLength => ShopLength + RequireLength + HeatSinkLength;

        public bool InRange(int truncatedCount)
        {
            return truncatedCount < TotalLength;
        }
    }
    
    [Serializable]
    [CreateAssetMenu(fileName = "NewNeoActionAsset", menuName = "Neo ActionAsset/New Neo ActionAsset")]
    public class NeoLevelActionAsset : ScriptableObject
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
                if (RoundDatasGist.HasBossRound)
                {
                    RoundDatas = new NeoRoundData[RoundDatasGist.NormalRoundCount+1];
                    RoundDatas[RoundDatas.Length - 1].RoundType = RoundType.Boss;
                }
                else
                {
                    RoundDatas = new NeoRoundData[RoundDatasGist.NormalRoundCount];
                }
            }
        }
        
        [ShowIf("levelType", LevelType.Career)]
        public NeoRoundData[] RoundDatas;
    }
}