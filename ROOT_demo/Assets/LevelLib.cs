using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ROOT
{
    public class LevelLib : MonoBehaviour
    {
        //public TutorialActionAssetLib ActionAssetLib;

        private static LevelLib _instance;
        public static LevelLib Instance { get { return _instance; } }

        public TutorialActionAssetLib TutorialActionAssetLib;
        public TutorialActionAsset[] TutorialActionAssetList => TutorialActionAssetLib.TutorialActionAssetList;
        public int TutorialActionAssetCount => TutorialActionAssetLib.TutorialActionAssetList.Length;

        public TutorialActionAsset GetNextTutorialActionAsset(in TutorialActionAsset asset)
        {
            for (var i = 0; i < TutorialActionAssetList.Length; i++)
            {
                if (TutorialActionAssetList[i].Equals(asset))
                {
                    if (i + 1 < TutorialActionAssetCount)
                    {
                        return TutorialActionAssetList[i + 1];
                    }

                    return null;
                }
            }
            throw new ArgumentOutOfRangeException();
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                _instance = this;
            }
        }

        public void LockedLibin()
        {
            Object.DontDestroyOnLoad(this);
        }
    }
}