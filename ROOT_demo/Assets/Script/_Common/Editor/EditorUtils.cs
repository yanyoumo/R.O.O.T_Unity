using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ROOT
{
    public static class EditorUtils
    {
        [MenuItem("Tools/Reserialize all prefabs")]
        private static void onClick_ReserializeAllPrefabs()
        {
            foreach (string _prefabPath in GetAllPrefabs())
            {
                GameObject _prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(_prefabPath);
                if (!PrefabUtility.IsPartOfImmutablePrefab(_prefabAsset))
                {
                    PrefabUtility.SavePrefabAsset(_prefabAsset);
                }
            }
        }

        public static string[] GetAllPrefabs()
        {
            return AssetDatabase.GetAllAssetPaths()
                .Where(str => str.Contains(".prefab"))
                .ToArray();
        }
    }
}