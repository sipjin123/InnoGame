﻿using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

public class GenericBuilding : MonoBehaviour
{
    public BuildingType BuildingType { get { return _BuildingData.BuildingType; } }

    [SerializeField]
    private BuildingData _BuildingData;

    [SerializeField]
    private GameObject _BuildingIndicator;

    [SerializeField]
    private Vector2 _GridSize;

    [Inject]
    private UIStatPoolHandler _UIStatPoolHandler;

    public Vector2 GridSize { get { return _GridSize; } }

    private bool _HasInitialized;
    public bool HasInitialized { get { return _HasInitialized; } }

    private bool _CanOperate;
    public bool CanOperate { get { return _CanOperate; } }

    private FloatEvent _TimerTickEvent = new FloatEvent();
    public FloatEvent TimerTickEvent { get { return _TimerTickEvent; } }

    private UnityEvent _TimerReachEvent = new UnityEvent();
    public UnityEvent TimerReachEvent { get { return _TimerReachEvent; } }

    private bool _ProductionInProgress;

    private UnityAction<float> _TickerAction;
    private UnityAction _TickerReachedAction;
    private UnityAction _StopListeningAction;

    private ResourceManager _ResourceManager;
    private UIProductionTemplate _UIProductionTemplateCache;

    #region Init

    public virtual void Initialize()
    {
        _HasInitialized = true;
        _ResourceManager = ManagerRegistry.Get<ResourceManager>();
        InitBuilding();
    }

    private void InitBuilding()
    {
        UnityEvent buildingCompleteEvent = new UnityEvent();
        FloatEvent durationTickEvent = new FloatEvent();

        var borrowedTimer = _UIStatPoolHandler.RequestTimer(transform.position);
        borrowedTimer.gameObject.SetActive(true);
        borrowedTimer.transform.position = transform.position;
        borrowedTimer.SetMaxCap(_BuildingData.BuildTime, transform);

        buildingCompleteEvent.AddListener(() =>
        {
            _TickerAction += ListenToTicker;
            _TickerReachedAction += RegisterResource;

            _UIStatPoolHandler.ReturnTimer(borrowedTimer);
            _BuildingIndicator.SetActive(false);
            _CanOperate = true;

            durationTickEvent.RemoveAllListeners();
            buildingCompleteEvent.RemoveAllListeners();

            if (_BuildingData.AutoProduce)
                ProduceResources();
        });
        durationTickEvent.AddListener(_ =>
        {
            borrowedTimer.UpdateText(_);
        });

        EventTimerClass newTimerClass = new EventTimerClass
        {
            CurrentTime = 0,
            TimerCap = _BuildingData.BuildTime,
            TriggerOnce = true,
            TimerReach = buildingCompleteEvent,
            DurationEvent = durationTickEvent,
        };

        MessageBroker.Default.Publish(new RegisterTimeSignal { EventTimerClass = newTimerClass });
    }

    #endregion Init

    #region Resource Production / Data Registry

    private void ProduceResources()
    {
        _ProductionInProgress = true;
        _TimerReachEvent.AddListener(_TickerReachedAction);

        EventTimerClass newTimerClass = new EventTimerClass
        {
            CurrentTime = 0,
            TimerCap = _BuildingData.BuildTime,
            TriggerOnce = !_BuildingData.AutoProduce,
            TimerReach = _TimerReachEvent,
            DurationEvent = _TimerTickEvent,
        };
        MessageBroker.Default.Publish(new RegisterTimeSignal { EventTimerClass = newTimerClass });
    }

    private void RegisterResource()
    {
        if (_UIProductionTemplateCache)
        {
            _UIProductionTemplateCache.UpdateTime(0);
            if (!_BuildingData.AutoProduce)
                _UIProductionTemplateCache.IsProgressing(false);
        }
        int resourceCount = _BuildingData.ResourceGathered.Count;
        for (int i = 0; i < resourceCount; i++)
        {
            var quantity = _BuildingData.ResourceGathered[i].Quantity;
            var resourceType = _BuildingData.ResourceGathered[i].ResourceType;
            _ResourceManager.AddResourceEvent.Invoke(new ResourceCostClass
            {
                Quantity = quantity,
                ResourceType = resourceType
            });
        }
        if (!_BuildingData.AutoProduce)
        {
            _ProductionInProgress = false;
            _TimerReachEvent.RemoveListener(_TickerReachedAction);
        }
    }

    #endregion Resource Production / Data Registry

    #region UI Progress Display Functionality

    public void ShowUIProgress(UIProductionTemplate uiProductionTemplate)
    {
        PurgeProgressView();
        _UIProductionTemplateCache = uiProductionTemplate;
        _UIProductionTemplateCache.SetData(
            _BuildingData.ResourceGathered[0].ResourceType,
            _BuildingData.BuildingType.ToString(),
            _BuildingData.ProductionTime,
            transform);

        _UIProductionTemplateCache.IsProgressing(_ProductionInProgress);
        _UIProductionTemplateCache.CurrentBuilding = this;
        _UIProductionTemplateCache.ProduceButton.onClick.RemoveAllListeners();
        _UIProductionTemplateCache.ProduceButton.onClick.AddListener(() =>
        {
            ProduceResources();
            _UIProductionTemplateCache.IsProgressing(true);
        });
        _TimerTickEvent.AddListener(_TickerAction);
    }

    public void PurgeProgressView()
    {
        _TimerTickEvent.RemoveListener(_TickerAction);
        if (_UIProductionTemplateCache)
        {
            _UIProductionTemplateCache.ProduceButton.onClick.RemoveAllListeners();
            _UIProductionTemplateCache.UpdateTime(0);
        }
        _UIProductionTemplateCache = null;
    }

    private void ListenToTicker(float tick)
    {
        _UIProductionTemplateCache.UpdateTime(tick);
    }

    #endregion UI Progress Display Functionality

    #region RequestData

    public bool IsDecoration()
    {
        return _BuildingData.Decoration;
    }

    public List<ResourceCostClass> ResourceCost()
    {
        return _BuildingData.ResourceCost;
    }

    public bool IsPreviewed()
    {
        return _UIProductionTemplateCache == null ? false : true;
    }

    #endregion RequestData
}