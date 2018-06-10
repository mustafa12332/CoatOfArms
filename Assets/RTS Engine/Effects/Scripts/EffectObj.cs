using UnityEngine;
using System.Collections;

namespace RTSEngine
{
	public class EffectObj : MonoBehaviour {

		public string Code; //Give each type of attack object a unique code used to identify it.

        public bool LocalLifeTime = true; //Is the lifetime of this effect object controlled by this component or an external component?
		public float LifeTime = 3.0f; //Determine how long will the effect object will be shown for
		[HideInInspector]
		public float Timer; 

		void Update ()
		{
            if (LocalLifeTime == true)
            {
                if (Timer > 0.0f)
                {
                    Timer -= Time.deltaTime;
                }
                if (Timer < 0.0f)
                {
                    Timer = 0.0f;
                    gameObject.SetActive(false);
                }
            }
		}
	}
}