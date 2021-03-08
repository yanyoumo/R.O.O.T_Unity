using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ROOT.SetupAsset
{
    public partial class LevelLib : MonoBehaviour
    {
        private static LevelLib _instance;
        public static LevelLib Instance => _instance;

        public LevelActionAsset[] TutorialActionAssetList => TutorialLevelActionAssetLib.ActionAssetList;
        public LevelActionAsset[] CareerActionAssetList => CareerLevelActionAssetLib.ActionAssetList;
        public LevelActionAsset[] TestingActionAssetList => TestingLevelActionAssetLib.ActionAssetList;

        public LevelActionAsset ActionAsset(int i)
        {
            if (i<TutorialActionAssetList.Length)
            {
                return TutorialActionAssetList[i];
            }
            i -= TutorialActionAssetList.Length;
            if (i<CareerActionAssetList.Length)
            {
                return CareerActionAssetList[i];
            }
            i -= CareerActionAssetList.Length;
            return TestingActionAssetList[i];
        }
        
        private LevelActionAsset GetNextActionAsset(LevelActionAsset[] lib, in LevelActionAsset asset)
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
            DontDestroyOnLoad(this);
        }
    }
}