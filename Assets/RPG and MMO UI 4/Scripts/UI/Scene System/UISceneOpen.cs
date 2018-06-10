using UnityEngine;
using System.Collections;

namespace UnityEngine.UI
{
    public class UISceneOpen : MonoBehaviour
    {
        [SerializeField] private int m_SceneId = 0;

        public void Open()
        {
            if (UISceneManager.instance != null)
            {
                UIScene scene = UISceneManager.instance.GetScene(this.m_SceneId);

                if (scene != null)
                {
                    scene.TransitionTo();
                }
            }
        }
    }
}
