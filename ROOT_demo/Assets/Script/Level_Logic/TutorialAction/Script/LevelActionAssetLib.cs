﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace ROOT
{
    [CreateAssetMenu(fileName = "NewActionAssetLib")]
    public class LevelActionAssetLib : ScriptableObject
    {
        [ShowInInspector]
        public LevelActionAsset[] ActionAssetList;
    }
}