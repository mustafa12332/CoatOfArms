using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace RTSEngine { 
public class ChangeCharacter : AbilityBehavior {
    private const string abname = "Change Character";
    private const string abdescription = "Change character behavior !";
    private const BehaviorStartTimes startTimes = BehaviorStartTimes.End;


    private float effectDuration;
    public ChangeCharacter(float effectDuration):
        base(new BasicInformation(abname, abdescription),startTimes)
    {
        this.effectDuration = effectDuration;
    }
    public override void PerformBehavior(AnimatorOverrideController anim,GameObject particle,GameObject currentGameObject, GameObject changedGameObject)
    {
      
        Job.make(changePlayer(anim,particle, currentGameObject, changedGameObject), true);
        
    }
        
        

     IEnumerator changePlayer(AnimatorOverrideController anim,GameObject particle,GameObject currentGameObject, GameObject changedGameObject)
    {
        //change player
        Transform prevTemp=null;
        particle = Instantiate(particle);
        particle.transform.parent = currentGameObject.transform;
        particle.transform.localPosition = new Vector3(0, 0, 0);
            print("change character entered 1");
        yield return new WaitForSeconds(3f);
        changedGameObject.transform.position = currentGameObject.transform.position;
        changedGameObject.transform.rotation = currentGameObject.transform.rotation;
        currentGameObject.SetActive(false);
        changedGameObject.transform.GetChild(1).gameObject.SetActive(true);
                print("change character entered 2");

                if (Camera.main.gameObject.GetComponent<ThirdPersonCamera>() != null) {
                string team = "team" + (Camera.main.gameObject.GetComponent<ThirdPersonCamera>().teamNo) + "Rpg";
                print(team + "\t" + currentGameObject.tag);
                if(team == currentGameObject.tag) { 
                    prevTemp = Camera.main.gameObject.GetComponent<ThirdPersonCamera>().Target;
                    Camera.main.gameObject.GetComponent<ThirdPersonCamera>().type = 1;
                    Camera.main.gameObject.GetComponent<ThirdPersonCamera>().Target = changedGameObject.transform;
                }
            
            }
            yield return new WaitForSeconds(effectDuration);
            currentGameObject.transform.position = changedGameObject.transform.position;
        currentGameObject.transform.rotation = changedGameObject.transform.rotation;
           
            if (changedGameObject.GetComponent<theiving>().instaniatedThievingUi)
                Destroy(changedGameObject.GetComponent<theiving>().instaniatedThievingUi);
            if (prevTemp != null){
                Camera.main.gameObject.GetComponent<ThirdPersonCamera>().type = 0;
                Camera.main.gameObject.GetComponent<ThirdPersonCamera>().Target = prevTemp;
            }
            print("change character entered 3");
            currentGameObject.SetActive(true);
            changedGameObject.transform.GetChild(1).gameObject.SetActive(false);
        Destroy(particle);
        yield return null;
    }
    public float EffectDuration
    {
        get { return effectDuration; }
        set { effectDuration = value; }
    }
    }
}
