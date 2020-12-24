using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ROOT
{
    public abstract class UnitLogicCoreBase : MonoBehaviour
    {
        public int CalScore()
        {
            return CalScore(out var a);
        }
        public abstract int CalScore(out int hardwareCount);
    }
}