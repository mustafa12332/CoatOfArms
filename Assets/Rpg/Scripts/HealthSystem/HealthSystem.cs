using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace RTSEngine { 
public class HealthSystem  {

    private int currentHealth;
    private int maxHealth;
    private int minHealth = 0;
    
    public HealthSystem(int currentHealth,int maxHealth)
    {
        this.currentHealth = currentHealth;
        this.maxHealth = maxHealth;
    }

    public bool doDamage(int amount)
    {
            if (currentHealth < minHealth)
            {
                currentHealth = minHealth;
                return true;
            }
            else { 
                currentHealth -= amount;
                return false;
            }

        }
    public void healDamage(int amount)
    {
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;
        else
            currentHealth += amount;

    }
    public int CurrentHealth
    {
        get { return currentHealth; }
        set { currentHealth = value; }
    }
    public int MaxHealth
    {
        get { return maxHealth; }
        
    }
    }
}
