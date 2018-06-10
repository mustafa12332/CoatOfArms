using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
public class DamageOverTime : AbilityBehavior {

    private const string abname = "Damage Over Time";
    private const string abdescription = "Damage units over time";
    private const BehaviorStartTimes startTime = BehaviorStartTimes.End;
    private float baseEffectDamage;
    private bool isOccupied;
    //private const Sprite icon = Resources.Load();

    private float damageTickDuration;
    private float effectDuration; // how long the effect lasts
    private Stopwatch durationTimer = new Stopwatch();
    
    public DamageOverTime( float effectDuration, float baseEffectDamage,float damageTickDuration)
        : base(new BasicInformation(abname, abdescription), startTime)
    {
        this.effectDuration = effectDuration;
        this.baseEffectDamage = baseEffectDamage;
        isOccupied = false;
        this.damageTickDuration = damageTickDuration;
    }
    public override void PerformBehavior(GameObject playerObject, GameObject objectHit)
    {

     //   StartCoroutine(DOT());
    }

    private IEnumerator DOT()
    {
        durationTimer.Start();

        while (durationTimer.Elapsed.TotalSeconds <= effectDuration)
        {

            yield return new WaitForSeconds(damageTickDuration);
        }
        durationTimer.Stop();
        durationTimer.Reset();
        yield return null;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (isOccupied)
        {
            //do damage
        }
        else
            isOccupied = true;

    }
    private void OnTriggerExit(Collider other)
    {
        isOccupied = false;
    }
}
