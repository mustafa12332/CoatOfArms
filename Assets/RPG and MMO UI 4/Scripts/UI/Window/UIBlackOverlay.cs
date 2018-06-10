using UnityEngine;
using UnityEngine.UI.Tweens;

namespace UnityEngine.UI
{
    /// <summary>
    /// The black overlay used behind windows such as the game menu.
    /// To use it for a window simply add the window to the array of windows on this component.
    /// </summary>
	[ExecuteInEditMode, RequireComponent(typeof(Image)), RequireComponent(typeof(CanvasGroup))]
	public class UIBlackOverlay : MonoBehaviour {
		
        [SerializeField] UIWindow[] m_Windows = new UIWindow[0];

		private Image m_Image;
		private CanvasGroup m_CanvasGroup;
        private int m_WindowsCount = 0;

        private bool m_Transitioning = false;

        // Tween controls
        [System.NonSerialized]
        private readonly TweenRunner<FloatTween> m_FloatTweenRunner;

        // Called by Unity prior to deserialization, 
        // should not be called by users
        protected UIBlackOverlay()
        {
            if (this.m_FloatTweenRunner == null)
                this.m_FloatTweenRunner = new TweenRunner<FloatTween>();

            this.m_FloatTweenRunner.Init(this);
        }

        protected void Awake()
		{
			this.m_Image = this.gameObject.GetComponent<Image>();
			this.m_CanvasGroup = this.gameObject.GetComponent<CanvasGroup>();
        }
		
		protected void Start()
		{
			// Non interactable
			this.m_CanvasGroup.interactable = false;
			
			// Hide the overlay
			this.Hide();
		}
		
		protected void OnEnable()
		{
			// Hide the overlay
			if (!Application.isPlaying)
				this.Hide();

            // Hook the transition events
            if (this.m_Windows.Length > 0)
            {
                foreach (UIWindow window in this.m_Windows)
                {
                    window.onTransitionBegin.AddListener(OnTransitionBegin);
                }
            }
        }

        protected void OnDisable()
        {
            // Unhook the transition events
            if (this.m_Windows.Length > 0)
            {
                foreach (UIWindow window in this.m_Windows)
                {
                    window.onTransitionBegin.RemoveListener(OnTransitionBegin);
                }
            }
        }

        public void Show()
		{
			// Show the overlay
			this.SetAlpha(1f);
			
			// Toggle block raycast on
			this.m_CanvasGroup.blocksRaycasts = true;
		}
		
		public void Hide()
		{
			// Hide the overlay
			this.SetAlpha(0f);
			
			// Toggle block raycast off
			this.m_CanvasGroup.blocksRaycasts = false;
		}
		
		public bool IsActive()
		{
			return (this.enabled && this.gameObject.activeInHierarchy);
		}
		
		public bool IsVisible()
		{
			return (this.m_Image.canvasRenderer.GetAlpha() > 0f);
		}
		
		public void OnTransitionBegin(UIWindow window, UIWindow.VisualState state, bool instant)
		{
			if (!this.IsActive() || window == null)
				return;
			
			// Check if we are receiving hide event and we are not showing the overlay to begin with, return
			if (state == UIWindow.VisualState.Hidden && !this.IsVisible())
				return;
			
			// Prepare transition duration
			float duration = (instant) ? 0f : window.transitionDuration;
            TweenEasing easing = window.transitionEasing;

			// Showing a window
			if (state == UIWindow.VisualState.Shown)
			{
				// Increase the window count so we know when to hide the overlay
				this.m_WindowsCount += 1;
				
				// Check if the overlay is already visible
				if (this.IsVisible() && !this.m_Transitioning)
				{
					// Bring the window forward
					UIUtility.BringToFront(window.gameObject);
                    
                    // Break
                    return;
				}
				
				// Bring the overlay forward
				UIUtility.BringToFront(this.gameObject);
				
				// Bring the window forward
				UIUtility.BringToFront(window.gameObject);
                
                // Transition
                this.StartAlphaTween(1f, duration, easing);

                // Toggle block raycast on
                this.m_CanvasGroup.blocksRaycasts = true;
			}
			// Hiding a window
			else
			{
				// Decrease the window count
				this.m_WindowsCount -= 1;
                
                // Never go below 0
                if (this.m_WindowsCount < 0)
					this.m_WindowsCount = 0;
				
				// Check if we still have windows using the overlay
				if (this.m_WindowsCount > 0)
					return;
                
                // Transition
                this.StartAlphaTween(0f, duration, easing);

                // Toggle block raycast on
                this.m_CanvasGroup.blocksRaycasts = false;
			}
		}

        private void StartAlphaTween(float targetAlpha, float duration, TweenEasing easing)
        {
            if (this.m_Image == null)
                return;
            
            // Check if currently transitioning
            if (this.m_Transitioning)
            {
                this.m_FloatTweenRunner.StopTween();
            }

            if (duration == 0f || !Application.isPlaying)
            {
                this.SetAlpha(targetAlpha);
            }
            else
            {
                this.m_Transitioning = true;

                var floatTween = new FloatTween { duration = duration, startFloat = this.m_Image.canvasRenderer.GetAlpha(), targetFloat = targetAlpha };
                floatTween.AddOnChangedCallback(SetAlpha);
                floatTween.ignoreTimeScale = true;
                floatTween.easing = easing;
                floatTween.AddOnFinishCallback(OnTweenFinished);

                this.m_FloatTweenRunner.StartTween(floatTween);
            }
        }

        public void SetAlpha(float alpha)
		{
            if (this.m_Image != null)
			    this.m_Image.canvasRenderer.SetAlpha(alpha);
		}

        protected void OnTweenFinished()
        {
            this.m_Transitioning = false;
        }
	}
}
