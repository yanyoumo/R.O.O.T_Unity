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

    public abstract partial class UnitBase : MoveableBase
    {
        protected string UnitName { get; }

        public CoreType UnitCore { get; protected set; }
        public CoreGenre UnitCoreGenre { get; protected set; }
        public Dictionary<RotationDirection, SideType> UnitSides { get; protected set; }

        protected RotationDirection UnitRotation;

        //public RotationDirection UnitRotation => unitRotation;

        protected Transform RootTransform;

        protected Material CoreMat;

        protected MeshRenderer CoreMeshRenderer;
        protected MeshRenderer LocalNorthSideMeshRenderer;
        protected MeshRenderer LocalEastSideMeshRenderer;
        protected MeshRenderer LocalSouthSideMeshRenderer;
        protected MeshRenderer LocalWestSideMeshRenderer;

        protected Transform SideRootTransform;
        protected Transform CoreTransform;

        protected Dictionary<CoreType, string> CoreMatNameDic;
        protected Dictionary<SideType, Color> SideMatColorDic;

        public Mesh DtoDSideMesh;
        public Mesh DtoSSideMesh;

        public Board GameBoard;

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

            SideMatColorDic.Add(SideType.NoConnection, Color.red * 0.75f); //先干脆改成红的
            //SideMatColorDic.Add(SideType.Firewall, new Color(0.6f, 0.1f, 0.1f));
            SideMatColorDic.Add(SideType.Connection, Color.blue);
            //SideMatColorDic.Add(SideType.SerialConnector, new Color(0.1f, 0.6f, 0.6f));
        }

        public bool Visited { get; set; } //for scoring purpose
        public int IntA { get; set; } //for scoring purpose
        public int IntB { get; set; } //for scoring purpose
        public bool InHddGrid { get; set; } //for scoring purpose
        public bool InServerGrid { get; set; } //for scoring purpose

        public readonly RotationDirection[] RotationList = {
            RotationDirection.East,
            RotationDirection.North,
            RotationDirection.South,
            RotationDirection.West
        };

        protected Dictionary<RotationDirection, MeshRenderer> meshRendererLocalDir;

        [HideInInspector]
        public Vector2Int LastNetworkPos = Vector2Int.zero;

        //public GlobalAssetLib _globalAssetLib;

        //North,South,West,East
        protected void InitSide(MeshRenderer meshRenderer,SideType sideType)
        {
            if (sideType == SideType.Connection)
            {
                meshRenderer.material.SetColor("_Color", Color.green*0.55f);
            }
            else if (sideType == SideType.NoConnection)
            {
                //感觉还是有个红的比较靠谱
                //meshRenderer.material.SetColor("_Color", Color.red * 0.25f);
                meshRenderer.enabled = false;
            }
        }

        protected void InitUnitMeshByCore(CoreType core)
        {
            UnitCoreGenre = GetCoreGenreByCoreType(core);
            string connectorMasterNodeName = "";
            string coreMeshNodeName = "";

            if (UnitCoreGenre == CoreGenre.Source)
            {
                connectorMasterNodeName = StaticName.SourceConnectorMasterNodeName;
                coreMeshNodeName = StaticName.SourceCoreNodeName;
            }
            else
            {
                connectorMasterNodeName = StaticName.DestConnectorMasterNodeName;
                coreMeshNodeName = StaticName.DestCoreNodeName;
            }

            Transform[] trans = RootTransform.GetComponentsInChildren<Transform>();
            foreach (var tran in trans)
            {
                if (tran.parent)
                {
                    if (tran.parent.name == "UnitRoot")
                    {
                        if (tran.name == connectorMasterNodeName)
                        {
                            SideRootTransform = tran;
                        }
                        else if (tran.name == coreMeshNodeName)
                        {
                            CoreTransform = tran;
                        }
                        else
                        {
                            MeshRenderer[] meshRenderers = tran.GetComponentsInChildren<MeshRenderer>();
                            foreach (var meshRenderer in meshRenderers)
                            {
                                meshRenderer.enabled = false;
                            }
                        }
                    }
                }
            }

            CoreMeshRenderer = CoreTransform.GetComponentInChildren<MeshRenderer>();

            MeshRenderer[] sideMeshRenderers = SideRootTransform.GetComponentsInChildren<MeshRenderer>();
            foreach (var meshRenderer in sideMeshRenderers)
            {
                if (meshRenderer.name == StaticName.LocalEastSideMeshRendererName)
                {
                    LocalEastSideMeshRenderer = meshRenderer;
                }
                if (meshRenderer.name == StaticName.LocalNorthSideMeshRendererName)
                {
                    LocalNorthSideMeshRenderer = meshRenderer;
                }
                if (meshRenderer.name == StaticName.LocalSouthSideMeshRendererName)
                {
                    LocalSouthSideMeshRenderer = meshRenderer;
                }
                if (meshRenderer.name == StaticName.LocalWestSideMeshRendererName)
                {
                    LocalWestSideMeshRenderer = meshRenderer;
                }
            }
            meshRendererLocalDir=new Dictionary<RotationDirection, MeshRenderer>()
            {
                { RotationDirection.East,LocalEastSideMeshRenderer},
                { RotationDirection.North,LocalNorthSideMeshRenderer},
                { RotationDirection.South,LocalSouthSideMeshRenderer},
                { RotationDirection.West,LocalWestSideMeshRenderer},
            };
        }

        public void SetCoreEmissive(Color color)
        {
            CoreMeshRenderer.material.EnableKeyword("_EMISSION");//还是不懂，为什么每次设置前得Enable一下。
            CoreMeshRenderer.material.SetColor("_EmissionColor", color);
        }

        public void InitUnit(CoreType core, SideType[] sides, Board gameBoard = null)
        {
            Debug.Assert(sides.Length == 4);
            InitUnit(core, sides[0], sides[1], sides[2], sides[3], gameBoard);
        }

        public void InitUnit(CoreType core, SideType lNSide, SideType lSSide, SideType lWSide, SideType lESide,Board gameBoard=null)
        {
            //Debug.Assert(side.Length == 4);
            this.UnitCore = core;
            InitUnitMeshByCore(core);

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
            InHddGrid = false;
            GameBoard = gameBoard;
        }

        protected virtual void Awake()
        {
            RootTransform = transform.parent;
            Debug.Assert(RootTransform != null,"Unit should use as prefab");
            CurrentBoardPosition = new Vector2Int(0,0);
            UnitSides=new Dictionary<RotationDirection, SideType>();
            UnitRotation = RotationDirection.North;

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
            UnitRotation = Utils.GetCWDirection(UnitRotation);
        }
        public void UnitRotateCcw()
        {
            UnitRotation = Utils.GetCCWDirection(UnitRotation);
        }

        public SideType GetLocalSpaceUnitSide(RotationDirection localDirection)
        {
            SideType res = SideType.SIDETYPECOUNT;
            Debug.Assert(UnitSides.TryGetValue(UnitRotation, out res));
            return res;
        }
        public SideType GetWorldSpaceUnitSide(RotationDirection worldDirection)
        {
            SideType res = SideType.SIDETYPECOUNT;
            var desiredLocalSideDirection=Utils.RotateDirectionBeforeRotation(worldDirection, UnitRotation);
            Debug.Assert(UnitSides.TryGetValue(desiredLocalSideDirection, out res));
            return res;
        }

        public void UpdateWorldRotationTransform()
        {
            switch (UnitRotation)
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
            UpdateSideMesh();
        }

        public void UpdateSideMesh()
        {
            //TODO Board也需要一个postAnimationUpdate，就是调这个。旋转之后也要调这个。
            if (GameBoard != null)
            {
                if (UnitCoreGenre == CoreGenre.Destination)
                {
                    foreach (var CurrentSideDirection in RotationList)
                    {
                        if (GetWorldSpaceUnitSide(CurrentSideDirection) == SideType.Connection)
                        {
                            RotationDirection LocalRotation =Utils.RotateDirectionBeforeRotation(CurrentSideDirection, UnitRotation);
                            meshRendererLocalDir.TryGetValue(LocalRotation, out MeshRenderer localMeshRenderer);
                            if (localMeshRenderer != null)
                            {
                                localMeshRenderer.gameObject.GetComponent<MeshFilter>().mesh = DtoDSideMesh;
                                Vector2Int otherUnitPos = GetNeigbourCoord(CurrentSideDirection);
                                GameBoard.Units.TryGetValue(otherUnitPos, out GameObject value);
                                if (value != null)
                                {
                                    Unit otherUnit = value.GetComponentInChildren<Unit>();
                                    bool otherIsSrc=(otherUnit.UnitCoreGenre == CoreGenre.Source);
                                    bool otherSideIsConnecting =(otherUnit.GetWorldSpaceUnitSide(Utils.GetInvertDirection(otherUnit.UnitRotation)) == SideType.Connection);
                                    if (otherIsSrc&&otherSideIsConnecting)
                                    {
                                        localMeshRenderer.gameObject.GetComponent<MeshFilter>().mesh = DtoSSideMesh;
                                    }
                                }                               
                            }
                        }
                    }
                }
            }
        }
    }
}