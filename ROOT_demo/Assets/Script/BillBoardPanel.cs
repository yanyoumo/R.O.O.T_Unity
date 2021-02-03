using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ROOT
{
    public class BillBoardPanel : MonoBehaviour
    {
        void Update()
        {
            if (Camera.main != null)
            {
                transform.rotation = Camera.main.transform.rotation;
            }
        }
    }
}