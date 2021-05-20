using System;
using ROOT.Common;
using UnityEngine;
using CommandDir = ROOT.RotationDirection;

namespace ROOT
{
    public abstract class MoveableBase : MonoBehaviour
    {
        protected bool _immovable = false;
        public abstract bool Immovable { set; get; }
        public abstract Transform AnimatingRoot { get; }
        public Vector2Int CurrentBoardPosition { get;protected set; }
        public Vector2Int NextBoardPosition { get; protected set; }

        public RotationDirection CurrentRotationDirection { get; protected set; } = RotationDirection.North;
        public RotationDirection NextRotationDirection { get; protected set; } = RotationDirection.North;
        public abstract void UpdateTransform(Vector3 pos);
        public abstract void PingPongRotationDirection();

        #region MoveToNeigbour

        public Vector2Int GetNeigbourCoord(CommandDir rotation) => CurrentBoardPosition + Utils.ConvertDirectionToBoardPosOffset(rotation);
        public void Move(CommandDir dir) => NextBoardPosition = GetNeigbourCoord(dir);
        public void MoveTo(Vector2Int pos) => NextBoardPosition = pos;
        
        #endregion

        public void SetCurrentAndNextPos(Vector2 pos)
        {
            CurrentBoardPosition = Utils._V2ToV2Int(pos);
            NextBoardPosition = Utils._V2ToV2Int(pos);
        }
    }
}
