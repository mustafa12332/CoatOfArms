using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
namespace RTSEngine
{
    public class fireArrowAbilityUse : NetworkBehaviour
    {
        public GameObject auraPrefab;
        private Button button;
        private Image fillImage;
        private FireArrowAbility fireArrowAbility;
        public bool otherAbilityUse = false;
        public void onAbilityUse(GameObject btn)
        {
            if (otherAbilityUse)
                return;
            fillImage = btn.transform.GetChild(0).gameObject.GetComponent<Image>();
            button = btn.GetComponent<Button>();
            button.interactable = false;
            fillImage.fillAmount = 1;
            CmdUseAbility();
            Job.make(resetImage(120f, button));

        }
        [Command]
        public void CmdUseAbility()
        {

            RpcUseAbility();
        }
        [ClientRpc]
        public void RpcUseAbility()
        {
            fireArrowAbility = new FireArrowAbility(60f);
            fireArrowAbility.useAbility(gameObject,auraPrefab);

        }
        IEnumerator resetImage(float amountOfSeconds, Button button)
        {
            Image fillImage = button.transform.GetChild(0).gameObject.GetComponent<Image>();
            while (fillImage.fillAmount > 0)
            {
                float amountOfDeduction = 1f / amountOfSeconds;
                print("entered \t" + fillImage.fillAmount + "\t" + amountOfDeduction);
                fillImage.fillAmount -= amountOfDeduction;
                yield return new WaitForSeconds(1.0f);
            }
            fillImage.fillAmount = 0;
            button.interactable = true;
            yield return null;
        }
    }
}