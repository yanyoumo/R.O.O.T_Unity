using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ROOT
{
    public partial class LevelLib : MonoBehaviour
    {
        private static LevelLib _instance;
        public static LevelLib Instance => _instance;

        [Obsolete]
        public LevelActionAsset[] TutorialActionAssetList => TutorialLevelActionAssetLib.ActionAssetList;
        [Obsolete]
        public int TutorialActionAssetCount => TutorialLevelActionAssetLib.ActionAssetList.Length;
        public int CareerActionAssetCount => CareerActionAssetList.Length;

        public LevelActionAsset GetNextActionAsset(LevelActionAsset[] lib, in LevelActionAsset asset)
        {
            for (var i = 0; i < lib.Length; i++)
            {
                if (lib[i].Equals(asset))
                {
                    if (i + 1 < lib.Length)
                    {
                        return lib[i + 1];
                    }

                    return null;
                }
            }
            throw new ArgumentOutOfRangeException();
        }

        public LevelActionAsset GetNextTutorialActionAsset(in LevelActionAsset asset)
        {
            return GetNextActionAsset(TutorialActionAssetList,in asset);
        }

        public LevelActionAsset GetNextCareerActionAsset(in LevelActionAsset asset)
        {
            return GetNextActionAsset(CareerActionAssetList, in asset);
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