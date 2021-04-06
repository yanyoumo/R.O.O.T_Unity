using System;
using TMPro;
using UnityEngine;

namespace ROOT.UI
{
    public class LibItem : MonoBehaviour
    {
        public String itemname;
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
            Name.text = TmpColorXml(itemname, nameColor);
            Content.text = content;
        }
    }
}