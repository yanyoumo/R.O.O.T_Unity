using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ROOT
{
    public abstract class UnitSignalCoreBase : MonoBehaviour
    {
        public Unit Owner;

        [ReadOnly] public RotationDirection SignalFromDir;
        [ReadOnly] public bool Visited;//dequeue
        [ReadOnly] public bool Visiting;//enqueue
        [ReadOnly] public int HardDiskVal;
        [ReadOnly] public bool InHddGrid;
        [ReadOnly] public bool InHddSignalGrid;

        /// <summary>
        /// 记录服务器信号深度的变量，和服务器相连的Network该数值应该为1.
        /// 可以作为中间量、即使不处于最长序列该值不必清除。
        /// </summary>
        [ReadOnly] public int ServerDepth; //for scoring purpose

        /// <summary>
        /// 标记一次计分后，本单元是否处于必要最长序列中。不处于的需要显式记为false。
        /// </summary>
        [ReadOnly] public bool InServerGrid; //for scoring purpose

        /// <summary>
        /// 具体显示LED的field，即，最接近服务器的该数值应为全部深度，最枝端的显示值需要为1。
        /// </summary>
        public int NetworkVal => ServerDepth;

        void Awake()
        {
            Visited = false;
            InServerGrid = false;
            InHddGrid = false;
        }

        public float CalScore()
        {
            return CalScore(out var a);
        }
        public abstract float CalScore(out int hardwareCount);
    }
}