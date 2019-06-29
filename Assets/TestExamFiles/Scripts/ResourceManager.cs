using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Zenject;

public class ResourceManager : MonoBehaviour
{
    [Inject]
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
    }


    void Start()
    {
        _ConsumeResourceEvent.AddListener(_ =>
        {
            var resource = _ResourceData.Find(q => q.ResourceType == _.ResourceType);
            resource.Quantity -= _.Quantity;
            _UIResourceHandler.UpdateResourceText(_.ResourceType, resource.Quantity);
            MessageBroker.Default.Publish(new ResourcesUpdatedSignal());
        });
        _AddResourceEvent.AddListener(_ =>
        {
            var resource = _ResourceData.Find(q => q.ResourceType == _.ResourceType);
            resource.Quantity += _.Quantity;
            _UIResourceHandler.UpdateResourceText(_.ResourceType, resource.Quantity);
            MessageBroker.Default.Publish(new ResourcesUpdatedSignal());
        });


        _ResourceData.Find(_ => _.ResourceType == ResourceType.Gold).Quantity = 100;
        _ResourceData.Find(_ => _.ResourceType == ResourceType.Steel).Quantity = 0;
        _ResourceData.Find(_ => _.ResourceType == ResourceType.Wood).Quantity = 0;

        MessageBroker.Default.Publish(new ResourcesUpdatedSignal());
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            _ResourceData.Find(_ => _.ResourceType == ResourceType.Gold).Quantity = 1000;
            _ResourceData.Find(_ => _.ResourceType == ResourceType.Steel).Quantity = 1000;
            _ResourceData.Find(_ => _.ResourceType == ResourceType.Wood).Quantity = 1000;
        }
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
                return false;
            }
        }
        return true;
    }
}
