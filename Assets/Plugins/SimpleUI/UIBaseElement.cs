using System;
using UnityEngine;
using UnityEngine.Profiling;

namespace SimpleUI
{
}

namespace SimpleUI
{
    public class UIBaseElement : MonoBehaviour
    {
        internal Action OnHide;
        internal Action OnShow;

        public bool Visible = true;

        public UIBaseElement Blocker;
        public bool UsePooling = false;
        public bool ShowOnAwake = false;
        public bool HideOnAwake = false;
        
        internal bool isInit = false;

        internal bool isVisibleSubscribed = false;

        protected UIShowHideController _showHideController;

        private Canvas _canvas;
        private bool _isDestroying = false;
        const string UIPool = "UI";

        void OnValidate()
        {
            _showHideController = GetComponent<UIShowHideController>();
            if (_showHideController != null)
            {
                
            }
            else
            {
                Visible = gameObject.activeSelf;
            }
        }

        public UIBaseElement()
        {
        }

        public void Awake()
        {
            if (!isInit)
                Init();

            if (ShowOnAwake)
            {
                Show();
            }
            else if(HideOnAwake)
            {
                Hide();
            }
        }

        /// <summary> 
        /// UI initialization, UI events subscription, Game pause
        /// </summary>
        public virtual void Init()
        {
            isInit = true;
            _showHideController = GetComponent<UIShowHideController>();
            _canvas = GetComponent<Canvas>();

            if (!isVisibleSubscribed)
                Subscribe();
        }

        public void Subscribe()
        {
            if (isVisibleSubscribed)
                return;

            SubscribeSetup();

            isVisibleSubscribed = true;
        }

        public virtual void SubscribeSetup()
        {
            
        }

        public virtual void Unsubscribe()
        {
            isVisibleSubscribed = false;
        }

        public virtual void Show(bool immediate = false)
        {
            if (gameObject == null)
                return;

            if (!isInit)
                Init();

            if (Visible)
                return;

            Profiler.BeginSample("UIBaseElement - Show - " + this.name, this);

            if (_showHideController != null)
            {
                _showHideController.Show(immediate);
            }
            else if (_canvas != null)
            {
                _canvas.enabled = true;
                gameObject.SetActive(true);
            }
            else
            {
                gameObject.SetActive(true);
            }

            Visible = true;
            OnShow?.Invoke();

            Subscribe();

            Profiler.EndSample();
        }

        public virtual void Hide()
        {
            Hide(false);
        }

        public virtual void Show()
        {
            Show(false);
        }

        public virtual void Destroy()
        {
            if (isVisibleSubscribed)
                Unsubscribe();

            if (this == null || _isDestroying)
                return;

            //POOL MANAGER LOGIC GOES HERE
            // if (PoolManager.Pools.ContainsKey(UIPool) && PoolManager.Pools[UIPool].IsSpawned(gameObject.transform))
            // {
            //     PoolManager.Pools[UIPool].Despawn(gameObject.transform);
            //     gameObject.transform.SetParent(PoolManager.Pools[UIPool].group);
            // }
            // else
            {
                _isDestroying = true;
                Destroy(gameObject);
            }
        }

        public virtual void OnDestroy()
        {
            if (!_isDestroying)
            {
                _isDestroying = true;
                Destroy();
            }
        }

        public virtual void Hide(bool immediate = false)
        {
            if (gameObject == null)
                return;

            //if (!isInit)
            //    Init();

            //if (!Visible)
            //    return;

            Profiler.BeginSample("UIBaseElement - Hide - " + this.name, this);

            if (_showHideController != null)
            {
                _showHideController.Hide(immediate);
            }
            else if (_canvas != null)
            {
                _canvas.enabled = false;
            }
            else
            {
                gameObject.SetActive(false);
            }

            Visible = false;
            OnHide?.Invoke();
            Unsubscribe();
            Profiler.EndSample();
        }

        public virtual void SwitchVisible()
        {
            if (gameObject == null)
                return;

            if (!isInit)
                Init();

            if (Visible != true)
            {
                Show();
            }
            else
            {
                Hide();
            }
        }

        public virtual void SetVisible(bool visible,bool immediate = false)
        {
            if (gameObject == null)
                return;

            if(Visible == visible)
                return;

            if (visible == true)
            {
                Show(immediate);
            }
            else
            {
                Hide(immediate);
            }
        }

        private bool _available = true;

        public virtual void SetAvailable(bool available)
        {
            _available = available;

            if(Blocker != null)
                Blocker.SetVisible(!_available);
        }
    }
}