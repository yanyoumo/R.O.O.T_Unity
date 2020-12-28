using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ROOT
{
    public class ScanUnitSignalCore : UnitSignalCoreBase
    {
        public override float CalScore(out int networkCount)
        {
            Debug.LogWarning("ScanUnitSignalCore_CalScore");
            networkCount = 0;
            return -1.0f;
            //throw new System.NotImplementedException("ScanUnitSignalCore_CalScore");
        }
    }
}