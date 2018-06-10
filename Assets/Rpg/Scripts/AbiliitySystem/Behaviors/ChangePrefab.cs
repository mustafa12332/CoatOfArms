using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace RTSEngine {
    public class ChangePrefab : AbilityBehavior {
        private const string abname = "Change Character";
        private const string abdescription = "Change character behavior !";
        private const BehaviorStartTimes startTimes = BehaviorStartTimes.End;

        private float effectDuration;
        public ChangePrefab(float effectDuration) :
           base(new BasicInformation(abname, abdescription), startTimes)
        {
            this.effectDuration = effectDuration;
        }
        public override void PerformBehavior(GameObject currentGameObject,GameObject auraPrefab)
        {

            Job.make(fireArrowPrefab(currentGameObject,auraPrefab), true);

        }

        private IEnumerator fireArrowPrefab(GameObject currentGameObject,GameObject auraPrefab)
        {
            //add the aura effect 
            auraPrefab = Instantiate(auraPrefab);
            auraPrefab.transform.parent = currentGameObject.transform;
            auraPrefab.transform.localPosition = new Vector3(0, 0, 0);
            yield return new WaitForSeconds(4f);
            Destroy(auraPrefab);
            currentGameObject.GetComponent<BowManager>().arrowFlag = true;
          //  currentGameObject.GetComponent<AbilityUse>().otherAbilityUse = true;
            yield return new WaitForSeconds(effectDuration);
            currentGameObject.GetComponent<BowManager>().arrowFlag = false;
        //    currentGameObject.GetComponent<AbilityUse>().otherAbilityUse = false;
           

        }
    }
}