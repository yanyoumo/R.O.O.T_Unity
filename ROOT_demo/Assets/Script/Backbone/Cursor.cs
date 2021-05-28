using System;
using System.Collections;
using com.ootii.Messages;
using ROOT.Message;
using ROOT.Message.Inquiry;
using ROOT.SetupAsset;
using UnityEngine;

namespace ROOT
{
    public class Cursor : MoveableBase
    {
        protected Transform RootTransform;
        private MeshFilter _meshFilter;
        public Material tm;
        //private string defaultColor = "#5FD2D6";
        private Color defaultColor => ColorLibManager.Instance.ColorLib.ROOT_CURSOR_DEFAULT;
        private Color blinkingColor => ColorLibManager.Instance.ColorLib.ROOT_CURSOR_DEFAULT;
        private Color infoColor => ColorLibManager.Instance.ColorLib.ROOT_CURSOR_INFOMODE;
        
        public Color CursorColor
        {
            set => tm.color = value;
        }

        public Mesh CursorMesh;
        public Mesh IndicatorMesh;
        private float blinkTimer = 0.0f;
        private float blinkDuration = 1.0f;

        private Coroutine BlinkingCoroutine = null;

        private bool _targeting;
        public bool Targeting
        {
            set
            {
                _targeting = value;
                ToggleBlinking();
            }
            get => _targeting;
        }

        private void ToggleBlinking()
        {
            if (_targeting)
            {
                if (BlinkingCoroutine == null)
                {
                    BlinkingCoroutine = StartCoroutine(TargetingBlinking());
                }

            }
            else
            {
                if (BlinkingCoroutine != null)
                {
                    StopCoroutine(BlinkingCoroutine);
                    tm.color = defaultColor;
                    BlinkingCoroutine = null;
                }
            }
        }

        private IEnumerator TargetingBlinking()
        {
            while (true)
            {
                yield return 0;
                var val = 0.5f * (Mathf.Sin(Time.time * 9.0f) + 1);
                tm.color = Color.Lerp(Color.green, defaultColor, val);
            }
        }

        private bool ShowMesh
        {
            get => GetComponentInChildren<MeshRenderer>().enabled;
            set => GetComponentInChildren<MeshRenderer>().enabled = value;
        }

        private bool infoCursor = false;

        private void HintToggle(IMessage rMessage)
        {
            infoCursor = !infoCursor;
            tm.color = infoCursor ? infoColor : defaultColor;
        }

        void Awake()
        {
            RootTransform = transform;
            Debug.Assert(RootTransform != null, "Unit should use as prefab");
            CurrentBoardPosition = new Vector2Int(0, 0);
            _meshFilter = GetComponentInChildren<MeshFilter>();
            _meshFilter.mesh = CursorMesh;
            tm = GetComponentInChildren<MeshRenderer>().material;
            if (StartGameMgr.UseTouchScreen)
            {
                ShowMesh = false;
            }
            MessageDispatcher.AddListener(WorldEvent.InGameOverlayToggleEvent, HintToggle);
        }

        private void OnDestroy()
        {
            MessageDispatcher.RemoveListener(WorldEvent.InGameOverlayToggleEvent, HintToggle);
        }

        public override bool Immovable { get; set; }

        public override Transform AnimatingRoot => RootTransform;

        public override void UpdateTransform(Vector3 pos)
        {
            Vector3 cursorPos=new Vector3(pos.x,0.26f,pos.z);
            RootTransform.position = cursorPos;
        }

        public void SetCursorMesh()
        {
            _meshFilter.mesh = CursorMesh;
        }

        public void SetIndMesh()
        {
            if (!ShowMesh)
            {
                ShowMesh = true;
            }
            _meshFilter.mesh = IndicatorMesh;
        }

        public void ClampPosesInBoard()
        {
            CurrentBoardPosition = Board.ClampPosInBoard(CurrentBoardPosition);
            NextBoardPosition = Board.ClampPosInBoard(NextBoardPosition);
        }
        
        public override void SetCurrentAndNextPos(Vector2 pos)
        {
            CurrentBoardPosition = Common.Utils._V2ToV2Int(pos);
            NextBoardPosition = Common.Utils._V2ToV2Int(pos);
            MessageDispatcher.SendMessage(new CursorMovedEventData
            {
                CurrentPosition = CurrentBoardPosition
            });
        }
        
        //因为光标实质无向，就是这两个把这个放在儿简单弄一下就行。
        public override void PingPongRotationDirection()
        {
            transform.rotation = Quaternion.identity;
            CurrentRotationDirection = RotationDirection.North;
            NextRotationDirection = RotationDirection.North;
        }
        
        public void RotateCw()
        {
            NextRotationDirection = RotationDirection.East;
        }
    }
}