using System.Collections.Generic;
using UnityEngine;

public class UIResourceHandler : MonoBehaviour
{
    [SerializeField]
    private List<UIResourcePair> _ResourceUIPair;

    private void Start()
    {
        int resourceCount = _ResourceUIPair.Count;
        for (int i = 0; i < resourceCount; i++)
        {
            _ResourceUIPair[i].TextUI.text = _ResourceUIPair[i].ResourceData.Quantity.ToString();
        }
    }

    public void UpdateResourceText(ResourceType type, int quantity)
    {
        var uiToUpdate = _ResourceUIPair.Find(_ => _.ResourceData.ResourceType == type);
        uiToUpdate.TextUI.text = quantity.ToString();
    }
}