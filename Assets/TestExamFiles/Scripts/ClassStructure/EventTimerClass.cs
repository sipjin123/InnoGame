using UnityEngine;
using UnityEngine.Events;

public class EventTimerClass
{
    public float TimerCap;
    public float CurrentTime;
    public UnityEvent TimerReach;
    public FloatEvent DurationEvent;
    public bool TriggerOnce;
}
