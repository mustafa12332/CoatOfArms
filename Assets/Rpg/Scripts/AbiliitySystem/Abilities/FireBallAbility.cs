using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBallAbility:Ability {
    private const string aName = "FireBall";
    private const string aDescription = "FireBall Ability explodes on impact";
    public FireBallAbility()
        : base(new BasicInformation(aName, aDescription))
    {
        AbilityBehaviors.Add(new Ranged(10f,20f,true));
        AbilityBehaviors.Add(new AreaOfEffect(2f, 2.5f, 10f));
        AbilityBehaviors.Add(new DamageOverTime(45f, 10f,5f));

    }

}
