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
    //BareboneLoop: PreInit==>Init==>Idle==>Upkeep==>Major==>CleanUp==>Idle

    public class PreInit_Idle_0 : RootFSMTransition
    {
        public override int priority => 0;
        public override RootFSMStatus StartingStatus => RootFSMStatus.PreInit;

        public override bool AdditionalReq()
        {
            return true;
            //return (greatOwner.ReadyToGo) && (!greatOwner.PendingCleanUp);
        }

        public override void Consequence()
        {
            owner.currentStatus = RootFSMStatus.Idle;
        }
    }
    public class Idle_Upkeep_0 : RootFSMTransition
    {
        public override int priority => 0;
        public override RootFSMStatus StartingStatus => RootFSMStatus.Idle;

        public override bool AdditionalReq()
        {
            return true;
        }

        public override void Consequence()
        {
            owner.currentStatus = RootFSMStatus.Upkeep;
        }
    }
    public class Upkeep_Major_0 : RootFSMTransition
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
    public class Major_CleanUp_0 : RootFSMTransition
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
    public class CleanUp_Idle_0 : RootFSMTransition
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