using System;
using System.Collections;
using System.Collections.Generic;
using ROOT;
using UnityEngine;

namespace ROOT
{
    public abstract class RootFSMTransition : IComparable<RootFSMTransition>
    {
        public RootFSM owner;
        public FSMLevelLogic levelLogic => owner.owner as FSMLevelLogic;
        public abstract int priority { get; } //这个值越高优先级越高。
        public abstract RootFSMStatus StartingStatus { get; } //Transit的高优先级要求，如果FSM不是这个状态则不考虑。
        public abstract bool AdditionalReq();
        public Func<bool> AdditionalReqFunc;
        public abstract void Consequence();

        public int CompareTo(RootFSMTransition other)
        {
            return other.priority - priority;
        }

        public RootFSMTransition()
        {

        }

        public RootFSMTransition(Func<bool> req)
        {
            //理论上讲，这么写也可以。
            AdditionalReqFunc = req;
        }
    }

    //这套FSM的实现，是基于Transit的流程、每个不同的FSM只需要在一开始注册不同的Transit，那么将成为不同的FSM。
    //不同的FSM在相同状态执行的逻辑和离开逻辑是相同的。
    //每个状态将会有结束逻辑和转移逻辑、结束后才进行转移逻辑的判断。
    //命名原则：StartingStatus_TargetStatus_Priority
    //DONE
    //DONE
    public class PreInit_UpKeep_0 : RootFSMTransition
    {
        public override int priority => 0;
        public override RootFSMStatus StartingStatus => RootFSMStatus.PreInit;

        public override bool AdditionalReq()
        {
            return (levelLogic.ReadyToGo) && (!levelLogic.PendingCleanUp);
        }

        public override void Consequence()
        {
            if (!levelLogic.Playing) levelLogic.Playing = true;
            owner.currentStatus = RootFSMStatus.UpKeep;
        }
    }
    //DONE
    public class UpKeep_BossInit_6 : RootFSMTransition
    {
        public override int priority => 6;
        public override RootFSMStatus StartingStatus => RootFSMStatus.UpKeep;

        public override bool AdditionalReq()
        {
            return (levelLogic.stage == StageType.Boss) && (!WorldCycler.BossStage);
        }

        public override void Consequence()
        {
            owner.currentStatus = RootFSMStatus.BossInit;
        }
    }
    //DONE
    public class UpKeep_Boss_5 : RootFSMTransition
    {
        public override int priority => 5;
        public override RootFSMStatus StartingStatus => RootFSMStatus.UpKeep;

        public override bool AdditionalReq()
        {
            return (levelLogic.stage == StageType.Boss);
        }

        public override void Consequence()
        {
            owner.currentStatus = RootFSMStatus.Boss;
        }
    }

    public class UpKeep_Skill_4 : RootFSMTransition
    {
        public override int priority => 4;
        public override RootFSMStatus StartingStatus => RootFSMStatus.UpKeep;

        public override bool AdditionalReq()
        {
            throw new NotImplementedException();
        }

        public override void Consequence()
        {
            owner.currentStatus = RootFSMStatus.Skill;
        }
    }
    //DONE
    public class UpKeep_RCycle_3 : RootFSMTransition
    {
        public override int priority => 3;
        public override RootFSMStatus StartingStatus => RootFSMStatus.UpKeep;

        public override bool AdditionalReq()
        {
            var autoDrive = WorldCycler.NeedAutoDriveStep;
            return autoDrive.HasValue && !autoDrive.Value;
        }

        public override void Consequence()
        {
            owner.currentStatus = RootFSMStatus.R_Cycle;
        }
    }
    //DONE
    public class UpKeep_FCycle_2 : RootFSMTransition
    {
        public override int priority => 2;
        public override RootFSMStatus StartingStatus => RootFSMStatus.UpKeep;

        public override bool AdditionalReq()
        {
            var autoDrive = WorldCycler.NeedAutoDriveStep;
            return autoDrive.HasValue && autoDrive.Value;
        }

        public override void Consequence()
        {
            owner.currentStatus = RootFSMStatus.F_Cycle;
        }
    }
    //DONE
    public class UpKeep_RIO_1 : RootFSMTransition
    {
        public override int priority => 1;
        public override RootFSMStatus StartingStatus => RootFSMStatus.UpKeep;

        public override bool AdditionalReq()
        {
            return levelLogic.CtrlPack.AnyFlag();
        }

        public override void Consequence()
        {
            owner.currentStatus = RootFSMStatus.R_IO;
        }
    }
    //DONE
    public class UpKeep_UpKeep_0 : RootFSMTransition
    {
        public override int priority => 0;
        public override RootFSMStatus StartingStatus => RootFSMStatus.UpKeep;

        public override bool AdditionalReq()
        {
            return true;
        }

        public override void Consequence()
        {
            owner.currentStatus = RootFSMStatus.UpKeep;
        }
    }
    //DONE
    public class RIO_FCycle_0 : RootFSMTransition
    {
        public override int priority => 0;
        public override RootFSMStatus StartingStatus => RootFSMStatus.R_IO;

        public override bool AdditionalReq()
        {
            return true;
        }

        public override void Consequence()
        {
            owner.currentStatus = RootFSMStatus.F_Cycle;
        }
    }
    //DONE
    public class BossInit_Boss_0 : RootFSMTransition
    {
        public override int priority => 0;
        public override RootFSMStatus StartingStatus => RootFSMStatus.BossInit;

        public override bool AdditionalReq()
        {
            return true;
        }

        public override void Consequence()
        {
            owner.currentStatus = RootFSMStatus.Boss;
        }
    }
    //DONE
    public class FCycle_Animate_1 : RootFSMTransition
    {
        public override int priority => 1;
        public override RootFSMStatus StartingStatus => RootFSMStatus.F_Cycle;

        public override bool AdditionalReq()
        {
            return levelLogic.Animating;
        }

        public override void Consequence()
        {
            owner.currentStatus = RootFSMStatus.Animate;
        }
    }
    //DONE
    public class FCycle_Clean_0 : RootFSMTransition
    {
        public override int priority => 0;
        public override RootFSMStatus StartingStatus => RootFSMStatus.F_Cycle;

        public override bool AdditionalReq()
        {
            return true;
        }

        public override void Consequence()
        {
            owner.currentStatus = RootFSMStatus.CleanUp;
        }
    }
    //DONE
    public class Animate_Animate_1 : RootFSMTransition
    {
        public override int priority => 1;
        public override RootFSMStatus StartingStatus => RootFSMStatus.Animate;

        public override bool AdditionalReq()
        {
            return levelLogic.Animating;
        }

        public override void Consequence()
        {
            owner.currentStatus = RootFSMStatus.Animate;
        }
    }
    //DONE
    public class Animate_Clean_0 : RootFSMTransition
    {
        public override int priority => 0;
        public override RootFSMStatus StartingStatus => RootFSMStatus.Animate;

        public override bool AdditionalReq()
        {
            return !levelLogic.Animating;
        }

        public override void Consequence()
        {
            owner.currentStatus = RootFSMStatus.CleanUp;
        }
    }
    //DONE
    public class Clean_UpKeep_0 : RootFSMTransition
    {
        public override int priority => 0;
        public override RootFSMStatus StartingStatus => RootFSMStatus.CleanUp;

        public override bool AdditionalReq()
        {
            return true;
        }

        public override void Consequence()
        {
            owner.currentStatus = RootFSMStatus.UpKeep;
        }
    }
}