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
        [ReadOnly] public int ServerDepth;
        [ReadOnly] public bool InServerGrid;

        public int CalScore()
        {
            return CalScore(out var a);
        }
        public abstract int CalScore(out int hardwareCount);
    }
}