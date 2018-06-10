using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityBehavior:MonoBehaviour  {
    private BasicInformation objectInfo;
    private BehaviorStartTimes startTime;


    public AbilityBehavior(BasicInformation objectInfo,BehaviorStartTimes startTime)
    {
        this.objectInfo = objectInfo;
        this.startTime = startTime;
    }

    public enum BehaviorStartTimes
    {
        Beginning,
        Middle,
        End
    }
    public virtual void PerformBehavior(GameObject playerrObject)
    {
        Debug.LogWarning("Need To Add Behavior");
    }
    public virtual void PerformBehavior(GameObject playerrObject, GameObject objectHit)
    {
        Debug.LogWarning("Need To Add Behavior");
    }
    public virtual void PerformBehavior(AnimatorOverrideController anim,GameObject particle,GameObject playerrObject, GameObject objectHit)
    {
        Debug.LogWarning("Need To Add Behavior");
    }
    public BasicInformation AbilityBehaviorInfo
    {
        get { return objectInfo; }
    }

    public BehaviorStartTimes AbilityBehaviorStartTime
    {
        get { return startTime; }
    }
}
