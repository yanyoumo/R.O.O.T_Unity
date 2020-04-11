using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ROOT
{
    public class Cursor : MoveableBase
    {
        protected Transform RootTransform;
        private MeshFilter _meshFilter;
        public Material tm;

        public Mesh CursorMesh;
        public Mesh IndicatorMesh;

        void Awake()
        {
            RootTransform = transform;
            Debug.Assert(RootTransform != null, "Unit should use as prefab");
            CurrentBoardPosition = new Vector2Int(0, 0);
            _meshFilter = GetComponentInChildren<MeshFilter>();
            _meshFilter.mesh = CursorMesh;
            tm = GetComponentInChildren<MeshRenderer>().material;
        }

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
            _meshFilter.mesh = IndicatorMesh;
        }
    }
}