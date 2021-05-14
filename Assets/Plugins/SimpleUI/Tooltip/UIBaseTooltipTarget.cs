using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SimpleUI
{
    public abstract class UIBaseTooltipTarget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public static UIBaseTooltipTarget CurrentTooltip;
        public static Action<UIBaseTooltipTarget> OnTooltipShow;

        public bool ShowOnTop = false;
        public bool ShowAlways = false;

        public bool Active = true;

        private static float ShowDelay = 0.1f;

        internal float timer = 0f;
        internal bool StartTimer = false;

        internal abstract void SetData<T>(T data);

        internal bool LockOpen = false;
        internal bool hovering = false;

        public virtual void Start()
        {
            GM.InputManager.OnPointerEnter += PointerEnter;
            GM.InputManager.OnPointerExit += PointerExit;
        }

        private void PointerExit(GameObject gameObject)
        {
        }

        private void PointerEnter(GameObject gameObject)
        {
            if (this.gameObject == gameObject)
            {
                OnPointerEnter(null);
            }
        }

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            if (eventData != null && eventData.used)
                return;

            hovering = true;
            if (!LockOpen)
            {
                StartTimer = true;
                timer = 0f;
            }

            eventData?.Use();
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            hovering = false;
            if (!LockOpen)
            {
                StartTimer = false;
                timer = 0f;
                HideTooltip();
            }
        }

        public void TooltipShowed(UIBaseTooltipTarget tooltipTarget)
        {
            CurrentTooltip = tooltipTarget;
            if (OnTooltipShow != null)
            {
                OnTooltipShow(tooltipTarget);
            }
        }

        public void OnDisable()
        {
            CheckHide();
        }

        public void OnDestroy()
        {
            CheckHide();

            GM.InputManager.OnPointerEnter -= PointerEnter;
            GM.InputManager.OnPointerExit -= PointerExit;
        }

        private void CheckHide()
        {
            if (CurrentTooltip == this)
            {
                HideTooltip();
            }
        }

        internal virtual void HideTooltip()
        {
        }

        internal virtual void ShowTooltip()
        {
        }

        internal virtual void Update()
        {
            if (Active)
            {
                if (StartTimer && !LockOpen)
                {
                    timer += Time.deltaTime;
                    if (timer >= ShowDelay)
                    {
                        StartTimer = false;
                        timer = 0f;
                        ShowTooltip();
                    }
                }
            }
        }
    }
}