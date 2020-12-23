using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ROOT
{
    [CreateAssetMenu(fileName = "NewUnitAssetLib", menuName = "Unit/New UnitAssetLib")]
    [Serializable]
    public class UnitAssetLib : ScriptableObject
    {
        public GameObject[] Units;
    }
}