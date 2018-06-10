using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace RTSEngine { 
public class SpyAbility : Ability {

    private const string aName = "Spy Ability";
    private const string aDescription = "Changes player to other player";
    private const float effectDuration = 60f;
    private const float coolDownTimer = effectDuration *4;
    public SpyAbility()
        : base(new BasicInformation(aName, aDescription))
    {

        AbilityBehaviors.Add(new ChangeCharacter(effectDuration));

    }
    public void useAbility(AnimatorOverrideController anim, GameObject particle, GameObject player, GameObject changedPlayer)
    {
        AbilityBehaviors[0].PerformBehavior(anim,particle, player, changedPlayer
            );
    }
    public float CoolDownTimer{
        get { return coolDownTimer; }     
}
}
}
