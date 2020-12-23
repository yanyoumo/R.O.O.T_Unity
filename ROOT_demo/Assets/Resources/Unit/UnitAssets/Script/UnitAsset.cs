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
        public UnitRules Rules;//日了，这个东西只能通过Mono实例/Prefab指定；不好整。
    }

    [Serializable]
    public abstract class UnitRules : MonoBehaviour
    {
        public abstract int CalScore();
    }
}