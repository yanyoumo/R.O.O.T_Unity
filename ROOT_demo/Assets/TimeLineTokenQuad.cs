using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;

namespace ROOT
{
    public class TimeLineTokenQuad : MonoBehaviour
    {
        [ReadOnly]
        public TimeLine owner;
        [ReadOnly]
        public TimeLineToken[] Token;
        [ReadOnly]
        public int MarkerID;

        public GameObject QuadTemplate;

        private readonly float baseTokenHeight = 0.38f;
        public Transform QuadTransform;
        public TextMeshPro valMarkerNormal;
        public TextMeshPro valMarkerNetwork;
        float yOffset = 0.01f;

        public Color QuadColor
        {
            set => GetComponentInChildren<MeshRenderer>().material.color = value;
        }

        private Color[] QuadColors;

        public void DisableValMarker()
        {
            valMarkerNormal.enabled = false;
            valMarkerNetwork.enabled = false;
        }

        public void SetValMarker(int val, TimeLineTokenType type)
        {
            if (type == TimeLineTokenType.RequireNormal)
            {
                valMarkerNormal.enabled = true;
                valMarkerNormal.text = val.ToString();
            }
            else if (type == TimeLineTokenType.RequireNetwork)
            {
                valMarkerNetwork.enabled = true;
                valMarkerNetwork.text = val.ToString();
            }
        }

        public void InitQuadShape(float unitLength, int subDivision, TimeLineToken[] token)
        {
            Token = token;
            var max = token.Length;
            token.Sort();
            if (token.Any(tok=>tok.type == TimeLineTokenType.Ending))
            {
                token = token.Where(tok => tok.type == TimeLineTokenType.Ending).ToArray();
            }
            for (var i = 0; i < Token.Length; i++)
            {
                var go = Instantiate(QuadTemplate, QuadTransform);
                var tokenHeightUnit = baseTokenHeight / max;
                var tokenHeight = tokenHeightUnit * (max - i);
                go.GetComponent<MeshRenderer>().material.color = QuadColors[(int) token[i].type];
                transform.localPosition = Vector3.zero;
                transform.localScale = Vector3.one;
                go.transform.localPosition = new Vector3((unitLength / subDivision) * 0.5f, yOffset * i, tokenHeight * 0.5f);
                go.transform.localScale = new Vector3(unitLength / subDivision, tokenHeight, 1.0f);
            }
        }

        public void Awake()
        {
            QuadColors = new[]
            {
                ColorUtilityWrapper.ParseHtmlStringNotNull(ColorName.ROOT_TIMELINE_GENERAL),
                ColorUtilityWrapper.ParseHtmlStringNotNull(ColorName.ROOT_TIMELINE_NETWORK),
                ColorUtilityWrapper.ParseHtmlStringNotNull(ColorName.ROOT_TIMELINE_DISASTER),
                ColorUtilityWrapper.ParseHtmlStringNotNull(ColorName.ROOT_TIMELINE_ENDING),
            };
        }

        private void SetSingleToken(TimeLineToken _token)
        {
            if (_token.type == TimeLineTokenType.RequireNormal || _token.type == TimeLineTokenType.RequireNetwork)
            {
                if (MarkerID == _token.Range.x)
                {
                    if (_token.Range.x >= owner.StepCount)
                    {
                        SetValMarker(_token.RequireAmount, _token.type);
                    }
                }
                else if (MarkerID <= _token.Range.y)
                {
                    if (MarkerID == owner.StepCount)
                    {
                        SetValMarker(_token.RequireAmount, _token.type);
                    }
                }
                else if (_token.Range.y == -1)
                {
                    if (MarkerID == owner.StepCount)
                    {
                        SetValMarker(_token.RequireAmount, _token.type);
                    }
                }
            }
        }

        public void Update()
        {
            DisableValMarker();
            Token.ForEach(SetSingleToken);
        }
    }
}