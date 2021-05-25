using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class TestUI : MonoBehaviour
{
    public RectTransform TestTransform;

    [Button]
    public void Test()
    {
        //TestTransform.position = new Vector3(30, 40, 50);
        TestTransform.anchoredPosition = new Vector2(150, 150);
    }
}
