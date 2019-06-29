using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIProductionTemplate : MonoBehaviour
{
    public UnityEvent StopListeners = new UnityEvent();
    public GenericBuilding CurrentBuilding;
    public Button ProduceButton;
    [SerializeField]
    private Text _TimerText;
    [SerializeField]
    private List<ResourceSpriteClass> _ResourceSprite;

    [SerializeField]
    private Text _BuildingName;

    [SerializeField]
    private Image _FillBar;

    private float _MaxCap;

    [SerializeField]
    private Image _Icon;
    public void IsProgressing(bool ifTrue)
    {
        if (!ifTrue)
        {
            _TimerText.text = "0";
        }
        ProduceButton.gameObject.SetActive(!ifTrue);
    }

    public void SetData(ResourceType type,string name, int maxCap)
    {
        var resource = _ResourceSprite.Find(_ => _.ResourceType == type);
        _Icon.sprite = resource.Sprite;
        _BuildingName.text = name;
        _MaxCap = maxCap;
    }

    public void UpdateTime(float val)
    {
        _TimerText.text = val.ToString("f0");
        _FillBar.fillAmount = val / _MaxCap;
    }


}