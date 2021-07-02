using System.Linq;
using com.ootii.Messages;
using ROOT.Message;
using ROOT.Message.Inquiry;
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
            if (_timer >= _lifeSpan)
            {
                Destroy(gameObject);
            }
            else if (_timer >= _delay)
            {
                if (GameAsset.CollectorZone.Count!=0)
                {
                    foreach (var gird in GameAsset.CollectorZone.Select(vec => GameAsset.GameBoard.BoardGirdDriver.BoardGirds[vec]))
                    {
                        if (WithInUnitExtend(gird, transform.position))
                        {
                            GameAsset.SignalInfo++;
                            var signalInfo = new BoardSignalUpdatedInfo {SignalData = new BoardSignalUpdatedData() {InfoCounter = GameAsset.SignalInfo},};
                            MessageDispatcher.SendMessage(signalInfo);
                            GameAsset.GameBoard.SomeGridHasCollectedInfo(gird);
                            Destroy(gameObject);
                        }
                    }
                }
            }
        }
    }
}