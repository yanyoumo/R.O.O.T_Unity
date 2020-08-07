using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ROOT
{
    //[ExecuteInEditMode]
    public class TimeLineMarker : MonoBehaviour
    {
        public bool PendingKill = false;
        public Transform TimeLineMarkerRoot;

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