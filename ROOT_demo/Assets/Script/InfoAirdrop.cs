using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ROOT
{
    /// <summary>
    /// 独立出来有一个好处，就是那里写的不好的可以重新写。
    /// 但是相当于MeteoriteBomber的配置都得重新弄。
    /// </summary>
    public class InfoAirdrop : MonoBehaviour
    {
        public Transform LFLocator;
        public Transform URLocator;
        public GameObject InfoSphere;
        public GameAssets GameAsset;

        private void SpraySingleInfo()
        {
            var randX = Random.Range(LFLocator.position.x, URLocator.position.x);
            var randZ = Random.Range(LFLocator.position.z, URLocator.position.z);
            var randPos = new Vector3(randX, 0.0f, randZ);
            var go=Instantiate(InfoSphere, randPos, Quaternion.identity, transform);
            go.GetComponent<InfoSphere>().Activate(3.0f, 0.25f, GameAsset);
        }

        public void SprayInfo(int count)
        {
            for (int i = 0; i < count; i++)
            {
                SpraySingleInfo();
            }
        }
    }
}