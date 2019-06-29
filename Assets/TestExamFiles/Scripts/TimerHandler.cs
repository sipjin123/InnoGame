using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerHandler : MonoBehaviour
{
    [SerializeField]
    private List<EventTimerClass> _EventTimerList = new List<EventTimerClass>();
    private List<EventTimerClass> _ToRemove = new List<EventTimerClass>();
    private void Awake()
    {
        ManagerRegistry.Register<TimerHandler>(this);

        _EventTimerList = new List<EventTimerClass>();
    }

    void Update()
    {
        if (_EventTimerList.Count < 1)
            return;
        
        int timerListCount = _EventTimerList.Count;
        for(int i = 0; i < timerListCount; i++)
        {
            var timerClass = _EventTimerList[i];
            if(Input.GetKey(KeyCode.X))
                timerClass.CurrentTime += Time.deltaTime*8;
            timerClass.CurrentTime += Time.deltaTime;
            timerClass.DurationEvent.Invoke(timerClass.CurrentTime);
            if (timerClass.CurrentTime >= timerClass.TimerCap)
            {
                timerClass.TimerReach.Invoke();
                timerClass.CurrentTime = 0;
                if (timerClass.TriggerOnce)
                {
                    _ToRemove.Add(timerClass);
                }
            }
        }

        int removeListCount = _ToRemove.Count;
        if (removeListCount > 0)
        {
            for (int i = 0; i < removeListCount; i++)
            {
                _EventTimerList.Remove(_ToRemove[i]);
            }
            _ToRemove.Clear();
        }
    }

    public void RegisterTimer(EventTimerClass timerClass)
    {
        _EventTimerList.Add(timerClass);
    }
}
