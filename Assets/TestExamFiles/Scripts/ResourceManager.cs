using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ResourceManager : MonoBehaviour
{
    [SerializeField]
    private UIResourceHandler _UIResourceHandler;

    private ConsumeResourceEvent _ConsumeResourceEvent = new ConsumeResourceEvent();
    public ConsumeResourceEvent ConsumeResourceEvent { get { return _ConsumeResourceEvent; } }

    private ConsumeResourceEvent _AddResourceEvent = new ConsumeResourceEvent();
    public ConsumeResourceEvent AddResourceEvent { get { return _AddResourceEvent; } }

    [SerializeField]
    private List<ResourceData> _ResourceData;

    private void Awake()
    {
        ManagerRegistry.Register<ResourceManager>(this);


        _ResourceData.Find(_ => _.ResourceType == ResourceType.Gold).Quantity = 100;
        _ResourceData.Find(_ => _.ResourceType == ResourceType.Steel).Quantity = 0;
        _ResourceData.Find(_ => _.ResourceType == ResourceType.Wood).Quantity = 0;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Y))
        {
            _ResourceData.Find(_ => _.ResourceType == ResourceType.Gold).Quantity = 1000;
            _ResourceData.Find(_ => _.ResourceType == ResourceType.Steel).Quantity = 1000;
            _ResourceData.Find(_ => _.ResourceType == ResourceType.Wood).Quantity = 1000;
        }
    }

    void Start()
    {
        _ConsumeResourceEvent.AddListener(_ =>
        {
            var resource = _ResourceData.Find(q => q.ResourceType == _.ResourceType);
            resource.Quantity -= _.Quantity;

            _UIResourceHandler.UpdateResourceText(_.ResourceType, resource.Quantity);
        });
        _AddResourceEvent.AddListener(_ =>
        {
            var resource = _ResourceData.Find(q => q.ResourceType == _.ResourceType);
            resource.Quantity += _.Quantity;
            _UIResourceHandler.UpdateResourceText(_.ResourceType, resource.Quantity);
        });
    }

    public Sprite RequestSprite(ResourceType type)
    {
        return _ResourceData.Find(_ => _.ResourceType == type).Sprite;
    }

    public bool SufficientResources(List<ResourceCostClass> costList)
    {
        int costCount = costList.Count;
        for(int i = 0; i < costCount; i++)
        {
            if (costList[i].Quantity > _ResourceData.Find(_ => _.ResourceType == costList[i].ResourceType).Quantity)
            {
                Debug.LogError("Not enough : " + costList[i].Quantity + " " + costList[i].ResourceType);
                return false;
            }
        }
        return true;
    }
}
