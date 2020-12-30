using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ROOT
{
    public abstract class SignalAssetBase : MonoBehaviour
    {
        public Type UnitSignalCoreType { protected set; get; }
        public abstract SignalType Type { get; }
        public abstract CoreType CoreUnitType { get; }
        public abstract CoreType FieldUnitType { get; }
        public abstract bool ShowSignal(RotationDirection dir,Unit unit,Unit otherUnit);
        public abstract int SignalVal(RotationDirection dir, Unit unit,Unit otherUnit);
        
        //�����￴�����ǽ�����һ���ط����Զ�ĳ���ź�ͳһ���ý����
        //���������൱��Ҫ���ӽǷ��ڵ�Ԫ�ϣ�
        //Ȼ���������ͳһ�������������Ѿ����¶���������ҪBoard�����ã��������ڰɡ�
        //Ŷ�ԣ����ﻹ��Ҫһ��Unit��SignalType��ӳ�䡣
        public virtual float CalAllScore(Board gameBoard, out int hardwareCount)
        {
            float res=0;
            int reshdCount = 0;
            foreach (var signalCore in gameBoard.Units.Where(unit => unit.UnitCore == CoreUnitType).Select(unit=>unit.SignalCore))
            {
                res += signalCore.CalScore(out var count);
                reshdCount += count;
            }
            hardwareCount = reshdCount;
            return res;
        }

        public float CalAllScore(Board gameBoard)
        {
            return CalAllScore(gameBoard, out var A);
        }
    }
}