using UnityEngine;
using UnityEngine.Events;

namespace UnityEngine.UI
{
    [RequireComponent(typeof(UIWindow)), RequireComponent(typeof(UIAlwaysOnTop))]
    public class UIModalBox : MonoBehaviour
    {
        [SerializeField] private Text m_Text1;
        [SerializeField] private Text m_Text2;
        [SerializeField] private Button m_Button;
        [SerializeField] private Text m_ButtonText;

        private UIWindow m_Window;

        public UnityEvent onConfirm = new UnityEvent();
        public UnityEvent onCancel = new UnityEvent();

        protected void Awake()
        {
            // Make sure we have the window component
            if (this.m_Window == null)
            {
                this.m_Window = this.gameObject.GetComponent<UIWindow>();
            }

            // Prepare some window parameters
            this.m_Window.ID = UIWindowID.ModalBox;
            this.m_Window.escapeKeyAction = UIWindow.EscapeKeyAction.None;

            // Hook an event to the window
            this.m_Window.onTransitionComplete.AddListener(OnWindowTransitionEnd);

            // Prepare the always on top component
            UIAlwaysOnTop aot = this.gameObject.GetComponent<UIAlwaysOnTop>();
            aot.order = UIAlwaysOnTop.ModalBoxOrder;

            // Hook the button click event
            if (this.m_Button != null)
            {
                this.m_Button.onClick.AddListener(Confirm);
            }
        }

        /// <summary>
        /// Sets the text on the first line.
        /// </summary>
        /// <param name="text"></param>
        public void SetText1(string text)
        {
            if (this.m_Text1 != null)
            {
                this.m_Text1.text = text;
            }
        }

        /// <summary>
        /// Sets the text on the second line.
        /// </summary>
        /// <param name="text"></param>
        public void SetText2(string text)
        {
            if (this.m_Text2 != null)
            {
                this.m_Text2.text = text;
            }
        }

        /// <summary>
        /// Sets the button text.
        /// </summary>
        /// <param name="text"></param>
        public void SetButtonText(string text)
        {
            if (this.m_ButtonText != null)
            {
                this.m_ButtonText.text = text;
            }
        }

        /// <summary>
        /// Shows the modal box.
        /// </summary>
        public void Show()
        {
            // Show the modal
            if (this.m_Window != null)
            {
                this.m_Window.Show();
            }
        }

        /// <summary>
        /// Closes the modal box.
        /// </summary>
        public void Close()
        {
            // Hide the modal
            if (this.m_Window != null)
            {
                this.m_Window.Hide();
            }

            // Invoke the cancel event
            if (this.onCancel != null)
            {
                this.onCancel.Invoke();
            }
        }

        public void Confirm()
        {
            // Hide the modal
            if (this.m_Window != null)
            {
                this.m_Window.Hide();
            }

            // Invoke the confirm event
            if (this.onConfirm != null)
            {
                this.onConfirm.Invoke();
            }
        }

        public void OnWindowTransitionEnd(UIWindow window, UIWindow.VisualState state)
        {
            // Destroy the modal box when hidden
            if (state == UIWindow.VisualState.Hidden)
            {
                Destroy(this.gameObject);
            }
        }
    }
}
