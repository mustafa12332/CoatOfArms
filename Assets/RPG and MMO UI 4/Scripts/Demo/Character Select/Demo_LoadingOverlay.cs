using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI.Tweens;

namespace UnityEngine.UI
{
    /// <summary>
    /// Loading Overlay
    /// This script requires to be attached to a canvas.
    /// The canvas must have the highest sorting order.
    /// The canvas must have a GraphicRaycaster.
    /// The GraphicRaycaster should be disabled by default.
    /// The visibility of the overlay is controlled by a CanvasGroup alpha.
    /// </summary>
    [RequireComponent(typeof(Canvas)), RequireComponent(typeof(GraphicRaycaster))]
    public class Demo_LoadingOverlay : MonoBehaviour
    {
        #region singleton
        private static Demo_LoadingOverlay m_Instance;
        public static Demo_LoadingOverlay Instance { get { return m_Instance; } }
        #endregion

        [SerializeField] private UIProgressBar m_ProgressBar;
        [SerializeField] private CanvasGroup m_CanvasGroup;
        [SerializeField] private GraphicRaycaster m_Raycaster;

        [SerializeField] private TweenEasing m_TransitionEasing = TweenEasing.InOutQuint;
		[SerializeField] private float m_TransitionDuration = 0.1f;

        private bool m_Showing = false;
        private int m_LoadSceneId = 0;

        // Tween controls
		[System.NonSerialized] private readonly TweenRunner<FloatTween> m_FloatTweenRunner;
		
		// Called by Unity prior to deserialization, 
		// should not be called by users
		protected Demo_LoadingOverlay()
		{
			if (this.m_FloatTweenRunner == null)
				this.m_FloatTweenRunner = new TweenRunner<FloatTween>();
			
			this.m_FloatTweenRunner.Init(this);
		}

        protected void Awake()
        {
            // Prevent multiple
            if (m_Instance != null)
            {
                Destroy(this.gameObject);
                return;
            }

            m_Instance = this;
            DontDestroyOnLoad(this.gameObject);

            if (this.m_Raycaster == null) this.m_Raycaster = this.gameObject.GetComponent<GraphicRaycaster>();
            if (this.m_Raycaster != null) this.m_Raycaster.enabled = false;
        }

        protected void OnDestroy()
        {
            if (m_Instance.Equals(this))
                m_Instance = null;
        }

        public void LoadScene(int sceneId)
        {
            this.m_Showing = true;
            this.m_LoadSceneId = sceneId;

            // Enable the raycaster so it blocks input
            if (this.m_Raycaster != null) this.m_Raycaster.enabled = true;

            // Start the tween
            this.StartAlphaTween(1f, this.m_TransitionDuration, true);
        }

        /// <summary>
        /// Starts alpha tween.
        /// </summary>
        /// <param name="targetAlpha">Target alpha.</param>
        /// <param name="duration">Duration.</param>
        /// <param name="ignoreTimeScale">If set to <c>true</c> ignore time scale.</param>
        public void StartAlphaTween(float targetAlpha, float duration, bool ignoreTimeScale)
        {
            if (this.m_CanvasGroup == null)
                return;

            var floatTween = new FloatTween { duration = duration, startFloat = this.m_CanvasGroup.alpha, targetFloat = targetAlpha };
            floatTween.AddOnChangedCallback(SetCanvasAlpha);
            floatTween.AddOnFinishCallback(OnTweenFinished);
            floatTween.ignoreTimeScale = ignoreTimeScale;
            floatTween.easing = this.m_TransitionEasing;
            this.m_FloatTweenRunner.StartTween(floatTween);
        }

        /// <summary>
        /// Sets the canvas alpha.
        /// </summary>
        /// <param name="alpha">Alpha.</param>
        protected void SetCanvasAlpha(float alpha)
        {
            if (this.m_CanvasGroup == null)
                return;

            // Set the alpha
            this.m_CanvasGroup.alpha = alpha;
        }

        /// <summary>
        /// Raises the list tween finished event.
        /// </summary>
        protected void OnTweenFinished()
        {
            // When the loading overlay is shown
            if (this.m_Showing)
            {
                this.m_Showing = false;
                StartCoroutine(AsynchronousLoad());
            }
            else
            {
                // Disable the raycaster so it does not block input
                if (this.m_Raycaster != null) this.m_Raycaster.enabled = false;
            }
        }
        
        IEnumerator AsynchronousLoad()
        {
            yield return null;

            AsyncOperation ao = SceneManager.LoadSceneAsync(this.m_LoadSceneId);
            ao.allowSceneActivation = false;

            while (!ao.isDone)
            {
                // [0, 0.9] > [0, 1]
                float progress = Mathf.Clamp01(ao.progress / 0.9f);
                
                // Update the progress bar
                if (this.m_ProgressBar != null)
                {
                    this.m_ProgressBar.fillAmount = progress;
                }

                // Loading completed
                if (ao.progress == 0.9f)
                {
                    ao.allowSceneActivation = true;

                    // Hide the loading overlay
                    this.StartAlphaTween(0f, this.m_TransitionDuration, true);
                }

                yield return null;
            }
        }
    }
}
