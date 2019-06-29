using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIBuildingHandler : MonoBehaviour
{
    [SerializeField]
    private List<UIBuildingPair> _UIBuildingPair;

    private SelectBuildingEvent _PointerDownEvent = new SelectBuildingEvent();
    private SelectBuildingEvent _PointerUpEvent = new SelectBuildingEvent();
    public SelectBuildingEvent PointerDownEvent { get { return _PointerDownEvent; } }
    public SelectBuildingEvent PointerUpEvent { get { return _PointerUpEvent; } }

    private void Start()
    {
        int loopCount = _UIBuildingPair.Count;
        for (int i = 0; i < loopCount ; i++)
        {
            var buildingType = _UIBuildingPair[i].BuildingType;

            EventTrigger trigger = _UIBuildingPair[i].UIButton;
            EventTrigger.Entry pointerDownEntry = new EventTrigger.Entry();
            pointerDownEntry.eventID = EventTriggerType.PointerDown;

            var buildingCostInfo = ManagerRegistry.Get<BuildingManager>().RequestBuildingCost(buildingType) ;

            for (int q = 0; q < buildingCostInfo.Count; q++)
            {
                var selectedTemplate= _UIBuildingPair[i].UIBuldingTemplate.CostUIList[q];
                selectedTemplate.GameObject.SetActive(true);
                selectedTemplate.Sprite.sprite = ManagerRegistry.Get<ResourceManager>().RequestSprite(buildingCostInfo[q].ResourceType);
                selectedTemplate.UIText.text = buildingCostInfo[q].Quantity.ToString();
            }
            _UIBuildingPair[i].UIBuldingTemplate.BuildingNameText.text = _UIBuildingPair[i].BuildingType.ToString();

            pointerDownEntry.callback.AddListener((eventData) =>
            {
                PointerDown(buildingType);
            });
            trigger.triggers.Add(pointerDownEntry);

            EventTrigger.Entry pointerUpEntry = new EventTrigger.Entry();
            pointerUpEntry.eventID = EventTriggerType.PointerUp;

            pointerUpEntry.callback.AddListener((eventData) =>
            {
                PointerUp(buildingType);
            });
            trigger.triggers.Add(pointerUpEntry);
        }
    }

    private void PointerDown(BuildingType type)
    {
        _PointerDownEvent.Invoke(type);
    }

    private void PointerUp(BuildingType type)
    {
        _PointerUpEvent.Invoke(type);
    }
}