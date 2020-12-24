using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ROOT
{
    public enum SignalType
    {
        Matrix,
        Scan,
    }

    public abstract class SignalAssetBase : MonoBehaviour
    {
        public Type UnitSignalCoreType { protected set; get; }
        public abstract SignalType Type { get; }
        public abstract bool ShowSignal(Unit unit,Unit otherUnit);
        public abstract int SignalVal(Unit unit,Unit otherUnit);
    }
}