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
        public LevelActionAsset[] ActionAssetList => LevelActionAssetLib.ActionAssetList;
        public int TutorialActionAssetCount => LevelActionAssetLib.ActionAssetList.Length;

        public LevelActionAsset GetNextTutorialActionAsset(in LevelActionAsset asset)
        {
            for (var i = 0; i < ActionAssetList.Length; i++)
            {
                if (ActionAssetList[i].Equals(asset))
                {
                    if (i + 1 < TutorialActionAssetCount)
                    {
                        return ActionAssetList[i + 1];
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