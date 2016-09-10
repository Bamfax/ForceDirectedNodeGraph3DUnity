using ProgressBar.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace ProgressBar
{
    /// <summary>
    /// This Script is directed at linearly progressing designs.
    /// </summary>
    public class ProgressBarBehaviour : MonoBehaviour, IIncrementable, IDecrementable
    {
        /// <summary>
        /// Rect from the panel that will act as Filler.
        /// </summary>
        [SerializeField]
        private RectTransform m_FillRect;
        /// <summary>
        /// Class used for storing the Min and Max width values that the Filler will vary between.
        /// </summary>
        private FillerProperty m_FillerInfo;

        public FillerProperty FillerInfo
        {
            get
            {
                if (m_FillerInfo == null)
                    m_FillerInfo = new FillerProperty(0, m_FillRect.rect.width);
                return m_FillerInfo;
            }
        }

        /// <summary>
        /// The Progress Value Class stores the current Filler state as a fraction of its maximal value (normalized).
        /// This is raw Data that you don't need to be using. See the Value property.
        /// </summary>
        private ProgressValue m_Value;
        /// <summary>
        /// Value is the variable you want to call to set or get the current Filler value as a percentage.
        /// It must always be set as a value between 0 and 100.
        /// </summary>
        public float Value
        {
            get
            {
                return Mathf.Round(m_Value.AsFloat / m_FillerInfo.MaxWidth * 100);
            }
            set
            {
                SetFillerSizeAsPercentage(value);
            }
        }
        /// <summary>
        /// This is the core of the Filler animation. If there is a difference between m_Value and TransitoryValue,
        /// the latter will play catch up in the Update Method.
        /// </summary>
        /// <remarks>
        /// Keep in mind that this means that you get the actual Filler value from the property Value only,
        /// If the animation is playing, TransitoryValue will be different until it catches up.
        /// </remarks>
        public float TransitoryValue { get; private set; }

        /// <summary>
        /// If a Text component is set here it will be updated with the ProgressBar value (percentage).
        /// Otherwise no Error will be raised.
        /// </summary>
        [SerializeField]
        private Text m_AttachedText;

        /// <summary>
        /// In pixels per seconds, the speed at which the Filler will be animated.
        /// </summary>
        [Range(1,1000)]
        public int ProgressSpeed;

        /// <summary>
        /// Has the ProgressBar reached its maximal value (100%)?
        /// </summary>
        public bool isDone { get { return m_Value.AsFloat == m_FillerInfo.MaxWidth; } }
        /// <summary>
        /// Is the ProgressBar done animating?
        /// </summary>
        public bool isPaused { get { return TransitoryValue == m_Value.AsFloat; } }

        /// <summary>
        /// If true, when the ProgressBar reaches 100% the chosen method(s) will be triggered (see OnCompleteMethods).
        /// </summary>
        public bool TriggerOnComplete;
        /// <summary>
        /// The methods that you register to be triggered when the ProgressBar is complete.
        /// </summary>
        [SerializeField]
        private OnCompleteEvent OnCompleteMethods;
        
        /// <summary>
        /// By default the Filler is centered horizontally in its container panel.
        /// This value is needed for the SetInsetAndSizeFromParentEdge method.
        /// </summary>
        private float m_XOffset = float.NaN;

        public float XOffset
        {
            get
            {
                if (float.IsNaN(m_XOffset))
                    m_XOffset = (transform.GetComponent<RectTransform>().rect.width - m_FillRect.rect.width) / 2;

                return m_XOffset;
            }
        }

        void OnEnable()
        {
            //We set the Filler size to zero at the start.
            SetFillerSize(0);
            //We initialize m_Value
            m_Value = new ProgressValue(0, FillerInfo.MaxWidth);
        }

        void Update()
        {
            //If theses two values aren't equals this means m_Value has been updated and the animation needs to start.
            if (TransitoryValue != m_Value.AsFloat)
            {
                //The difference between the two values.
                float Dvalue = m_Value.AsFloat - TransitoryValue;

                //If the difference is positive:
                //  TransitoryValue needs to be incremented.
                if (Dvalue > 0)
                {
                    TransitoryValue += ProgressSpeed * Time.deltaTime;
                    if (TransitoryValue > m_Value.AsFloat)
                        TransitoryValue = m_Value.AsFloat;
                }
                //If the difference is negative:
                //  TransitoryValue needs to be decremented.
                else if (Dvalue < 0)
                {
                    TransitoryValue -= ProgressSpeed * Time.deltaTime;
                    if (TransitoryValue < m_Value.AsFloat)
                        TransitoryValue = m_Value.AsFloat;
                }

                //Clamping:
                //  If the TransitoryValue is now over the max value we set it to be equal to it.
                if (TransitoryValue >= m_FillerInfo.MaxWidth)
                    TransitoryValue = m_FillerInfo.MaxWidth;
                //  If the TransitoryValue is inferior to zero, we set it to zero
                else if (TransitoryValue < 0)
                    TransitoryValue = 0;

                //We set the Filler to be the new value
                //We don't pass the value as a percentage because we directly use SetFillerSize here which takes a width as a parameter.
                SetFillerSize(TransitoryValue);

                //If we chose to trigger a method when finished AND
                //   the animation isn't playing anymore AND
                //   the Filler is at its max value:
                //      We trigger the selected method(s).
                if (TriggerOnComplete && isPaused && isDone) OnComplete();
            }
        }

        /// <summary>
        /// This method is used to set the Filler's width
        /// </summary>
        /// <param name="Width">the new Filler's width</param>
        public void SetFillerSize(float Width)
        {
            if (m_AttachedText)
                m_AttachedText.text = Mathf.Round(Width / FillerInfo.MaxWidth * 100).ToString() + " %";

            m_FillRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, XOffset, Width);
        }

        /// <summary>
        /// This method is used to set the Filler's width with a percentage.
        /// </summary>
        /// <param name="Percent">this method needs a percentage as parameter</param>
        public void SetFillerSizeAsPercentage(float Percent)
        {
            m_Value.Set(FillerInfo.MaxWidth * Percent / 100);
            
            if (Value < 0) Value = 0;
            else if (Value > 100) Value = 100;
        }

        /// <summary>
        /// Will be triggered if TriggerOnComplete is True
        /// </summary>
        public void OnComplete()
        {
            OnCompleteMethods.Invoke();
        }
        
        /// <summary>
        /// Increment value by X percents
        /// </summary>
        /// <param name="inc">percents</param>
        public void IncrementValue(float inc)
        {
            Value += inc;

            if (Value > 100) Value = 100;
        }

        /// <summary>
        /// Decrement value by X percents
        /// </summary>
        /// <param name="inc">percents</param>
        public void DecrementValue(float dec)
        {
            Value -= dec;

            if (Value < 0) Value = 0;
        }
    }
}