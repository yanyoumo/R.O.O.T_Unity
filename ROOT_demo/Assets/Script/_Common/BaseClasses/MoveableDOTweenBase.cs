using System.Collections;
using System.Collections.Generic;
using ROOT.Common;
using UnityEngine;
using CommandDir = ROOT.RotationDirection;


namespace ROOT
{
    public abstract class MoveableDOTweenBase : MonoBehaviour
    {
        protected bool _immovable = false;
        public abstract bool Immovable { set; get; }
        //public int UUID;//还需要类是这样的UUID。
        public bool MoveDesired;
        public bool RotateDesired;
        public Vector2Int CurrentBoardPosition { get;protected set; }
        public Vector2Int DesiredBoardPosition { get; protected set; }
        public RotationDirection CurrentRotationDirection { get;protected set; }
        public RotationDirection DesiredRotationDirection { get; protected set; }
    }
}