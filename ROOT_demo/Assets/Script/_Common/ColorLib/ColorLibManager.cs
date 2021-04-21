using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace ROOT.SetupAsset
{
    public class ColorLibManager : MonoBehaviour
    {
        [NotNull] private static ColorLibManager _instance;
        public static ColorLibManager Instance => _instance;
        
        public ColorLibAsset ColorLib;
        void Awake()
        {
            DontDestroyOnLoad(gameObject);
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                _instance = this;
            }
        }
    }
}