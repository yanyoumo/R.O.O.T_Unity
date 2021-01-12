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
    using Trans= RootFSMTransition;
    using FSMTransitions = List<RootFSMTransition>;
    using Status = RootFSMStatus;

    public class CareerFSMLevelLogic : FSMLevelLogic //LEVEL-LOGIC/每一关都有一个这个类。
    {
        protected override FSMTransitions RootFSMTransitions
        {
            get
            {
                var transitions = new FSMTransitions
                {
                    new Trans(Status.PreInit, Status.F_Cycle, 1, CheckInited),
                    new Trans(Status.PreInit),
                    new Trans(Status.UpKeep, Status.F_Cycle, 2, CheckAutoF),
                    new Trans(Status.UpKeep, Status.R_IO, 1, CheckCtrlPackAny),
                    new Trans(Status.UpKeep),
                    new Trans(Status.R_IO, Status.Skill, 3, CheckIsSkill),
                    new Trans(Status.R_IO, Status.F_Cycle, 2, CheckFCycle),
                    new Trans(Status.R_IO, Status.Animate, 1, CheckAnimating, TriggerAnimation),
                    new Trans(Status.R_IO, Status.UpKeep, 0,true),
                    new Trans(Status.F_Cycle, Status.Career_Cycle, 0),
                    new Trans(Status.Skill, Status.Career_Cycle, 0),
                    new Trans(Status.Career_Cycle, Status.Animate, 1, CheckAnimating, TriggerAnimation),
                    new Trans(Status.Career_Cycle, Status.CleanUp, 0),
                    new Trans(Status.Animate, Status.Animate, 1, true, CheckAnimating),
                    new Trans(Status.Animate, Status.CleanUp, 0, CheckNotAnimating),
                    new Trans(Status.CleanUp, Status.UpKeep, 0, true),
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
                    {Status.PreInit, PreInit},
                    {Status.UpKeep, UpKeepAction},
                    {Status.F_Cycle, ForwardCycle},
                    {Status.Career_Cycle, CareerCycle},
                    {Status.CleanUp, CleanUp},
                    {Status.BossInit, BossInit},
                    {Status.Boss, BossUpdate},
                    {Status.Animate, AnimateAction},
                    {Status.R_IO, ReactIO},
                    {Status.Skill, SkillMajorSkill},
                };
                return _fsmActions;
            }
        }
    }
}