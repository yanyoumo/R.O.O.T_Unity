using System;
using ROOT.Common;
using UnityEngine;
using CommandDir = ROOT.RotationDirection;

namespace ROOT
{
    public abstract class MoveableBase : MonoBehaviour//, IPlaceable
    {
        protected bool _immovable = false;
        public abstract bool Immovable { set; get; }
        public abstract Transform AnimatingRoot { get; }
        public Vector2Int CurrentBoardPosition { get;protected set; }
        public Vector2Int NextBoardPosition { get; protected set; }
        //public Vector2 LerpingBoardPosition { get; set; }

        public RotationDirection CurrentRotationDirection { get; protected set; } = RotationDirection.North;
        public RotationDirection NextRotationDirection { get; protected set; } = RotationDirection.North;
        //private float RotationDirectionLerper;
        public abstract void UpdateTransform(Vector3 pos);
        public abstract void PingPongRotationDirection();

        /*public Vector2 LerpBoardPos(float lerp)
        {
            var x = Mathf.Lerp((CurrentBoardPosition.x), (NextBoardPosition.x), lerp);
            var y = Mathf.Lerp((CurrentBoardPosition.y), (NextBoardPosition.y), lerp);
            return new Vector2(x, y);
        }*/

        #region MoveToNeigbour

        public Vector2Int GetNeigbourCoord(CommandDir rotation) => CurrentBoardPosition + Utils.ConvertDirectionToBoardPosOffset(rotation);
        public void Move(CommandDir dir) => NextBoardPosition = GetNeigbourCoord(dir);
        public void MoveTo(Vector2Int pos) => NextBoardPosition = pos;
        
        #endregion

        /*public void SetPosWithAnimation(Vector2 pos, PosSetFlag flag)
        {
            if (flag == PosSetFlag.NONE) return;
            if ((flag & PosSetFlag.Current) == PosSetFlag.Current) CurrentBoardPosition = Utils._V2ToV2Int(pos);
            if ((flag & PosSetFlag.Next) == PosSetFlag.Next) NextBoardPosition = Utils._V2ToV2Int(pos);
            //if ((flag & PosSetFlag.Lerping) == PosSetFlag.Lerping) LerpingBoardPosition = pos;
        }*/

        public void SetCurrentAndNextPos(Vector2 pos)
        {
            CurrentBoardPosition = Utils._V2ToV2Int(pos);
            NextBoardPosition = Utils._V2ToV2Int(pos);
        }
    }
}
