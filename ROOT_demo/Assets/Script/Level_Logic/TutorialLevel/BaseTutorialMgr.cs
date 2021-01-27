using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using static ROOT.TextProcessHelper;

namespace ROOT
{
    [Obsolete]
    public class TutorialLogic : BranchingLevelLogic { }
    
    public abstract partial class FSMTutorialLogic
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

        private bool ShowText
        {
            set => LevelAsset.HintMaster.RequestedShowTutorialContent = value;
        }

        private bool ShowCheckList
        {
            set => LevelAsset.HintMaster.ShouldShowCheckList = value;
        }

        protected bool UpdateGameOverStatus(GameAssets currentLevelAsset)
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
        
        protected void CreateUnitOnBoard(TutorialActionData data)
        {
            GameObject go = LevelAsset.GameBoard.InitUnit(Vector2Int.zero, data.Core,data.HardwareType, Utils.Shuffle(data.Sides), data.Tier);
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

        private void StepForward()
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

        private void DisplayText(string text)
        {
            //Debug.Log(text);
            LevelAsset.HintMaster.TutorialContent = ProcessText(text);
        }

        protected abstract void AddtionalDealStep(TutorialActionData data);
        
        /// <summary>
        /// Tutorial父类里面会为通用的动作做一个处理。如果没有会throw
        /// </summary>
        /// <param name="data">输入的TutorialActionData</param>
        private void DealStep(TutorialActionData data)
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
                    AddtionalDealStep(data);
                    break;
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

        /*protected sealed override void Awake()
        {
            base.Awake();
            LevelAsset.TutorialCompleted = false;
        }*/

        /*protected override void Update()
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
        }*/

        protected void InitShop()
        {
            LevelAsset.Shop.ShopInit(LevelAsset);
            LevelAsset.Shop.CurrentGameStateMgr = LevelAsset.GameStateMgr;
            LevelAsset.Shop.GameBoard = LevelAsset.GameBoard;
            if (LevelActionAsset.ExcludedShop)
            {
                LevelAsset.Shop.excludedTypes = LevelActionAsset.ShopExcludedType;
            }
        }

        protected void ForceSetWarningDestoryer(Vector2Int nextIncome)
        {
            MeteoriteBomber obj = LevelAsset.WarningDestoryer as MeteoriteBomber;
            obj?.ForceSetDestoryer(nextIncome);
        }

        protected bool AllUnitConnected()
        {
            return LevelAsset.GameBoard.UnitsGameObjects.All(gameBoardUnit => gameBoardUnit.Value.GetComponentInChildren<Unit>().AnyConnection);
        }
    }
}