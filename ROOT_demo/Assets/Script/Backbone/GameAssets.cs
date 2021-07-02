using System;
using System.Collections.Generic;
using Cinemachine;
using ROOT.SetupAsset;
using UnityEngine;

namespace ROOT
{
    public enum GameStatus
    {
        Starting,
        Playing,
        Tutorial,
        Ended
    }

    public class GameGlobalStatus
    {
        public GameStatus CurrentGameStatus;
    }

    /// <summary>
    /// 一个每个关卡都有这么一个类，在Lvl-WRD之间传来传去。这个类只有一个，做成最通用的样子。
    /// 意外地、根据现有流程；这个类似乎也要去掉了；所有数据直接从FSM里面读取最新的。
    /// </summary>
    [Serializable]
    public sealed class GameAssets //ASSET 这里不应该有任何实际的逻辑（有些便于操作的除外
    {
        public int StepCount => WorldCycler.Step;
        public float LevelProgress = 0.0f;
        public bool BuyingCursor = false;
        public int BuyingID = -1;
        
        public FSMLevelLogic Owner;
        public bool? TutorialCompleted = null;
        public LevelActionAsset ActionAsset;

        //这些引用在Asset外面要设好，在WRD-LOGIC里面也要处理。
        public GameObject CursorTemplate;
        public Board GameBoard;
        public TimeLine TimeLine;
        public SignalType? DestoryedCoreType;
        public InfoAirdrop AirDrop;
        public int ReqOkCount;
        public int SignalInfo;
        public List<Vector2Int> CollectorZone => GameBoard.GetInfoCollectorZone();

        public CinemachineFreeLook CineCam;

        internal GameObject GameCursor;
        internal Cursor Cursor
        {
            get
            {
                if (GameCursor != null) return GameCursor.GetComponent<Cursor>();
                Debug.LogWarning("Requested Cursor but not found");
                return null;
            }
        }

        internal GameCurrencyMgr GameCurrencyMgr;
        internal float CurrencyRebate = 1.0f;
        internal ShopSelectableMgr Shop;
        internal SkillMgr SkillMgr;
        internal IWarningDestoryer WarningDestoryer;
        internal GameObject[] WarningGo;
        internal GameObject SkillIndGoA;
        internal GameObject[] SkillIndGoB;
        public int occupiedHeatSinkCount=0;

        public GameOverAsset GameOverAsset;

        public float DeltaCurrency => BaseDeltaCurrency + BonusDeltaCurrency;
        public float BaseDeltaCurrency;
        public float BonusDeltaCurrency;

        //internal flag 
        internal bool _boughtOnce = false;

        public bool BoughtOnce
        {
            get => _boughtOnce;
            internal set => _boughtOnce = value;
        }

        internal bool MovedTileAni = false;
        internal bool MovedCursorAni = false;
        internal List<MoveableBase> AnimationPendingObj;

        //CoreFunctionFlag
        public bool InputEnabled = true;
        public bool CurrencyEnabled = true;
        /*public bool BoardCouldIOCurrency = true;
        public bool UnitCouldGenerateIncome = true;*/
        
        //FeatureFunctionFlag
        public bool CursorEnabled = true;
        public bool RotateEnabled = true;
        public bool ShopEnabled = true;
        public bool SkillEnabled = true;

        public bool DestroyerEnabled = true;

        //LevelLogicFlag
        public bool GameOverEnabled = true;

        //UtilsFlag
        public bool HintEnabled = true;


        //一些辅助函数可以在这里。

        internal void EnableAllCoreFunction()
        {
            InputEnabled = true;
            CurrencyEnabled = true;
        }

        internal void DisableAllCoreFunction()
        {
            InputEnabled = false;
            CurrencyEnabled = false;
        }

        internal void EnableAllCoreFunctionAndFeature()
        {
            InputEnabled = true;
            CursorEnabled = true;
            /*BoardCouldIOCurrency = true;
            UnitCouldGenerateIncome = true;*/
            RotateEnabled = true;
            ShopEnabled = true;
            SkillEnabled = true;
            CurrencyEnabled = true;
            DestroyerEnabled = true;
            HintEnabled = true;
            GameOverEnabled = true;
        }

        internal void DisableAllCoreFunctionAndFeature()
        {
            InputEnabled = false;
            CursorEnabled = false;
            /*BoardCouldIOCurrency = false;
            UnitCouldGenerateIncome = false;*/
            RotateEnabled = false;
            ShopEnabled = false;
            SkillEnabled = false;
            CurrencyEnabled = false;
            DestroyerEnabled = false;
            HintEnabled = false;
            GameOverEnabled = false;
        }
    }
}