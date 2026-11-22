using System;
using UnityEngine;

namespace PromVR.ScenarioSystem._Core._ScenarioSystem.Providers
{
    /// <summary>
    /// Класс прослойка для работы граб кондишинов с ригами
    /// </summary>
    public abstract class GrabProviderBase : MonoBehaviour
    {
        public abstract bool IsLeftHandGrabbed { get; }
        public abstract bool IsRightHandGrabbed { get; }

        /// <summary>
        /// Классический граб
        /// </summary>
        public Action<Transform> OnGrabbed;
        
        /// <summary>
        /// Классическое отпускание граба
        /// </summary>
        public Action<Transform> OnReleased;
                
        /// <summary>
        /// Наведение на объект
        /// TODO на данный момент только подсвечивается хайлайтом
        /// </summary>
        public Action<Transform> OnHoverEnter;
        
        /// <summary>
        /// Уведение с объекта
        /// TODO на данный момент только подсвечивается хайлайтом
        /// </summary>
        public Action<Transform> OnHoverExit;

        private void Start()
        {
            Initialize();
        }

        private void OnDestroy()
        {
            Dispose();
        }

        protected abstract void Initialize();
        protected abstract void Dispose();
        public abstract void ForceRelease();
    }
}
