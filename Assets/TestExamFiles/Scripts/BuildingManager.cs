using System.Collections.Generic;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    [SerializeField]
    private GenericBuilding _CachedBuilding;

    [SerializeField]
    private Camera _MainCam;

    [SerializeField]
    private BuildingPoolHandler _BuildingPoolHandler;

    [SerializeField]
    private UIBuildingHandler _UIBuildingHandler;

    [SerializeField]
    private GridHandler _GridHandler;

    private ResourceManager _ResourceManager;
    private UIStatPoolHandler _UIStatPoolHandler;
    private Vector3 _CachedLocation;
    private bool _ValidLocation;
    private bool _MoveMode;
    private bool _IsMovingBuilding;
    private bool _SetSpawn;
    private RaycastHit hit;
    private int _GroundLayerMask;

    [SerializeField]
    private List<BuildingData> _BuildingDataList;

    private void Awake()
    {
        ManagerRegistry.Register<BuildingManager>(this);   
    }

    private void Start()
    {
        _UIStatPoolHandler = ManagerRegistry.Get<UIStatPoolHandler>();
        _ResourceManager = ManagerRegistry.Get<ResourceManager>();

         _GroundLayerMask = LayerMask.GetMask(Constants.LAYER_GROUND);

        RegisterEvents();
    }

    private void RegisterEvents()
    {
        _UIBuildingHandler.PointerDownEvent.AddListener(_ =>
        {
            SelectBuildingEvent(_);
        });

        _UIBuildingHandler.PointerUpEvent.AddListener(_ =>
        {
            ReleaseBuildingEvent(_);
        });
    }

    private void SelectBuildingEvent(BuildingType type)
    {
        if (_SetSpawn)
            return;

        if (!_ResourceManager.SufficientResources(_BuildingDataList.Find(_ => _.BuildingType == type).ResourceCost))
        {
            Debug.LogError("InsufficientFunds");
            return;
        }

        //sets the building to follow pointer position
        if (_CachedBuilding == null)
        {
            _CachedBuilding = _BuildingPoolHandler.RequestGenericBuilding(type);
            _CachedBuilding.gameObject.SetActive(true);
            _SetSpawn = true;
        }
    }

    private void ReleaseBuildingEvent(BuildingType type)
    {
        if (!_SetSpawn)
            return;

        //dispatches building to the grid
        if (_ValidLocation)
        {
            DispatchBuilding();
        }
        //returns the building to unused pool
        else
        {
            _GridHandler.ClearGrid();
            ReturnBuilding();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            var bldng = _BuildingPoolHandler.RequestGenericBuilding(BuildingType.Steelmill);
            bldng.transform.position = new Vector3(5, 1, 0);
            bldng.gameObject.SetActive(true);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            _MoveMode = !_MoveMode;
        }

        MoveBuilding();
        ClickBuildingForInfo();
        ClickExistingBuilding();
    }

    private void ClickBuildingForInfo()
    {
        if (_MoveMode)
            return;

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            RaycastHit buildingHit;
            if (Physics.Raycast(_MainCam.ScreenPointToRay(Input.mousePosition), out buildingHit, 200))
            {
                if (buildingHit.collider.GetComponent<GenericBuilding>())
                {
                    var temp = buildingHit.collider.GetComponent<GenericBuilding>();
                    if (!temp.CanOperate || temp.IsPreviewed() || temp.IsDecoration())
                        return;

                    _UIStatPoolHandler.RefreshTemplate();
                    var productionUITemplate = _UIStatPoolHandler.RequestProductionTemplate(temp.transform.position);
                    temp.ShowUIProgress(productionUITemplate);
                }
                else
                {
                    _UIStatPoolHandler.RefreshTemplate();
                }
            }
        }
    }

    private void ClickExistingBuilding()
    {
        if (!_MoveMode)
            return;

        if (!_IsMovingBuilding)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                RaycastHit buildingHit;
                if (Physics.Raycast(_MainCam.ScreenPointToRay(Input.mousePosition), out buildingHit, 200))
                {
                    if (buildingHit.collider.GetComponent<GenericBuilding>())
                    {
                        var temp = buildingHit.collider.GetComponent<GenericBuilding>();
                        _GridHandler.RemoveGridData(temp.transform, temp.GridSize);
                        _CachedBuilding = temp;
                        _SetSpawn = true;
                        _IsMovingBuilding = true;
                        _CachedLocation = temp.transform.position;
                    }
                }
            }
        }
        else if (_IsMovingBuilding)
        {
            if (!Input.GetKey(KeyCode.Mouse0))
            {
                if (_CachedBuilding == null)
                    return;

                if (_GridHandler.SnapToGrid(_CachedBuilding.transform, _CachedBuilding.GridSize))
                {
                    DispatchBuilding();
                }
                else
                {
                    _GridHandler.ClearGrid();
                    _CachedBuilding.transform.position = _CachedLocation;
                    _GridHandler.RegisterToGrid(_CachedBuilding.transform, _CachedBuilding.GridSize);
                    _CachedBuilding = null;
                    _SetSpawn = false;
                }
                _IsMovingBuilding = false;
            }
        }
    }

    private void MoveBuilding()
    {
        if (!_SetSpawn)
            return;
        if (Physics.Raycast(_MainCam.ScreenPointToRay(Input.mousePosition), out hit, 200, _GroundLayerMask))
        {
            _CachedBuilding.transform.position = hit.point;
            _ValidLocation = _GridHandler.SnapToGrid(_CachedBuilding.transform, _CachedBuilding.GridSize);
        }
    }

    private void ReturnBuilding()
    {
        _SetSpawn = false;
        _BuildingPoolHandler.ReturnToPool(_CachedBuilding);
        _CachedBuilding = null;
    }

    private void DispatchBuilding()
    {
        _GridHandler.RegisterToGrid(_CachedBuilding.transform, _CachedBuilding.GridSize);
        if (!_CachedBuilding.HasInitialized)
        {
            var resourceCosting = _CachedBuilding.ResourceCost();
            int loopCount = resourceCosting.Count;
            for (int i = 0; i < loopCount; i++)
            {
                _ResourceManager.ConsumeResourceEvent.Invoke(resourceCosting[i]);
            }
            _CachedBuilding.Initialize();
        }
        _SetSpawn = false;
        _CachedBuilding = null;
    }

    public List<ResourceCostClass> RequestBuildingCost(BuildingType type)
    {
        return _BuildingDataList.Find(_ => _.BuildingType == type).ResourceCost;
    }
}