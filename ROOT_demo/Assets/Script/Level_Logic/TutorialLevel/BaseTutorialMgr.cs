using System;
using System.Collections;
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
        protected bool PlayerRequestedEnd = false;

        protected abstract string MainGoalEntryContent { get; }
        protected virtual string SecondaryGoalEntryContent { get; } = "";

        public LevelActionAsset LevelActionAsset;
        protected bool ShowText
        {
            set => LevelAsset.HintMaster.RequestedShowTutorialContent = value;
        }

        protected bool ShowCheckList
        {
            set => LevelAsset.HintMaster.ShouldShowCheckList = value;
        }

        protected abstract override bool UpdateGameOverStatus(GameAssets currentLevelAsset);

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
            //LevelAsset.tutorialCheckList = FindObjectOfType<TutorialCheckList>();
            //_tutorialMainText = FindObjectOfType<TutorialMainTextFrame>();
            PopulateArtLevelReference();
        }

        public override void PopulateArtLevelReference()
        {
            base.PopulateArtLevelReference();
            LevelAsset.HintMaster.TutorialCheckList.SetupEntryContent(MainGoalEntryContent, SecondaryGoalEntryContent);
        }

        public abstract override void InitLevel(ScoreSet scoreSet = null, PerMoveData perMoveData = default);

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
            Text = Text.Replace("一般数据", TutorialActionBase.TMPNormalDataCompo());
            Text = Text.Replace("网络数据", TutorialActionBase.TMPNetworkDataCompo());
            Text = Text.Replace("收入/损失",
                TutorialActionBase.TmpBracketAndBold(TutorialActionBase.TmpColorXml("收入", Color.green * 0.4f) + "/" +
                                                     TutorialActionBase.TmpColorRedXml("损失")));
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
                    DisplayText(data.Text);
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
    }
}