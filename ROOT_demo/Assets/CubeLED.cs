using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ROOT
{
    public class CubeLED : MonoBehaviour
    {
        public TextMeshPro LumpedText;
        public MeshRenderer LED;

        public bool On
        {
            set => LED.material.color = value ? Color.green : Color.grey;
        }

        public void InitLED()
        {
            LumpedText.gameObject.SetActive(false);
        }
    }
}