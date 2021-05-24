using System;
using UnityEngine;

namespace BinarySearchTree
{
    public class BSTLevelLibNode : ScriptableObject
    {
        public int LevelIndex;//这个只配置引用可能是比较靠谱的
        public BSTLevelLibNode Direct;
        public BSTLevelLibNode Branch;
    }
    
    public class BSTLevelLibTree : ScriptableObject
    {
        
    }
}