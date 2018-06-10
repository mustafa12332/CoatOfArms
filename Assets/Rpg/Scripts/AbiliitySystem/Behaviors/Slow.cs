using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
public class Slow : AbilityBehavior {

    private const string abname = "Slow";
    private const string abdescription = "Slows Objects Moving Speed!";
    private const BehaviorStartTimes startTime = BehaviorStartTimes.End;
    private bool isOccupied;
    //private const Sprite icon = Resources.Load();

    private float damageTickDuration;
    private float effectDuration; // how long the effect lasts
    private Stopwatch durationTimer = new Stopwatch();
    private float slowPercent;

    public Slow( float effectDuration, float slowPercent)
        : base(new BasicInformation(abname, abdescription), startTime)
    {
        this.effectDuration = effectDuration;
        isOccupied = false;
        damageTickDuration = 0.5f;
        this.slowPercent = slowPercent;
    }
    public override void PerformBehavior(GameObject playerObject, GameObject objectHit)
    {
      

     //   StartCoroutine(SlowMovement(playerObject,objectHit));
    }

    private IEnumerator SlowMovement(GameObject playerObject, GameObject objectHit)
    {

       // if(objectHit.GetComponent<"Movement">() != null)
        //{

//        }

        yield return new WaitForSeconds(effectDuration);




        yield return null;
    }
}
