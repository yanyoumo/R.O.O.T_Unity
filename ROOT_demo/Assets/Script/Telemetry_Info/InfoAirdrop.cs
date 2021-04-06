using System;
using UnityEngine;
using Random = UnityEngine.Random;

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

        private const float sphereLifeSpanMedian = 3.5f;
        private const float sphereLifeSpanDeviate = 1.5f;
        private const float sphereSpawnDelay = 0.25f;

        private Vector3 PureRandomPos()
        {
            //这个玩意儿是纯随机的，有没有可能要像Raytracing那样做均摊随机？。
            var randX = Random.Range(LFLocator.position.x, URLocator.position.x);
            var randZ = Random.Range(LFLocator.position.z, URLocator.position.z);
            return new Vector3(randX, 0.0f, randZ);
        }

        private Vector3 UniformRandomPos()
        {
            //TODO 这个估计还是需要实现、本质上就是先生成在某个预定义格点上。
            //然后再在这个格点上做一个方向、长度不同的随机偏移。
            throw new NotImplementedException();
        }

        private void SpraySingleInfo()
        {
            var randPos = PureRandomPos();
            var go=Instantiate(InfoSphere, randPos, Quaternion.identity, transform);
            var sphereLifeSpan = sphereLifeSpanDeviate + Random.Range(-1.0f * sphereSpawnDelay, sphereSpawnDelay);
            sphereLifeSpan = Mathf.Max(sphereLifeSpan, 0.0f);
            go.GetComponent<InfoSphere>().Activate(sphereLifeSpan, sphereSpawnDelay, GameAsset);
        }

        public void SprayInfo(int count)
        {
            for (var i = 0; i < count; i++)
            {
                SpraySingleInfo();
            }
        }
    }
}