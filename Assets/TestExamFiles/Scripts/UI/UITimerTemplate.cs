using UnityEngine;
using UnityEngine.UI;

public class UITimerTemplate : MonoBehaviour
{
    [SerializeField]
    private Text _TimerText;

    [SerializeField]
    private Image _ProgressBar;

    private float _MaxCapacity;

    public void SetMaxCap(float max)
    {
        _MaxCapacity = max;
    }

    public void UpdateText(float val)
    {
        _TimerText.text = val.ToString("f0");
        _ProgressBar.fillAmount =   val / _MaxCapacity;
    }
}