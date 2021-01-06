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
    public class PreInit_UpKeep_0 : RootFSMTransition
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
    public class UpKeep_Boss_5 : RootFSMTransition
    {
        public override int priority => 5;
        public override RootFSMStatus StartingStatus => RootFSMStatus.UpKeep;

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
            return !greatOwner.CtrlPack.IsFlag(ControllingCommand.Nop);
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
            return greatOwner.Animating;
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
            return greatOwner.Animating;
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
            return !greatOwner.Animating;
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