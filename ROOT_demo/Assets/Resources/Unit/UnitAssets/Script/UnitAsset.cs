using System;
using UnityEngine;

namespace ROOT
{
    [CreateAssetMenu(fileName = "NewUnitAsset", menuName = "Unit/New UnitAsset")]
    [Serializable]
    public class UnitAsset : ScriptableObject
    {
        public string Name;
        public Texture CoreTexture;
        public Texture FieldTexture;
        public UnitRules Rules;//���ˣ��������ֻ��ͨ��Monoʵ��/Prefabָ������������
    }

    [Serializable]
    public abstract class UnitRules : MonoBehaviour
    {
        public abstract int CalScore();
    }
}