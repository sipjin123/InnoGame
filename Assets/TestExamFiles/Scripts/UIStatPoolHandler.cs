using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIStatPoolHandler : MonoBehaviour
{
    [SerializeField]
    private List<UITimerTemplate> 
        _UITimerUsedList = new List<UITimerTemplate>(), 
        _UITimerReserverList = new List<UITimerTemplate>();

    [SerializeField]
    private GameObject _PrefabObject;

    [SerializeField]
    private UIProductionTemplate _UIProductionTemplate;

    [SerializeField]
    private Transform _Pool;

    private void Awake()
    {
        ManagerRegistry.Register<UIStatPoolHandler>(this);
    }

    public UIProductionTemplate RequestProductionTemplate(Vector3 location)
    {
        _UIProductionTemplate.gameObject.SetActive(true);
        _UIProductionTemplate.transform.position = location;
        return _UIProductionTemplate;
    }

    public void RefreshTemplate()
    {
        //_UIProductionTemplate.StopListeners.Invoke();
        if (_UIProductionTemplate.CurrentBuilding)
        {
            _UIProductionTemplate.CurrentBuilding.PurgeProgressView();
            _UIProductionTemplate.CurrentBuilding = null;
            _UIProductionTemplate.gameObject.SetActive(false);
        }
    }

    public UITimerTemplate RequestTimer(Vector3 positon)
    {
        if (_UITimerReserverList.Count > 0)
        {
            var timerToGive = _UITimerReserverList[0];
            _UITimerReserverList.RemoveAt(0);
            _UITimerUsedList.Add(timerToGive);
            return timerToGive;
        }
        else
        {
            GameObject temp = Instantiate(_PrefabObject, _Pool);
            var timerToGive = temp.GetComponent<UITimerTemplate>();
            _UITimerUsedList.Add(timerToGive);
            return timerToGive;
        }
    }
    public void ReturnTimer(UITimerTemplate timerObj)
    {
        timerObj.gameObject.SetActive(false);
        _UITimerUsedList.Remove(timerObj);
        _UITimerReserverList.Add(timerObj);
    }
}
