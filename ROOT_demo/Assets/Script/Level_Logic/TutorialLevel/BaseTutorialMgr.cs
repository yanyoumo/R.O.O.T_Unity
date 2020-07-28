using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ROOT
{
    public abstract class BaseTutorialMgr : BaseLevelMgr
    {
        protected int ActionIndex { get; private set; } = -1;
        protected int LastActionCount { get; private set; } = 0;
        protected TutorialActionBase TutorialAction;
        //private TutorialMainTextFrame _tutorialMainText;
        protected bool ShowText
        {
            set => LevelAsset.HintMaster.RequestedShowTutorialContent = value;
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
            //_tutorialMainText = FindObjectOfType<TutorialMainTextFrame>();
            PopulateArtLevelReference();
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

        protected virtual void DisplayText(string text)
        {
            //Debug.Log(text);
            LevelAsset.HintMaster.TutorialContent = text;
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
                    LevelMasterManager.Instance.LevelFinished(LevelAsset);
                    break;
                case TutorialActionType.ShowText:
                    ShowText = true;
                    break;
                case TutorialActionType.HideText:
                    ShowText = false;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
        protected void DealStepMgr()
        {
            int actionLength = TutorialAction.Actions.Length;
            for (int i = LastActionCount; i < actionLength; i++)
            {
                if (TutorialAction.Actions[i].ActionIdx > ActionIndex)
                {
                    LastActionCount = i;
                    break;
                }
                DealStep(TutorialAction.Actions[i]);
            }
        }

        protected override void Update()
        {
            base.Update();

            if (ReadyToGo)
            {
                if (ActionIndex == -1)
                {
                    StepForward();
                    DealStepMgr();
                }

                if (Input.GetButtonDown(StaticName.INPUT_BUTTON_NAME_QUIT))
                {
                    //TODO　先什么的都不做。
                }
                else
                {
                    if (Input.GetButtonDown(StaticName.INPUT_BUTTON_NAME_NEXT))
                    {
                        StepForward();
                        DealStepMgr();
                    }
                }
            }
        }
    }
}