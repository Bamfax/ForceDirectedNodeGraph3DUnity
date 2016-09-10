using System;
using UnityEngine.Events;

namespace ProgressBar.Utils
{
    /// <summary>
    /// Interface implementing an Incrementing Method.
    /// </summary>
    public interface IIncrementable
    {
        void IncrementValue(float inc);
    }

    /// <summary>
    /// Interface implementing a Decrementing Method.
    /// </summary>
    public interface IDecrementable
    {
        void DecrementValue(float dec);
    }

    /// <summary>
    /// Method chosen to be triggered when a ProgressBar is done.
    /// </summary>
    [Serializable]
    public class OnCompleteEvent : UnityEvent { }

    /// <summary>
    /// Min and Max Filler's width
    /// </summary>
    public class FillerProperty
    {
        public FillerProperty(float Min, float Max)
        {
            MinWidth = Min;
            MaxWidth = Max;
        }

        public float MaxWidth;
        public float MinWidth;
    }

    /// <summary>
    /// Used with linear ProgressBars.
    /// Stocks the Current and Max Filler's width
    /// </summary>
    public class ProgressValue
    {
        public ProgressValue(float value, float MaxValue)
        {
            m_Value = value;
            m_MaxValue = MaxValue;
        }

        /// <summary>
        /// Current Width
        /// </summary>
        private float m_Value;
        /// <summary>
        /// Max Width
        /// </summary>
        private float m_MaxValue;

        /// <summary>
        /// Set m_Value
        /// </summary>
        public void Set (float newValue)
        {
            m_Value = newValue;
        }

        /// <summary>
        /// Return m_Value
        /// </summary>
        public float AsFloat { get { return m_Value; } }
        /// <summary>
        /// Return m_Value as Int.
        /// </summary>
        public int AsInt { get { return (int)m_Value; } }
        /// <summary>
        /// Return m_Value as a fraction of its max Value.
        /// </summary>
        public float Normalized { get { return m_Value / m_MaxValue; } }
        /// <summary>
        /// Return m_Value as a percentage (float)
        /// </summary>
        public float PercentAsFloat { get { return Normalized * 100; } }
        /// <summary>
        /// Return m_Value as a percentage (no decimals)
        /// </summary>
        public float PercentAsInt { get { return (int)(PercentAsFloat); } }
    }
 }
