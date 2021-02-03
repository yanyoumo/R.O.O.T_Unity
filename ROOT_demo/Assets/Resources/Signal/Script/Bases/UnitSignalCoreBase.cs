using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ROOT.Signal
{
    public abstract class UnitSignalCoreBase : MonoBehaviour
    {
        public Unit Owner;
        protected Board GameBoard => Owner.GameBoard;
        //返回对应的信号的enum
        public abstract SignalType Type { get; }

        public Dictionary<SignalType, int> SignalStrength;
        //主要通过以下这个数据结构将整个棋盘中的数据网络记录下来。
        public Dictionary<SignalType, (int, int)> SignalStrength2;//Signal:信号 第一个int：物理的深度 第二个int：只计算对应型号场单元的深度。

        public void GetBoardSignalStrength(Board board)
        {
            //TODO
            foreach (var signalType in SignalMasterMgr.Instance.SignalLib)
            {
                //init all unit()

            }
            throw new NotImplementedException();
        }
 
        //为返回对应的在 遥测阶段 中获得的遥测范围。
        public abstract List<Vector2Int> SingleInfoCollectorZone { get; }

        [ReadOnly] public RotationDirection SignalFromDir;
        [ReadOnly] public bool Visited;//dequeue
        [ReadOnly] public bool Visiting;//enqueue
        [ReadOnly] public bool InMatrix;
        [ReadOnly] public bool InMatrixSignal;

        public bool IsActiveMatrixFieldUnit => InMatrix && (Owner.UnitSignal == SignalType.Matrix && Owner.UnitHardware == HardwareType.Field);
        public bool IsEndingScanFieldUnit => InServerGrid && (Owner.UnitSignal == SignalType.Scan && Owner.UnitHardware == HardwareType.Field) && ScanSignalPathDepth == 1;
        

        //标记扫描信号的路径的参数。
        [ReadOnly] public int ScanSignalPathDepth; //for scoring purpose
        
        [ReadOnly] 
        [ShowInInspector]
        public int ServerSignalDepth
        {
            get => SignalStrength[SignalType.Scan];
            set => SignalStrength[SignalType.Scan] = value;
        }
        /// <summary>
        /// 标记一次计分后，本单元是否处于必要最长序列中。不处于的需要显式记为false。
        /// </summary>
        [ReadOnly] 
        [ShowInInspector]
        public bool InServerGrid; //for scoring purpose

        void Awake()
        {
            Visited = false;
            InServerGrid = false;
            InMatrix = false;
            SignalStrength = new Dictionary<SignalType, int>();
            try
            {
                foreach (var signalType in SignalMasterMgr.Instance.SignalLib)
                {
                    SignalStrength.Add(signalType, 0);
                }
            }
            catch (NullReferenceException)
            {
                Debug.Log("This is template Core, no need to set EnteringSignalData.");
            }
        }

        //0:no signal.
        //1:has signal but no active.
        //2:signal and active.
        public abstract int GetActivationStatus { get; }
        
        public float CalScore()
        {
            return CalScore(out var a);
        }
        
        //从某个独立单元计分的逻辑、主要是为SignalAsset中总体计算的BackUp函数。 
        public abstract float CalScore(out int hardwareCount);
    }
}