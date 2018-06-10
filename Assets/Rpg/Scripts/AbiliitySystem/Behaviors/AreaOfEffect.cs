using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
[RequireComponent(typeof(SphereCollider))]
public class AreaOfEffect :AbilityBehavior{
    private const string abname = "Area Of Effect";
    private const string abdescription = "An area of damage!";
    private const BehaviorStartTimes startTime = BehaviorStartTimes.End;
    private float baseEffectDamage;
    private bool isOccupied;
    //private const Sprite icon = Resources.Load();

    private float damageTickDuration;
    private float areaRadius; // radius of collider
    private float effectDuration; // how long the effect lasts
    private Stopwatch durationTimer = new Stopwatch();

    public AreaOfEffect(float areaRadius,float effectDuration,float baseEffectDamage)
        : base(new BasicInformation(abname, abdescription), startTime)
    {
        this.areaRadius = areaRadius;
        this.effectDuration = effectDuration;
        this.baseEffectDamage = baseEffectDamage;
        isOccupied = false;
        damageTickDuration = 0.5f;
    }
    public override void PerformBehavior(GameObject playerObject, GameObject objectHit)
    {
      //  SphereCollider sc = this.gameObject.GetComponent<SphereCollider>(); 
       // sc.radius = areaRadius;
        //sc.isTrigger = true;

     //   StartCoroutine(AOE());
    }

    private IEnumerator AOE()
    {
        durationTimer.Start();
        
        while(durationTimer.Elapsed.TotalSeconds <= effectDuration)
        {
            if(isOccupied)
            {
                //do Damage
            }

            yield return new WaitForSeconds(damageTickDuration);
        }
        durationTimer.Stop();
        durationTimer.Reset();
        yield return null;
    }
    private void OnTriggerEnter(Collider other)
    {
        if(isOccupied)
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
