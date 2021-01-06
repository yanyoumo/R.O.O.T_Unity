using System;
using System.Collections;
using System.Collections.Generic;
using ROOT;
using UnityEngine;

namespace ROOT
{
    public abstract class RootFSMTransition : IComparable<RootFSMTransition>
    {
        public RootFSMBase owner;
        public LevelLogic greatOwner => owner.owner;
        public abstract int priority { get; } //���ֵԽ�����ȼ�Խ�ߡ�
        public abstract RootFSMStatus StartingStatus { get; } //Transit�ĸ߽�Ҫ�����FSM�������״̬�򲻿��ǡ�
        public abstract bool AdditionalReq();
        public abstract void Consequence();

        public int CompareTo(RootFSMTransition other)
        {
            return other.priority - priority;
        }
    }

    //����FSM��ʵ�֣��ǻ���Transit�����̡�ÿ����ͬ��FSMֻ��Ҫ��һ��ʼע�᲻ͬ��Transit����ô����Ϊ��ͬ��FSM��
    //��ͬ��FSM����ͬ״ִ̬�е��߼����뿪�߼�����ͬ�ġ�
    //ÿ��״̬�����н����߼���ת���߼���������Ž���ת���߼����жϡ�
    //����ԭ��StartingStatus_TargetStatus_Priority
    //DONE
    //DONE
    public class PreInit_Idle_0 : RootFSMTransition
    {
        public override int priority => 0;
        public override RootFSMStatus StartingStatus => RootFSMStatus.PreInit;

        public override bool AdditionalReq()
        {
            return (greatOwner.ReadyToGo) && (!greatOwner.PendingCleanUp);
        }

        public override void Consequence()
        {
            if (!greatOwner.Playing) greatOwner.Playing = true;
            owner.currentStatus = RootFSMStatus.Idle;
        }
    }
    //DONE
    public class Idle_Idle_0 : RootFSMTransition
    {
        public override int priority => 0;
        public override RootFSMStatus StartingStatus => RootFSMStatus.Idle;

        public override bool AdditionalReq()
        {
            return true;
        }

        public override void Consequence()
        {
            owner.currentStatus = RootFSMStatus.Idle;
        }
    }
    //DONE
    public class Idle_Upkeep_1 : RootFSMTransition
    {
        public override int priority => 1;
        public override RootFSMStatus StartingStatus => RootFSMStatus.Idle;

        public override bool AdditionalReq()
        {
            return !greatOwner.CtrlPack.IsFlag(ControllingCommand.Nop);
        }

        public override void Consequence()
        {
            owner.currentStatus = RootFSMStatus.Upkeep;
        }
    }
    //DONE
    public class Idle_BossInit_2 : RootFSMTransition
    {
        public override int priority => 2;
        public override RootFSMStatus StartingStatus => RootFSMStatus.Idle;

        public override bool AdditionalReq()
        {
            var roundGist = greatOwner.LevelAsset.ActionAsset.GetRoundGistByStep(greatOwner.LevelAsset.StepCount);
            var stage = roundGist?.Type ?? StageType.Shop;
            return (stage == StageType.Boss) && (!WorldCycler.BossStage);
        }

        public override void Consequence()
        {
            owner.currentStatus = RootFSMStatus.BossInit;
        }
    }
    //DONE
    public class Idle_Boss_1 : RootFSMTransition
    {
        public override int priority => 1;
        public override RootFSMStatus StartingStatus => RootFSMStatus.Idle;

        public override bool AdditionalReq()
        {
            var roundGist = greatOwner.LevelAsset.ActionAsset.GetRoundGistByStep(greatOwner.LevelAsset.StepCount);
            var stage = roundGist?.Type ?? StageType.Shop;
            return (stage == StageType.Boss);
        }

        public override void Consequence()
        {
            owner.currentStatus = RootFSMStatus.Boss;
        }
    }

    public class Idle_Animate_0 : RootFSMTransition
    {
        public override int priority => 0;
        public override RootFSMStatus StartingStatus => RootFSMStatus.Idle;

        public override bool AdditionalReq()
        {
            return (greatOwner.ReadyToGo) && (!greatOwner.PendingCleanUp);
        }

        public override void Consequence()
        {
            if (!greatOwner.Playing) greatOwner.Playing = true;
            owner.currentStatus = RootFSMStatus.Idle;
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

    public class Boss_Animate_0 : RootFSMTransition
    {
        public override int priority => 0;
        public override RootFSMStatus StartingStatus => RootFSMStatus.PreInit;

        public override bool AdditionalReq()
        {
            return (greatOwner.ReadyToGo) && (!greatOwner.PendingCleanUp);
        }

        public override void Consequence()
        {
            if (!greatOwner.Playing) greatOwner.Playing = true;
            owner.currentStatus = RootFSMStatus.Idle;
        }
    }
    //DONE
    public class Cycle_Animate_1 : RootFSMTransition
    {
        public override int priority => 1;
        public override RootFSMStatus StartingStatus => RootFSMStatus.Cycle;

        public override bool AdditionalReq()
        {
            return greatOwner.Animating;
        }

        public override void Consequence()
        {
            owner.currentStatus = RootFSMStatus.Animate;
        }
    }
    //DONE
    public class Animate_Clean_1 : RootFSMTransition
    {
        public override int priority => 1;
        public override RootFSMStatus StartingStatus => RootFSMStatus.Animate;

        public override bool AdditionalReq()
        {
            return !greatOwner.Animating;
        }

        public override void Consequence()
        {
            owner.currentStatus = RootFSMStatus.CleanUp;
        }
    }
    //DONE
    public class Animate_Animate_0 : RootFSMTransition
    {
        public override int priority => 0;
        public override RootFSMStatus StartingStatus => RootFSMStatus.Animate;

        public override bool AdditionalReq()
        {
            return greatOwner.Animating;
        }

        public override void Consequence()
        {
            owner.currentStatus = RootFSMStatus.Animate;
        }
    }
    //DONE
    public class Upkeep_Cycle_0 : RootFSMTransition
    {
        public override int priority => 0;
        public override RootFSMStatus StartingStatus => RootFSMStatus.Upkeep;

        public override bool AdditionalReq()
        {
            return true;
        }

        public override void Consequence()
        {
            owner.currentStatus = RootFSMStatus.Cycle;
        }
    }
    //DONE
    public class Cycle_Clean_0 : RootFSMTransition
    {
        public override int priority => 0;
        public override RootFSMStatus StartingStatus => RootFSMStatus.Cycle;

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
    public class Clean_Idle_0 : RootFSMTransition
    {
        public override int priority => 0;
        public override RootFSMStatus StartingStatus => RootFSMStatus.CleanUp;

        public override bool AdditionalReq()
        {
            return true;
        }

        public override void Consequence()
        {
            owner.currentStatus = RootFSMStatus.Idle;
        }
    }
}