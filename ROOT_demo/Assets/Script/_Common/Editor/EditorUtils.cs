using System;
using System.Linq;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace ROOT
{
    public static class EditorUtils
    {
        private static void TryReserializePrefab(String _prefabPath)
        {
            var _prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(_prefabPath);
            TryReserializePrefab(_prefabAsset);
        }

        private static void TryReserializePrefab(GameObject _prefabAsset)
        {
            if (PrefabUtility.IsPartOfImmutablePrefab(_prefabAsset)) return;
            PrefabUtility.SavePrefabAsset(_prefabAsset);
        }

        [MenuItem("Tools/Reserialize Project prefabs")]
        private static void onClick_ReserializeProjectPrefabs()
        {
            GetProjectPrefabs().ForEach(TryReserializePrefab);
        }

        [MenuItem("Tools/Reserialize ALL prefabs(include plugins)")]
        private static void onClick_ReserializeAllPrefabs()
        {
            GetAllPrefabs().ForEach(TryReserializePrefab);
        }
        
        public static string[] GetProjectPrefabs()
        {
            return AssetDatabase.GetAllAssetPaths()
                .Where(str => str.Contains(".prefab"))
                .Where(str => str.Contains("Resources")).ToArray();
        }

        public static string[] GetAllPrefabs()
        {
            return AssetDatabase.GetAllAssetPaths()
                .Where(str => str.Contains(".prefab"))
                .ToArray();
        }
    }
}