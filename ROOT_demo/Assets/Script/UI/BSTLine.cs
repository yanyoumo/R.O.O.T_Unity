using System;
using UnityEngine;

namespace ROOT.UI
{
    public class BSTLine : MonoBehaviour
    {
        public RectTransform A;
        public RectTransform B;
        public RectTransform Line;

        public void UpdateLine()
        {
            Line.anchoredPosition = (A.anchoredPosition + B.anchoredPosition) * 0.5f;
            var length = Vector2.Distance(A.anchoredPosition, B.anchoredPosition);
            length = Mathf.Max(0.01f, length);
            Line.localScale = new Vector3(length * 0.01f, 0.3f, 1.0f);
            var sin = (B.anchoredPosition.y - A.anchoredPosition.y) / length;
            var cos = (B.anchoredPosition.x - A.anchoredPosition.x) / length;
            var angle = Mathf.Atan2(sin, cos) * Mathf.Rad2Deg;
            if (angle<0)
            {
                angle += 360;
            }
            Line.rotation = Quaternion.Euler(new Vector3(0.0f, 0.0f, angle));
            Destroy(A.gameObject);
            Destroy(B.gameObject);
        }
    }
}