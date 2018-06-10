using UnityEngine;
using UnityEngine.Events;

namespace UnityEngine.UI
{
    [AddComponentMenu("UI/Modal Box Create"), DisallowMultipleComponent]
    public class UIModalBoxCreate : MonoBehaviour
    {
        [SerializeField] private string m_Text1;
        [SerializeField] private string m_Text2;
        [SerializeField] private string m_ButtonText;

        public UnityEvent onConfirm = new UnityEvent();
        public UnityEvent onCancel = new UnityEvent();

        public void CreateAndShow()
        {
            UIModalBox box = UIModalBoxManager.Instance.Create(this.gameObject);
            box.SetText1(this.m_Text1);
            box.SetText2(this.m_Text2);
            box.SetButtonText(this.m_ButtonText);
            box.onConfirm.AddListener(OnConfirm);
            box.onCancel.AddListener(OnCancel);
            box.Show();
        }

        public void OnConfirm()
        {
            if (this.onConfirm != null)
            {
                this.onConfirm.Invoke();
            }
        }

        public void OnCancel()
        {
            if (this.onCancel != null)
            {
                this.onCancel.Invoke();
            }
        }
    }
}
