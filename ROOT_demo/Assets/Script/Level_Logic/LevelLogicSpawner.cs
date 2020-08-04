using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ROOT
{
    public class LevelLogicSpawner : MonoBehaviour
    {
        IEnumerator KillNextFrame()
        {
            yield return 0;
            Destroy(transform.gameObject);
        }

        public LevelLogic SpawnLevelLogic<T>() where T : LevelLogic
        {
            var o = new GameObject("LevelLogic");
            var gameMgr = o.AddComponent<T>(); //加哪个Level就是玩哪关。
            StartCoroutine(KillNextFrame());
            return gameMgr;
        }

        public LevelLogic SpawnLevelLogic(GameObject levelLogicPrefab)
        {
            var go=Instantiate(levelLogicPrefab);
            var gameMgr = go.GetComponentInChildren<LevelLogic>();
            StartCoroutine(KillNextFrame());
            return gameMgr;
        }
    }
}