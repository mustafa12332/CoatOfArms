using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability {
    private BasicInformation objectInfo;
    private List<AbilityBehavior> behaviors;
    private bool requiresTarget;
    private bool canCastOnself;
    private int cooldown; //secs
    private GameObject particleEffect; //assigned when we create the ability;
    private float castTime;//cast time for each ability
    private float cost;//deducts from manapoints
    private AbilityType type;

    public enum AbilityType
    {
        Spell,
        Melee
    }
    public Ability(BasicInformation objectInfo)
    {
        this.objectInfo = objectInfo;
        this.behaviors = new List<AbilityBehavior>();
        cooldown = 0;
        requiresTarget = false;
        canCastOnself = false;
    }
    public Ability(BasicInformation objectInfo,List<AbilityBehavior> behaviors)
    {
        this.objectInfo = objectInfo;
        this.behaviors = new List<AbilityBehavior>();
        this.behaviors = behaviors;
        cooldown = 0;
        requiresTarget = false;
        canCastOnself = false;
    }
    public Ability(BasicInformation objectInfo, List<AbilityBehavior> behaviors,bool requiresTarget,int cooldown,GameObject particleEffect)
    {
        this.objectInfo = objectInfo;
        this.behaviors = new List<AbilityBehavior>();
        this.behaviors = behaviors;
        this.cooldown = cooldown;
        this.requiresTarget = requiresTarget;
        canCastOnself = false;
        this.particleEffect = particleEffect;
    }

    public BasicInformation AbilityInfo
    {
        get { return objectInfo; }
    }
    public int AbilityCoolDown
    {
        get { return cooldown; }
    }
    public List<AbilityBehavior> AbilityBehaviors
    {
        get { return behaviors; }
    }
    
}
