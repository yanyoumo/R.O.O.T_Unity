using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ROOT
{
    public abstract class TutorialLogic : LevelLogic
    {
        protected bool ActionEnded { get; private set; } = false;
        protected int ActionIndex { get; private set; } = -1;
        protected int LastActionCount { get; private set; } = 0;

        protected bool LevelCompleted = false;
        protected bool LevelFailed = false;
        protected bool PlayerRequestedEnd = false;
        protected bool PlayerRequestedQuit = false;

        protected bool OnceFlagA = false;
        protected bool OnceFlagB = false;

        protected abstract string MainGoalEntryContent { get; }
        protected virtual string SecondaryGoalEntryContent { get; } = "";

        public LevelActionAsset LevelActionAsset => LevelAsset.ActionAsset;
        protected bool ShowText
        {
            set => LevelAsset.HintMaster.RequestedShowTutorialContent = value;
        }

        protected bool ShowCheckList
        {
            set => LevelAsset.HintMaster.ShouldShowCheckList = value;
        }

        #region TextProcess

        public static string TmpColorBlueXml(string content)
        {
            return TmpColorXml(content, Color.blue);
        }

        public static string TmpColorGreenXml(string content)
        {
            return TmpColorXml(content, Color.green * 0.35f);
        }

        public static string TmpColorXml(string content, Color col)
        {
            var hexCol = ColorUtility.ToHtmlStringRGB(col);
            return "<color=#" + hexCol + ">" + content + "</color>";
        }

        public static string TmpColorBold(string content)
        {
            return "<b>" + content + "</b>";
        }

        public static string TmpBracket(string content)
        {
            return "[" + content + "]";
        }

        public static string TmpBracketAndBold(string content)
        {
            return TmpColorBold("[" + content + "]");
        }

        public static string TMPNormalDataCompo()
        {
            return TmpBracketAndBold(TmpColorGreenXml("一般数据"));
        }

        public static string TMPNetworkDataCompo()
        {
            return TmpBracketAndBold(TmpColorBlueXml("网络数据"));
        }

        #endregion

        protected override bool UpdateGameOverStatus(GameAssets currentLevelAsset)
        {
            if (LevelCompleted && PlayerRequestedEnd)
            {
                PendingCleanUp = true;
                LevelAsset.TutorialCompleted = true;
                LevelMasterManager.Instance.LevelFinished(LevelAsset);
                return true;
            }else if (LevelFailed && PlayerRequestedQuit)
            {
                PendingCleanUp = true;
                LevelAsset.TutorialCompleted = false;
                LevelMasterManager.Instance.LevelFinished(LevelAsset);
                return true;
            }
            else
            {
                return false;
            }
        }

        public override IEnumerator UpdateArtLevelReference(AsyncOperation aOP)
        {
            while (!aOP.isDone)
            {
                yield return 0;
            }
            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(StaticName.SCENE_ID_ADDTIVEVISUAL));
            LevelAsset.ItemPriceRoot = GameObject.Find("PlayUI");
            LevelAsset.DataScreen = FindObjectOfType<DataScreen>();
            LevelAsset.HintMaster = FindObjectOfType<HintMaster>();
            LevelAsset.TimeLine = FindObjectOfType<TimeLine>();
            PopulateArtLevelReference();
        }

        public override void PopulateArtLevelReference()
        {
            base.PopulateArtLevelReference();
            LevelAsset.HintMaster.TutorialCheckList.SetupEntryContent(MainGoalEntryContent, SecondaryGoalEntryContent);
        }

        public abstract override void InitLevel();

        protected void CreateUnitOnBoard(TutorialActionData data)
        {
            GameObject go = LevelAsset.GameBoard.InitUnit(Vector2Int.zero, data.Core, Utils.Shuffle(data.Sides));
            if (data.Pos.x < 0 || data.Pos.y < 0)
            {
                LevelAsset.GameBoard.DeliverUnitRandomPlace(go);
            }
            else
            {
                LevelAsset.GameBoard.DeliverUnitAssignedPlace(go, data.Pos);
            }
            LevelAsset.GameBoard.UpdateBoardInit();
        }

        protected virtual void StepForward()
        {
            ActionIndex++;
        }

        private string ProcessText(string Text)
        {
            Text = Text.Replace("\\n", "\n");
            Text = Text.Replace("单元", "<b>[单元]</b>");
            Text = Text.Replace("方形", "<b>[方形]</b>");
            Text = Text.Replace("圆形", "<b>[圆形]</b>");
            Text = Text.Replace("周期", "<b>[周期]</b>");
            Text = Text.Replace("一般数据", TMPNormalDataCompo());
            Text = Text.Replace("网络数据", TMPNetworkDataCompo());
            Text = Text.Replace("收入/损失", TmpBracketAndBold(TmpColorXml("收入", Color.green * 0.4f) + "/" + TmpColorGreenXml("损失")));
            Text = Text.Replace("绿色", TmpBracketAndBold(TmpColorXml("绿色", Color.green * 0.4f)));
            Text = Text.Replace("红色", TmpColorXml("红色", Color.red));
            ColorUtility.TryParseHtmlString("#71003E", out Color col);
            Text = Text.Replace("深紫色", TmpColorXml("深紫色", col));
            return Text;
        }

        protected virtual void DisplayText(string text)
        {
            //Debug.Log(text);
            LevelAsset.HintMaster.TutorialContent = ProcessText(text);
        }

        /// <summary>
        /// Tutorial父类里面会为通用的动作做一个处理。如果没有会throw
        /// </summary>
        /// <param name="data">输入的TutorialActionData</param>
        protected virtual void DealStep(TutorialActionData data)
        {
            switch (data.ActionType)
            {
                case TutorialActionType.Text:
                    if (data.DoppelgangerToggle)
                    {
                        DisplayText(StartGameMgr.UseTouchScreen ? data.DoppelgangerText : data.Text);
                    }
                    else
                    {
                        DisplayText(data.Text);
                    }
                    break;
                case TutorialActionType.CreateUnit:
                    CreateUnitOnBoard(data);
                    break;
                case TutorialActionType.End:
                    ActionEnded = true;
                    break;
                case TutorialActionType.ShowText:
                    ShowText = true;
                    break;
                case TutorialActionType.HideText:
                    ShowText = false;
                    break;
                case TutorialActionType.ShowCheckList:
                    ShowCheckList = true;
                    break;
                case TutorialActionType.HideCheckList:
                    ShowCheckList = false;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
        protected void DealStepMgr()
        {
            int actionLength = LevelActionAsset.Actions.Length;
            for (int i = LastActionCount; i < actionLength; i++)
            {
                if (LevelActionAsset.Actions[i].ActionIdx > ActionIndex)
                {
                    LastActionCount = i;
                    break;
                }
                DealStep(LevelActionAsset.Actions[i]);
            }
        }

        protected void InitCursor(Vector2Int pos)
        {
            LevelAsset.GameCursor = Instantiate(LevelAsset.CursorTemplate);
            Cursor cursor = LevelAsset.GameCursor.GetComponent<Cursor>();
            cursor.InitPosWithAnimation(pos);
            cursor.UpdateTransform(LevelAsset.GameBoard.GetFloatTransformAnimation(cursor.LerpingBoardPosition));
        }

        protected sealed override void Awake()
        {
            base.Awake();
            LevelAsset.TutorialCompleted = false;
        }

        protected override void Update()
        {
            base.Update(); //严格来说ControlPack在这里搞定了。

            if (ReadyToGo)
            {
                if (!ActionEnded)
                {
                    if (ActionIndex == -1)
                    {
                        StepForward();
                        DealStepMgr();
                    }

                    if (CtrlPack.HasFlag(ControllingCommand.NextButton))
                    {
                        StepForward();
                        DealStepMgr();
                    }
                }
            }
        }

        protected void InitShop()
        {
            LevelAsset.ShopMgr = gameObject.AddComponent<ShopMgr>();
            LevelAsset.ShopMgr.UnitTemplate = LevelAsset.GameBoard.UnitTemplate;
            LevelAsset.ShopMgr.ShopInit(LevelAsset);
            LevelAsset.ShopMgr.ItemPriceTexts_TMP = new[] { LevelAsset.Item1PriceTmp, LevelAsset.Item2PriceTmp, LevelAsset.Item3PriceTmp, LevelAsset.Item4PriceTmp };
            LevelAsset.ShopMgr.CurrentGameStateMgr = LevelAsset.GameStateMgr;
            LevelAsset.ShopMgr.GameBoard = LevelAsset.GameBoard;
            if (LevelActionAsset.ExcludedShop)
            {
                LevelAsset.ShopMgr.excludedTypes = LevelActionAsset.ShopExcludedType;
            }
        }

        protected void ForceSetWarningDestoryer(Vector2Int nextIncome)
        {
            MeteoriteBomber obj = LevelAsset.WarningDestoryer as MeteoriteBomber;
            obj?.ForceSetDestoryer(nextIncome);
        }

        protected void InitCurrencyIoMgr()
        {
            LevelAsset.BoardDataCollector = gameObject.AddComponent<BoardDataCollector>();
            LevelAsset.BoardDataCollector.m_Board = LevelAsset.GameBoard;
        }

        protected bool AllUnitConnected()
        {
            return LevelAsset.GameBoard.UnitsGameObjects.All(gameBoardUnit => gameBoardUnit.Value.GetComponentInChildren<Unit>().AnyConnection);
        }
    }
}