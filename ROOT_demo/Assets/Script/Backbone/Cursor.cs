using System.Collections;
using UnityEngine;

namespace ROOT
{
    public class Cursor : MoveableBase
    {
        protected Transform RootTransform;
        private MeshFilter _meshFilter;
        public Material tm;
        private string defaultColor = "#5FD2D6";

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
                    ColorUtility.TryParseHtmlString(defaultColor, out var colB);
                    tm.color = colB;
                    BlinkingCoroutine = null;
                }
            }
        }

        private IEnumerator TargetingBlinking()
        {
            while (true)
            {
                yield return 0;
                ColorUtility.TryParseHtmlString(defaultColor, out var colB);
                var val = 0.5f * (Mathf.Sin(Time.time * 9.0f) + 1);
                tm.color = Color.Lerp(Color.green, colB, val);
            }
        }

        private bool ShowMesh
        {
            get => GetComponentInChildren<MeshRenderer>().enabled;
            set => GetComponentInChildren<MeshRenderer>().enabled = value;
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
        }

        public override bool Immovable { get; set; }

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

        /*public void Update()
        {
            //RISK 这个到时候改成基于事件的、因为这个东西的引用不好搞，就先轮询吧。
            //TODO 这个逻辑意外地的不好弄，试图立刻搞了；等有时间把时序舔明白再弄吧。
        }*/
    }
}