using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ROOT
{
    public class CareerFSMLevelLogic : LevelLogic //LEVEL-LOGIC/每一关都有一个这个类。
    {
        public void UpkeepAction()
        {

        }

        public void MajorCycle()
        {

        }

        public void Execute()
        {
            switch (MainFSM.currentStatus)
            {
                case RootFSMStatus.PreInit:
                    break;
                case RootFSMStatus.Idle:
                    break;
                case RootFSMStatus.Upkeep:
                    UpkeepAction();
                    break;
                case RootFSMStatus.Cycle:
                    break;
                case RootFSMStatus.CleanUp:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private RootFSMBase MainFSM;

        public override void InitLevel()
        {
            MainFSM = gameObject.AddComponent<CareerDefaultFSM>();
            MainFSM.owner = this;
        }

        protected override void Update()
        {
            //base.Update();
        }
    }
}