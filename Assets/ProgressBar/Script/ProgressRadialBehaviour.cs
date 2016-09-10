using ProgressBar.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace ProgressBar
{
    /// <summary>
    /// This Script is directed at radially progressing designs.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class ProgressRadialBehaviour : MonoBehaviour, IIncrementable, IDecrementable
    {
        /// <summary>
        /// This script's Filler is its own Image component
        /// </summary>
        private Image m_Fill;

        /// <summary>
        /// Current value as a fraction of Filling.
        /// </summary>
        private float m_Value;
        /// <summary>
        /// Return and Set the current value as percentage
        /// </summary>
        public float Value
        {
            get
            {
                return Mathf.Round(m_Value * 100);
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
        public float ProgressSpeed;

        /// <summary>
        /// Has the ProgressBar reached its maximal value (100%)?
        /// </summary>
        public bool isDone { get { return m_Value == 1; } }
        /// <summary>
        /// Is the ProgressBar done animating?
        /// </summary>
        public bool isPaused { get { return TransitoryValue == m_Value; } }

        /// <summary>
        /// If true, when the ProgressBar reaches 100% the chosen method(s) will be triggered (see OnCompleteMethods).
        /// </summary>
        public bool TriggerOnComplete;
        /// <summary>
        /// The methods that you register to be triggered when the ProgressBar is complete.
        /// </summary>
        [SerializeField]
        private OnCompleteEvent OnCompleteMethods;
        
        void Start()
        {
            m_Fill = GetComponent<Image>();
            m_Value = 0;
            SetFillerSize(0);
        }

        void Update()
        {
            //If theses two values aren't equals this means m_Value has been updated and the animation needs to start.
            if (TransitoryValue != m_Value)
            {
                //The difference between the two values.
                float Dvalue = m_Value - TransitoryValue;

                //If the difference is positive:
                //  TransitoryValue needs to be incremented.
                if (Dvalue > 0)
                {
                    TransitoryValue += ProgressSpeed * Time.deltaTime;
                    if (TransitoryValue >= m_Value)
                        TransitoryValue = m_Value;
                }
                //If the difference is negative:
                //  TransitoryValue needs to be decremented.
                else if (Dvalue < 0)
                {
                    TransitoryValue -= ProgressSpeed * Time.deltaTime;
                    if (TransitoryValue <= m_Value)
                        TransitoryValue = m_Value;
                }

                //Clamping:
                //  If the TransitoryValue is now over the max value we set it to be equal to it.
                if (TransitoryValue >= 1)
                    TransitoryValue = 1;
                //  If the TransitoryValue is inferior to zero, we set it to zero
                else if (TransitoryValue < 0)
                    TransitoryValue = 0;

                //We set the Filler to be the new value
                //We don't pass the value as a percentage because we directly use SetFillerSize here which takes a fraction as a parameter.
                SetFillerSize(TransitoryValue);

                //If we chose to trigger a method when finished AND
                //   the animation isn't playing anymore AND
                //   the Filler is at its max value:
                //      We trigger the selected method(s).
                if (TriggerOnComplete && isPaused && isDone) OnComplete();
            }
        }


        /// <summary>
        /// This method is used to set the Fill amount
        /// </summary>
        /// <param name="fill">new Fill amount as a fraction</param>
        public void SetFillerSize(float fill)
        {
            if (m_AttachedText)
                m_AttachedText.text = Mathf.RoundToInt(fill*100).ToString() + " %";

            m_Fill.fillAmount = fill;
        }

        /// <summary>
        /// This method is used to set the Fill amount with a percentage.
        /// </summary>
        /// <param name="Percent">this method needs a percentage as parameter</param>
        public void SetFillerSizeAsPercentage(float Percent)
        {
            m_Value = Percent / 100;

            if (m_Value < 0) m_Value = 0;
            else if (m_Value > 1) m_Value = 1;
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
            m_Value += inc/100;

            if (m_Value > 1) m_Value = 1;
        }

        /// <summary>
        /// Decrement value by X percents
        /// </summary>
        /// <param name="inc">percents</param>
        public void DecrementValue(float dec)
        {
            m_Value -= dec / 100;

            if (m_Value < 0) m_Value = 0;
        }
    }
}