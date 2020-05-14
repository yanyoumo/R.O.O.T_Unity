using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ROOT
{
    //[ExecuteInEditMode]
    public class LCD : MonoBehaviour
    {
        public int digit = 0;
        public bool PosOrNeg = false;

        public MeshRenderer stroke_1;
        public MeshRenderer stroke_2;
        public MeshRenderer stroke_3;
        public MeshRenderer stroke_4;
        public MeshRenderer stroke_5;
        public MeshRenderer stroke_6;
        public MeshRenderer stroke_7;

        private Color PosColor = Color.green;
        private Color OffColor = Color.black;
        private Color NegColor = Color.red;
        private MeshRenderer[] _strokes;
        private int[][] _digitLib;
        private int[] nanDigit;

        public int Digit
        {
            set
            {
                digit = value % 10;
                Color targetColor = PosOrNeg ? PosColor : NegColor;
                foreach (var meshRenderer in _strokes)
                {
                    meshRenderer.material.SetColor("_EmissionColor", OffColor);
                }

                if (value >= 0)
                {
                    for (var i = 0; i < _digitLib[digit].Length; i++)
                    {
                        _strokes[_digitLib[digit][i]].material.SetColor("_EmissionColor", targetColor);
                    }
                }
            }
            get => digit;
        }

        void Awake()
        {
            _strokes = new[] {stroke_1, stroke_2, stroke_3, stroke_4, stroke_5, stroke_6, stroke_7};
            _digitLib = new[]
            {
                new[] {0, 1, 2, 4, 5, 6}, new[] {0, 4}, new[] {1, 2, 3, 4, 5},
                new[] {0, 1, 3, 4, 5}, new[] {0, 3, 4, 6}, new[] {0, 1, 3, 5, 6}, new[] {0, 1, 2, 3, 5, 6},
                new[] {0, 1, 4},
                new[] {0, 1, 2, 3, 4, 5, 6}, new[] {0, 1, 3, 4, 5, 6}
            };
            nanDigit = new[]
            {
                3
            };
        }

        // Start is called before the first frame update
        void Start()
        {

        }

#if UNITY_EDITOR
        void OnEnable()
        {
            _strokes = new[] {stroke_1, stroke_2, stroke_3, stroke_4, stroke_5, stroke_6, stroke_7};
            _digitLib = new[]
            {
                new[] {0, 1, 2, 4, 5, 6}, new[] {0, 4}, new[] {1, 2, 3, 4, 5},
                new[] {0, 1, 3, 4, 5}, new[] {0, 3, 4, 6}, new[] {0, 1, 3, 5, 6}, new[] {0, 1, 2, 3, 5, 6},
                new[] {0, 1, 4},
                new[] {0, 1, 2, 3, 4, 5, 6}, new[] {0, 1, 3, 4, 5, 6}
            };
        }
#endif

        public void SetDigit(int dig)
        {
            //Digit = Mathf.Abs(dig);
            Digit = dig % 10;
            Color targetColor = PosOrNeg ? PosColor : NegColor;
            foreach (var meshRenderer in _strokes)
            {
                meshRenderer.material.SetColor("_EmissionColor", OffColor);
            }

            if (dig >= 0)
            {
                for (var i = 0; i < _digitLib[Digit].Length; i++)
                {
                    _strokes[_digitLib[Digit][i]].material.SetColor("_EmissionColor", targetColor);
                }
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}