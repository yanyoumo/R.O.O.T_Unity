using System;
using System.Collections;
using System.Collections.Generic;
using Doozy.Engine.UI;
using UnityEngine;

namespace ROOT
{
    public class UIViewInjector : MonoBehaviour
    {
        public UIView payload;

        private void Start()
        {
            payload.transform.parent = GameObject.Find("View - GamePlayUI").transform;
        }
    }
}

