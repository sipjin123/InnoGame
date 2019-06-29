using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIBuildingHandler : MonoBehaviour
{
    [SerializeField]
    private List<UIBuildingPair> _UIBuildingPair;

    private SelectBuildingEvent _PointerDownEvent = new SelectBuildingEvent();
    private SelectBuildingEvent _PointerUpEvent = new SelectBuildingEvent();
    public SelectBuildingEvent PointerDownEvent { get { return _PointerDownEvent; } }
    public SelectBuildingEvent PointerUpEvent { get { return _PointerUpEvent; } }
    [SerializeField]
    private Button _EditButton;
    private bool _InEditMode;
    [SerializeField]
    private GameObject _EditModeNotif;

    private BuildingManager _BuildingManager;
    private ResourceManager _ResourceManager;
    private void Awake()
    {
        _EditButton.onClick.AddListener(() =>
        {
            MessageBroker.Default.Publish(new EditModeButtonClickSignal());
        });
        MessageBroker.Default.Receive<MoveModeSignal>().Subscribe(_ =>
        {
            var result = _.MoveBuildings;
            _EditModeNotif.SetActive(result);
        }).AddTo(this);

        MessageBroker.Default.Receive<ResourcesUpdatedSignal>().Subscribe(_ =>
        {
            CheckAffordability();
        }).AddTo(this);
    }

    private void Start()
    {
        _BuildingManager = ManagerRegistry.Get<BuildingManager>();
        _ResourceManager = ManagerRegistry.Get<ResourceManager>();

        SetupTemplates();

    }

    private void SetupTemplates()
    {
        int loopCount = _UIBuildingPair.Count;
        for (int i = 0; i < loopCount; i++)
        {
            var buildingType = _UIBuildingPair[i].BuildingType;

            var buildingCostInfo = _BuildingManager.RequestBuildingCost(buildingType);

            for (int q = 0; q < buildingCostInfo.Count; q++)
            {
                var selectedTemplate = _UIBuildingPair[i].UIBuldingTemplate.CostUIList[q];
                selectedTemplate.GameObject.SetActive(true);
                selectedTemplate.Sprite.sprite = ManagerRegistry.Get<ResourceManager>().RequestSprite(buildingCostInfo[q].ResourceType);
                selectedTemplate.UIText.text = buildingCostInfo[q].Quantity.ToString();
            }
            _UIBuildingPair[i].UIBuldingTemplate.BuildingNameText.text = _UIBuildingPair[i].BuildingType.ToString();

            _UIBuildingPair[i].UIBuldingTemplate.InsufficientNotice.SetActive(false);


            EventTrigger trigger = _UIBuildingPair[i].UIButton;

            //handles pointer up event
            EventTrigger.Entry pointerDownEntry = new EventTrigger.Entry();
            pointerDownEntry.eventID = EventTriggerType.PointerDown;
            pointerDownEntry.callback.AddListener((eventData) =>
            {
                _PointerDownEvent.Invoke(buildingType);
            });
            trigger.triggers.Add(pointerDownEntry);

            //handles pointer up event
            EventTrigger.Entry pointerUpEntry = new EventTrigger.Entry();
            pointerUpEntry.eventID = EventTriggerType.PointerUp;
            pointerUpEntry.callback.AddListener((eventData) =>
            {
                _PointerUpEvent.Invoke(buildingType);
            });
            trigger.triggers.Add(pointerUpEntry);
        }
    }

    private void CheckAffordability()
    {
        int loopCount = _UIBuildingPair.Count;
        for (int i = 0; i < loopCount; i++)
        {
            bool cantAfford = true;
            var buildingCostInfo = _BuildingManager.RequestBuildingCost(_UIBuildingPair[i].BuildingType);
            int innerLoopCount = buildingCostInfo.Count;
            for (int q = 0; q < innerLoopCount; q++)
            {
                if(_ResourceManager.SufficientResources(buildingCostInfo))
                {
                    cantAfford = false;
                }
            }
            _UIBuildingPair[i].UIBuldingTemplate.InsufficientNotice.SetActive(cantAfford);

        }
    }
}