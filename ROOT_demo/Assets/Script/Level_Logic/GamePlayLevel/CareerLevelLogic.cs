using UnityEngine;
using UnityEngine.SceneManagement;

namespace ROOT
{
    public class CareerLevelLogic : LevelLogic //LEVEL-LOGIC/每一关都有一个这个类。
    {
        public override void InitLevel()
        {
            Debug.Assert(ReferenceOk);//意外的有确定Reference的……还行……
            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(StaticName.SCENE_ID_ADDTIVELOGIC));

            InitCurrencyIoMgr();
            LevelAsset.DeltaCurrency = 0.0f;

            LevelAsset.GameStateMgr = new GameStateMgr();
            LevelAsset.GameStateMgr.InitGameMode(LevelAsset.ActionAsset.GameModeAsset);

            InitShop();
            InitDestoryer();
            InitCursor(new Vector2Int(2, 3));
            LevelAsset.EnableAllCoreFunctionAndFeature();
            LevelAsset.GameBoard.InitBoardWAsset(LevelAsset.ActionAsset);
            LevelAsset.GameBoard.UpdateBoardAnimation();
            StartShop();

            ReadyToGo = true;
            if (LevelAsset.ActionAsset.RoundDatas.Length>0)
            {
                LevelAsset.StepCount = 0;
                LevelAsset.TimeLine.InitWithAssets(LevelAsset);
            }
            LevelAsset.TimeLine.SetGoalCount = LevelAsset.ActionAsset.TargetCount;
            LevelAsset.SignalPanel.TGTtMission= LevelAsset.ActionAsset.TargetCount;
            LevelAsset.HintMaster.ShouldShowCheckList = false;
        }

        protected void InitDestoryer()
        {
            LevelAsset.WarningDestoryer = new MeteoriteBomber {GameBoard = LevelAsset.GameBoard};
            LevelAsset.WarningDestoryer.Init(4, 1);
        }
        protected void InitShop()
        {
            LevelAsset.Shop.ShopInit(LevelAsset);
            LevelAsset.Shop.CurrentGameStateMgr = LevelAsset.GameStateMgr;
            LevelAsset.Shop.GameBoard = LevelAsset.GameBoard;
            if (LevelAsset.ActionAsset.ExcludedShop)
            {
                LevelAsset.Shop.excludedTypes = LevelAsset.ActionAsset.ShopExcludedType;
            }
        }
        protected void InitCurrencyIoMgr()
        {
            LevelAsset.BoardDataCollector = gameObject.AddComponent<BoardDataCollector>();
            LevelAsset.BoardDataCollector.m_Board = LevelAsset.GameBoard;
        }
        protected void InitCursor(Vector2Int pos)
        {
            LevelAsset.GameCursor = Instantiate(LevelAsset.CursorTemplate);
            Cursor cursor = LevelAsset.GameCursor.GetComponent<Cursor>();
            cursor.InitPosWithAnimation(pos);
            cursor.UpdateTransform(LevelAsset.GameBoard.GetFloatTransformAnimation(cursor.LerpingBoardPosition));
        }

        protected override bool UpdateGameOverStatus(GameAssets currentLevelAsset)
        {
            var res= UpdateCareerGameOverStatus(currentLevelAsset);
            LevelAsset.TimeLine.SetCurrentCount = RequirementSatisfiedCycleCount;
            LevelAsset.SignalPanel.CRTMission = RequirementSatisfiedCycleCount;
            return res;
        }

        //private int _obsoletedID = -1;
        /*protected override void Update()
        {
            base.Update();
        }*/
    }
}