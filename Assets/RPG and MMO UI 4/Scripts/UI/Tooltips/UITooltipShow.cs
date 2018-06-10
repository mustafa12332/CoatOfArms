using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    [AddComponentMenu("UI/Tooltip Show"), DisallowMultipleComponent]
    public class UITooltipShow : UIBehaviour, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        public enum Position
        {
            Floating,
            Anchored
        }

        [SerializeField] private Position m_Position = Position.Floating;

        [SerializeField, Tooltip("How long of a delay to expect before showing the tooltip."), Range(0f, 10f)]
        private float m_Delay = 1f;

        [SerializeField] private string m_Title;
        [SerializeField] private string m_Description;

        private bool m_IsTooltipShown = false;

        /// <summary>
        /// Raises the tooltip event.
        /// </summary>
        /// <param name="show">If set to <c>true</c> show.</param>
        public virtual void OnTooltip(bool show)
        {
            if (!this.enabled || !this.IsActive())
                return;

            UITooltip.InstantiateIfNecessary(this.gameObject);

            // If we are showing the tooltip
            if (show)
            {
                // Set the title if not empty
                if (!string.IsNullOrEmpty(this.m_Title))
                {
                    UITooltip.AddTitle(this.m_Title);
                }

                // Spacer
                if (!string.IsNullOrEmpty(this.m_Title) && !string.IsNullOrEmpty(this.m_Description))
                {
                    UITooltip.AddSpacer();
                }

                // Set description if not empty
                if (!string.IsNullOrEmpty(this.m_Description))
                {
                    UITooltip.AddDescription(this.m_Description);
                }

                // Anchor to this slot
                if (this.m_Position == Position.Anchored)
                {
                    UITooltip.AnchorToRect(this.transform as RectTransform);
                }

                // Show the tooltip
                UITooltip.Show();
            }
            else
            {
                // Hide the tooltip
                UITooltip.Hide();
            }
        }

        /// <summary>
        /// Raises the pointer enter event.
        /// </summary>
        /// <param name="eventData">Event data.</param>
        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            // Check if tooltip is enabled
            if (this.enabled && this.IsActive())
            {
                // Instantiate the tooltip now
                UITooltip.InstantiateIfNecessary(this.gameObject);

                // Start the tooltip delayed show coroutine
                // If delay is set at all
                if (this.m_Delay > 0f)
                {
                    this.StartCoroutine("DelayedShow");
                }
                else
                {
                    this.InternalShowTooltip();
                }
            }
        }

        /// <summary>
        /// Raises the pointer exit event.
        /// </summary>
        /// <param name="eventData">Event data.</param>
        public virtual void OnPointerExit(PointerEventData eventData)
        {
            this.InternalHideTooltip();
        }
        
        /// <summary>
        /// Raises the pointer down event.
        /// </summary>
        /// <param name="eventData">Event data.</param>
        public virtual void OnPointerDown(PointerEventData eventData)
        {
            // Hide the tooltip
            this.InternalHideTooltip();
        }

        /// <summary>
        /// Raises the pointer up event.
        /// </summary>
        /// <param name="eventData">Event data.</param>
        public virtual void OnPointerUp(PointerEventData eventData)
        {
        }

        /// <summary>
		/// Internal call for show tooltip.
		/// </summary>
		protected void InternalShowTooltip()
        {
            // Call the on tooltip only if it's currently not shown
            if (!this.m_IsTooltipShown)
            {
                this.m_IsTooltipShown = true;
                this.OnTooltip(true);
            }
        }

        /// <summary>
        /// Internal call for hide tooltip.
        /// </summary>
        protected void InternalHideTooltip()
        {
            // Cancel the delayed show coroutine
            this.StopCoroutine("DelayedShow");

            // Call the on tooltip only if it's currently shown
            if (this.m_IsTooltipShown)
            {
                this.m_IsTooltipShown = false;
                this.OnTooltip(false);
            }
        }

        protected IEnumerator DelayedShow()
        {
            yield return new WaitForSeconds(this.m_Delay);
            this.InternalShowTooltip();
        }
    }
}
