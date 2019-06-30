using UnityEngine;
using UnityEngine.UI;

public class UITimerTemplate : MonoBehaviour
{
    [SerializeField]
    private Text _TimerText;

    [SerializeField]
    private Image _ProgressBar;

    private float _MaxCapacity;

    private Transform _TransformReference;

    public void SetMaxCap(float max, Transform transformReference)
    {
        _TransformReference = transformReference;
        _MaxCapacity = max;
    }

    public void UpdateText(float val)
    {
        transform.localPosition = _TransformReference.localPosition;
        _TimerText.text = val.ToString("f0");
        _ProgressBar.fillAmount = val / _MaxCapacity;
    }
}