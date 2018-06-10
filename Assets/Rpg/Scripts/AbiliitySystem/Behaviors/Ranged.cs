using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ranged : AbilityBehavior {
    private const string abname = "Ranged";
    private const string abdescription = "A ranged attack!";
    private const BehaviorStartTimes startTime = BehaviorStartTimes.Beginning;
    
    //private const Sprite icon = Resources.Load();

    private float minDistance;
    private float maxDistance;
    private bool isRandom;
    private float lifeDistance;
    public Ranged(float minDistance,float maxDistance,bool isRandom)
        :base(new BasicInformation(abname,abdescription),startTime)
    {
        this.minDistance = minDistance;
        this.maxDistance = maxDistance;
        this.isRandom = isRandom;
    }
    public override void PerformBehavior(GameObject playerObject,GameObject objectHit)
    {
        lifeDistance = isRandom ? Random.Range(minDistance, maxDistance) : maxDistance;
      //  StartCoroutine(CheckDistance(playerObject.transform.position));
    }
    private IEnumerator CheckDistance(Vector3 startPosition)
    {
     //   float tempDistance = Vector3.Distance(startPosition, this.transform.position);
      //  while (tempDistance >= lifeDistance) {
      //      tempDistance = Vector3.Distance(startPosition, this.transform.position);
            
       // }
       // gameObject.SetActive(false);
        yield return null;

    }

    public float MaxDistance
    {
        get { return maxDistance; }
    }
    public float MinDistance
    {
        get { return minDistance; }
    }
}
