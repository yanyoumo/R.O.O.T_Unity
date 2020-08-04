using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ROOT
{
    public class LevelLib : MonoBehaviour
    {
        private static LevelLib _instance;
        public static LevelLib Instance { get { return _instance; } }

        public LevelActionAssetLib LevelActionAssetLib;
        public LevelActionAsset[] TutorialActionAssetList => LevelActionAssetLib.TutorialActionAssetList;
        public int TutorialActionAssetCount => LevelActionAssetLib.TutorialActionAssetList.Length;

        public LevelActionAsset GetNextTutorialActionAsset(in LevelActionAsset asset)
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

        public void LockInLib()
        {
            Object.DontDestroyOnLoad(this);
        }
    }
}