using System.Collections;
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
        
        public FSMLevelLogic SpawnLevelLogic(GameObject levelLogicPrefab)
        {
            var go=Instantiate(levelLogicPrefab);
            var gameMgr = go.GetComponentInChildren<FSMLevelLogic>();
            StartCoroutine(KillNextFrame());
            
            return gameMgr;
        }
    }
}