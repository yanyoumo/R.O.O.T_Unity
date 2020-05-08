using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ROOT
{
    public class LibItem : MonoBehaviour
    {
        public String name;
        public String content;
        public Texture2D Icon;
        public Color nameColor = Color.white;

        public MeshRenderer IconQuad;
        public TextMeshPro Name;
        public TextMeshPro Content;

        string TmpColorXml(string content, Color col)
        {
            var hexCol = ColorUtility.ToHtmlStringRGB(col);
            return "<color=#" + hexCol + "> " + content + " </color> ";
        }

        void Awake()
        {
            IconQuad.material.SetTexture("_MainTex", Icon);
            Name.text = TmpColorXml(name, nameColor);
            Content.text = content;
        }
    }
}