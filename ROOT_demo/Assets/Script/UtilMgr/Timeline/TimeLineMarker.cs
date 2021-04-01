using Sirenix.OdinInspector;
using UnityEngine;

namespace ROOT
{
    public class TimeLineMarker : MonoBehaviour
    {
        [ReadOnly]
        public bool PendingKill = false;
        public bool UseMajorMark
        {
            set
            {
                MajorMark.gameObject.SetActive(value);
                MinorMark.gameObject.SetActive(!value);
            }
        }
        public Transform TimeLineMarkerRoot;

        public MeshRenderer MajorMark;
        public MeshRenderer MinorMark;

        void Update()
        {
            if (PendingKill)
            {
                Destroy(gameObject);
            }
        }

        void OnDestroy()
        {
            Destroy(gameObject);
        }
    }
}