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

        protected abstract override bool UpdateGameOverStatus(GameAssets currentLevelAsset);

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
            Debug.Log(text);
        }

        protected abstract void DealStep(TutorialActionData data);
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