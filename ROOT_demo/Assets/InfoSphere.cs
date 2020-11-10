using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using UnityEngine;

namespace ROOT
{
    public class InfoSphere : MonoBehaviour
    {
        public GameAssets GameAsset;

        private bool _activated = false;
        private float _lifeSpan = 0.0f;
        private float _delay = 0.0f;
        private float _timer = 0.0f;

        public void Activate(float lifeSpan,float delay, GameAssets gameAsset)
        {
            _lifeSpan = lifeSpan;
            _delay = delay;
            _activated = true;
            GameAsset = gameAsset;
        }

        private float GirdExtend = 0.65f;//HACK 这个值是人肉测出来的MagicNumber别瞎改。

        private bool WithInExtend(Vector2 center, Vector2 other)
        {
            var disp = other - center;
            return (Mathf.Abs(disp.x) <= GirdExtend) && (Mathf.Abs(disp.y) <= GirdExtend);
        }

        private bool WithInUnitExtend(BoardGirdCell unit,Vector3 otherVec3)
        {
            var center = new Vector2
            {
                x = unit.transform.position.x,
                y = unit.transform.position.z
            };
            var other = new Vector2
            {
                x = otherVec3.x,
                y = otherVec3.z
            };
            return WithInExtend(center, other);
        }

        public void Update()
        {
            if (!_activated) return;

            _timer += Time.deltaTime;
            if (_timer>=_delay)
            {
                foreach (var gird in GameAsset.CollectorZone.Select(vec => GameAsset.GameBoard.BoardGirds[vec]))
                {
                    if (WithInUnitExtend(gird, transform.position))
                    {
                        GameAsset.SignalInfo++;
                        GameAsset.SignalPanel.SignalCounter = GameAsset.SignalInfo;
                        GameAsset.GameBoard.SomeGridHasCollectedInfo(gird);
                        Destroy(gameObject);
                    }
                }
            }
            else if (_timer>=_lifeSpan)
            {
                Destroy(gameObject);
            }
        }
    }
}