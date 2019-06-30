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

    [SerializeField]
    private Image _Icon;

    private float _MaxCap;

    private Transform _TransformReference;

    public void IsProgressing(bool ifTrue)
    {
        if (!ifTrue)
        {
            _TimerText.text = "0";
        }
        ProduceButton.gameObject.SetActive(!ifTrue);
    }

    public void SetData(ResourceType type, string name, int maxCap, Transform transformReference)
    {
        var resource = _ResourceSprite.Find(_ => _.ResourceType == type);
        _Icon.sprite = resource.Sprite;
        _BuildingName.text = name;
        _MaxCap = maxCap;
        _TransformReference = transformReference;
        transform.SetParent(_TransformReference);
    }

    public void UpdateTime(float val)
    {
        //transform.localPosition = _TransformReference.localPosition;
        _TimerText.text = val.ToString("f0");
        _FillBar.fillAmount = val / _MaxCap;
    }
}