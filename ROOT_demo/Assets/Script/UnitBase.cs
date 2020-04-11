using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace ROOT
{
    public enum RotationDirection
    {
        //即所为当前状态的表示，也作为旋转动作的标识
        //顺时针为正：±0，+90，-90，±180
        North, East,West,South
    }

    public abstract class UnitBase : MoveableBase
    {
        protected string UnitName { get; }

        public CoreType UnitCore { get; protected set; }
        public Dictionary<RotationDirection,SideType> UnitSides { get; protected set; }

        protected RotationDirection unitRotation;

        public RotationDirection UnitRotation => unitRotation;

        protected Transform RootTransform;

        public MeshRenderer CoreMeshRenderer;
        public MeshRenderer LocalNorthSideMeshRenderer;
        public MeshRenderer LocalEastSideMeshRenderer;
        public MeshRenderer LocalSouthSideMeshRenderer;
        public MeshRenderer LocalWestSideMeshRenderer;

        protected Dictionary<CoreType, string> CoreMatNameDic;
        protected Dictionary<SideType, Color> SideMatColorDic;

        protected UnitBase()
        {
            UnitName = "";
        }

        public void InitDic()
        {
            CoreMatNameDic.Add(CoreType.PCB, GlobalResourcePath.UNIT_PCB_MAT_NAME);
            CoreMatNameDic.Add(CoreType.BackPlate, "");
            CoreMatNameDic.Add(CoreType.Bridge, "");
            CoreMatNameDic.Add(CoreType.Cooler, "");
            CoreMatNameDic.Add(CoreType.HardDrive, GlobalResourcePath.UNIT_HDD_MAT_NAME);
            CoreMatNameDic.Add(CoreType.NetworkCable, GlobalResourcePath.UNIT_NETCABLE_MAT_NAME);
            CoreMatNameDic.Add(CoreType.Processor, GlobalResourcePath.UNIT_PROCESSOR_MAT_NAME);
            CoreMatNameDic.Add(CoreType.Server, GlobalResourcePath.UNIT_SERVER_MAT_NAME);

            SideMatColorDic.Add(SideType.NoConnection, Color.red*0.75f);//先干脆改成红的
            //SideMatColorDic.Add(SideType.Firewall, new Color(0.6f, 0.1f, 0.1f));
            SideMatColorDic.Add(SideType.Connection, Color.blue);
            //SideMatColorDic.Add(SideType.SerialConnector, new Color(0.1f, 0.6f, 0.6f));
        }

        public bool Visited { get; set; }//for scoring purpose
        public int IntA { get; set; }//for scoring purpose
        public int IntB { get; set; }//for scoring purpose
        public bool InHDDGrid { get; set; }//for scoring purpose
        public bool InServerGrid { get; set; }//for scoring purpose

        public Vector2Int LastNetworkPos = Vector2Int.zero;

        //public GlobalAssetLib _globalAssetLib;

        //North,South,West,East
        public void InitUnit(CoreType core, SideType[] sides)
        {
            Debug.Assert(sides.Length == 4);
            InitUnit(core, sides[0], sides[1], sides[2], sides[3]);
        }

        private void InitSide(MeshRenderer meshRenderer,SideType sideType)
        {
            if (sideType == SideType.Connection)
            {
                meshRenderer.material.SetColor("_Color", Color.green*0.55f);
            }
            else if (sideType == SideType.NoConnection)
            {
                //感觉还是有个红的比较靠谱
                meshRenderer.material.SetColor("_Color", Color.red * 0.25f);
                //meshRenderer.enabled = false;
            }
        }

        public void InitUnit(CoreType core, SideType lNSide, SideType lSSide, SideType lWSide, SideType lESide)
        {
            //Debug.Assert(side.Length == 4);
            this.UnitCore = core;
            UnitSides.Add(RotationDirection.North, lNSide);
            UnitSides.Add(RotationDirection.South, lSSide);
            UnitSides.Add(RotationDirection.West, lWSide);
            UnitSides.Add(RotationDirection.East, lESide);

            CoreMatNameDic.TryGetValue(UnitCore, out string val);
            CoreMeshRenderer.material = Resources.Load<Material>(GlobalResourcePath.UNIT_MAT_PATH_PREFIX + val);
            Debug.Assert(CoreMeshRenderer.material);

            InitSide(LocalNorthSideMeshRenderer,lNSide);
            InitSide(LocalEastSideMeshRenderer, lESide);
            InitSide(LocalSouthSideMeshRenderer, lSSide);
            InitSide(LocalWestSideMeshRenderer, lWSide);

            Visited = false;
            InServerGrid = false;
            InHDDGrid = false;
        }

        protected virtual void Awake()
        {
            RootTransform = transform.parent;
            Debug.Assert(RootTransform != null,"Unit should use as prefab");
            CurrentBoardPosition = new Vector2Int(0,0);
            UnitSides=new Dictionary<RotationDirection, SideType>();
            unitRotation = RotationDirection.North;

            CoreMatNameDic=new Dictionary<CoreType, string>();
            SideMatColorDic=new Dictionary<SideType, Color>();
            InitDic();
        }

        public override void UpdateTransform(Vector3 pos)
        {
            RootTransform.position = pos;
        }

        public void UnitRotateCw()
        {
            unitRotation = Utils.GetCWDirection(unitRotation);
        }
        public void UnitRotateCcw()
        {
            unitRotation = Utils.GetCCWDirection(unitRotation);
        }

        public SideType GetLocalSpaceUnitSide(RotationDirection localDirection)
        {
            SideType res = SideType.SIDETYPECOUNT;
            Debug.Assert(UnitSides.TryGetValue(unitRotation, out res));
            return res;
        }
        public SideType GetWorldSpaceUnitSide(RotationDirection worldDirection)
        {
            SideType res = SideType.SIDETYPECOUNT;
            var desiredLocalSideDirection=Utils.RotateDirectionBeforeRotation(worldDirection, unitRotation);
            Debug.Assert(UnitSides.TryGetValue(desiredLocalSideDirection, out res));
            return res;
        }

        public void UpdateWorldRotationTransform()
        {
            switch (unitRotation)
            {
                case RotationDirection.North:
                    RootTransform.rotation=Quaternion.Euler(0,0,0);
                    break;
                case RotationDirection.East:
                    RootTransform.rotation = Quaternion.Euler(0, 90, 0);
                    break;
                case RotationDirection.West:
                    RootTransform.rotation = Quaternion.Euler(0, 270, 0);
                    break;
                case RotationDirection.South:
                    RootTransform.rotation = Quaternion.Euler(0, 180, 0);
                    break;
            }
        }
    }
}