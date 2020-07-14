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

        public BaseLevelMgr SpawnLevelLogic<T>() where T : BaseLevelMgr
        {
            var o = new GameObject("LevelLogic");
            var gameMgr = o.AddComponent<T>(); //加哪个Level就是玩哪关。
            StartCoroutine(KillNextFrame());
            return gameMgr;
        }
    }
}