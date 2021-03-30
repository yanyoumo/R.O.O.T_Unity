using ROOT.SetupAsset;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ROOT
{
    [CreateAssetMenu(fileName = "NewActionAssetLib", menuName = "ActionAsset/New ActionAssetLib")]
    public class LevelActionAssetLib : ScriptableObject
    {
        [ShowInInspector]
        public LevelActionAsset[] ActionAssetList;
    }
}