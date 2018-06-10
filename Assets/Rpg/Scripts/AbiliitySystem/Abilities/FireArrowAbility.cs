using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace RTSEngine
{
    public class FireArrowAbility : Ability {

        private const string aName = "FireBall";
        private const string aDescription = "FireBall Ability explodes on impact";

        public FireArrowAbility(float effectDuration)
            : base(new BasicInformation(aName, aDescription))
        {
            AbilityBehaviors.Add(new ChangePrefab(effectDuration));


        }
        public void useAbility(GameObject currentPlayer,GameObject auraPrefab)
        {
            AbilityBehaviors[0].PerformBehavior(currentPlayer,auraPrefab);
        }
    }
}