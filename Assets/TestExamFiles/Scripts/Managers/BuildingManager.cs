using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Zenject;

public class BuildingManager : MonoBehaviour
{
    [Inject]
    private Camera _MainCam;

    [Inject]
    private BuildingPoolHandler _BuildingPoolHandler;

    [Inject]
    private UIBuildingHandler _UIBuildingHandler;

    [Inject]
    private GridHandler _GridHandler;

    [Inject]
    private UIStatPoolHandler _UIStatPoolHandler;

    [SerializeField]
    private List<BuildingData> _BuildingDataList;

    private GenericBuilding _CachedBuilding;
    private ResourceManager _ResourceManager;
    private Vector3 _CachedLocation;
    private RaycastHit hit;
    private bool _ValidLocation;
    private bool _MoveMode;
    private bool _IsMovingBuilding;
    private bool _SetSpawn;
    private int _GroundLayerMask;

    #region Init

    private void Awake()
    {
        ManagerRegistry.Register<BuildingManager>(this);
    }

    private void Start()
    {
        _ResourceManager = ManagerRegistry.Get<ResourceManager>();

        _GroundLayerMask = LayerMask.GetMask(Constants.LAYER_GROUND);

        RegisterEvents();
        ClickStreamSetup();
    }

    private void ClickStreamSetup()
    {
        var clickBuildingInfoStream = Observable.EveryUpdate()
            .Where(_ => Input.GetMouseButtonDown(0))
            .Where(_ => _MoveMode == false);

        clickBuildingInfoStream.Subscribe(xs =>
        {
            StreamBuildingInfo();
        });

        var clickBuildingMoveStream = Observable.EveryUpdate()
            .Where(_ => Input.GetMouseButtonDown(0))
            .Where(_ => _MoveMode == true && _IsMovingBuilding == false);

        clickBuildingMoveStream.Subscribe(xs =>
        {
            StreamBuildingMovement();
        });

        var clickBuildingReleaseStream = Observable.EveryUpdate()
            .Where(_ => !Input.GetMouseButton(0))
            .Where(_ => _CachedBuilding != null)
            .Where(_ => _MoveMode == true);

        clickBuildingReleaseStream.Subscribe(xs =>
        {
            StreamBuildingRelease();
        });

        var movementStream = Observable.EveryUpdate()
            .Where(_ => _SetSpawn == true);
        movementStream.Subscribe(xs =>
        {
            StreamMoveBuilding();
        });
    }

    private void RegisterEvents()
    {
        MessageBroker.Default.Receive<EditModeButtonClickSignal>().Subscribe(_ =>
        {
            _MoveMode = !_MoveMode;
            MessageBroker.Default.Publish(new MoveModeSignal { MoveBuildings = _MoveMode });
            if (_MoveMode)
                _UIStatPoolHandler.RefreshTemplate();
        });

        _UIBuildingHandler.PointerDownEvent.AddListener(_ =>
        {
            SelectBuildingEvent(_);
        });

        _UIBuildingHandler.PointerUpEvent.AddListener(_ =>
        {
            ReleaseBuildingEvent(_);
        });
    }

    #endregion

    #region Observable Stream Events / Mouse Events

    private void StreamMoveBuilding()
    {
        if (Physics.Raycast(_MainCam.ScreenPointToRay(Input.mousePosition), out hit, 200, _GroundLayerMask))
        {
            _CachedBuilding.transform.position = hit.point;
            _ValidLocation = _GridHandler.SnapToGrid(_CachedBuilding.transform, _CachedBuilding.GridSize);
        }
    }

    private void StreamBuildingInfo()
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

    private void StreamBuildingMovement()
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

    private void StreamBuildingRelease()
    {
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

    #endregion Observable Stream Events / Mouse Events

    #region Events Listeners
    
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

    #endregion Events Listeners

    #region Building Deployment / Return / Cost Inquiry

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

    #endregion Building Deployment / Return / Cost Inquiry
}