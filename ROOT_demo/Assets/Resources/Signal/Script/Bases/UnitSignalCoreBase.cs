using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ROOT
{
    public abstract class UnitSignalCoreBase : MonoBehaviour
    {
        public Unit Owner;
        protected Board GameBoard => Owner.GameBoard;
        public abstract SignalType Type { get; }
        public abstract List<Vector2Int> SingleInfoCollectorZone { get; }

        [ReadOnly] public RotationDirection SignalFromDir;
        [ReadOnly] public bool Visited;//dequeue
        [ReadOnly] public bool Visiting;//enqueue
        [ReadOnly] public int MatrixVal;
        [ReadOnly] public bool InMatrix;
        [ReadOnly] public bool InMatrixSignal;

        /// <summary>
        /// 记录服务器硬件深度的变量。可以作为中间量、即使不处于最长序列该值不必清除。
        /// 这个变量为扫描信号的硬件深度；末端必然为1；并且非扫描信号仍然增加这个值。
        /// 即——这个值从末端到扫描核心单元应该是单增的。
        /// </summary>
        [ReadOnly] public int ServerDepth; //for scoring purpose

        /// <summary>
        /// 记录服务器信号深度的变量。可以作为中间量、即使不处于最长序列该值不必清除。
        /// 这个变量为扫描信号的信号深度；非扫描信号不增加这个值。
        /// </summary>
        [ReadOnly] public int ServerSignalDepth; //for scoring purpose

        /// <summary>
        /// 标记一次计分后，本单元是否处于必要最长序列中。不处于的需要显式记为false。
        /// </summary>
        [ReadOnly] public bool InServerGrid; //for scoring purpose

        void Awake()
        {
            Visited = false;
            InServerGrid = false;
            InMatrix = false;
        }

        public float CalScore()
        {
            return CalScore(out var a);
        }
        public abstract float CalScore(out int hardwareCount);
    }
}