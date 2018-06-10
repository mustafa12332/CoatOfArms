using UnityEngine;
using System.Collections;

namespace UnityEngine.UI
{
    public class UISliderDisplayValue : MonoBehaviour
    {
        [SerializeField] private Slider m_slider;
        [SerializeField] private Text m_Text;
        [SerializeField] private string m_Format = "0";
        [SerializeField] private string m_Append = "%";

        protected void Awake()
        {
            if (this.m_slider == null) this.m_slider = this.gameObject.GetComponent<Slider>();
        }

        protected void OnEnable()
        {
            if (this.m_slider != null)
            {
                this.m_slider.onValueChanged.AddListener(SetValue);
                this.SetValue(this.m_slider.value);
            }
        }

        protected void OnDisable()
        {
            if (this.m_slider != null)
            {
                this.m_slider.onValueChanged.RemoveListener(SetValue);
            }
        }

        public void SetValue(float value)
        {
            if (this.m_Text != null)
                this.m_Text.text = Mathf.RoundToInt(value * 100f).ToString(this.m_Format) + this.m_Append;
        }
    }
}
