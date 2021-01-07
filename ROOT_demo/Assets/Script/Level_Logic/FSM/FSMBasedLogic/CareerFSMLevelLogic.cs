using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace ROOT
{
    using FSMActions = Dictionary<RootFSMStatus, Action>;
    using FSMTransitions = List<RootFSMTransition>;

    public class CareerFSMLevelLogic : FSMLevelLogic //LEVEL-LOGIC/每一关都有一个这个类。
    {
        protected override FSMTransitions RootFSMTransitions
        {
            get
            {
                var transitions = new FSMTransitions
                {
                    new PreInit_UpKeep_0(),
                    new UpKeep_FCycle_2(),
                    new UpKeep_RIO_1(),
                    new UpKeep_UpKeep_0(),
                    new RIO_FCycle_0(),
                    new FCycle_Animate_1(),
                    new FCycle_Clean_0(),
                    new Animate_Clean_0(),
                    new Animate_Animate_1(),
                    new Clean_UpKeep_0(),
                };
                return transitions;
            }
        }
        protected override FSMActions fsmActions
        {
            get
            {
                var _fsmActions = new FSMActions
                {
                    {RootFSMStatus.PreInit, PreInit},
                    {RootFSMStatus.UpKeep, UpKeepAction},
                    {RootFSMStatus.F_Cycle, ForwardCycle},
                    {RootFSMStatus.CleanUp, CleanUp},
                    {RootFSMStatus.BossInit, BossInit},
                    {RootFSMStatus.Boss, BossUpdate},
                    {RootFSMStatus.Animate, AnimateAction},
                    {RootFSMStatus.R_IO, ReactIO},
                };
                return _fsmActions;
            }
        }
    }
}