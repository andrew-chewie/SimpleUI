using System;
using UnityEngine.Profiling;

namespace SimpleUI
{
    public abstract class UIBaseDataView<T> : UIBaseDataView,IData
    {
        internal const string ViewElementsGroup = "View Elements";

        public bool ShowIfNotNull = false;
        public bool HideIfNull = false;
        public UIBaseDataView<T> ChildDataView;

        [NonSerialized]
        internal Func<T, bool> CanAddDataFilter;
        [NonSerialized]
        internal Action<UIBaseDataView<T>> OnChange;
        [NonSerialized]
        internal Action<UIBaseDataView<T>> OnSet;

        public Action OnRefresh { get; set; }
        
        private T _data;

        private bool _isTooltipSubscribed = false;

        public T Data => _data;

        private void OnReferenceSet(UIBaseDataView<T> dataView)
        {
            SetData(dataView.Data);
        }

        private void OnParentDataChange(UIBaseDataView<T> view)
        {
            SetData(view.Data);
        }

        public virtual void InitData()
        {
            if (Data == null)
            {
                //Debug.Log("No DATA set", this);
                if (HideIfNull)
                {
                    Hide(true);
                }
                return;
            }
            else
            {
                if (ShowIfNotNull)
                {
                    Show(true);
                }
            }

            ChildDataView?.SetData(Data);

            Profiler.BeginSample("BaseView - InitData - " + this.name, this);

            if (Tooltip != null && !_isTooltipSubscribed)
            {
                _isTooltipSubscribed = true;
                UIBaseTooltipTarget.OnTooltipShow += CheckTooltip;
            }

            Profiler.EndSample();
        }

        public override void Destroy()
        {
            base.Destroy();

            if (_data != null && _isDataSubscribed)
            {
                DataEventsUnsubscribe();
                _isDataSubscribed = false;
            }

            OnRefresh = null;
            OnChange = null;
            OnSet = null;

            OnClick = null;
            OnRightClick = null;
            OnRemoveClick = null;
            OnSelectClick = null;
            
            OnShow = null;
            OnHide = null;

            _isTooltipSubscribed = false;
            UIBaseTooltipTarget.OnTooltipShow -= CheckTooltip;
        }

        private void CheckTooltip(UIBaseTooltipTarget t)
        {
            if (t != Tooltip)
                return;

            OnTooltipShow(t);
        }

        public override void SetData(object data)
        {
            if (data is T d)
            {
                SetData(d);
            }
            else
            {
                //Debug.Log("SetData - Type mismatch",this);
            }
        }

        public bool SetData(T data,bool useFilters = true)
        {
            if (CanAddData(data) || !useFilters)
            {
                Profiler.BeginSample("BaseView - Set data - " + this.name, this);

                if (_isDataSubscribed)
                {
                    DataEventsUnsubscribe();
                    _isDataSubscribed = false;
                }

                _data = data;

                InitData();

                if (data != null && !_isDataSubscribed)
                {
                    DataEventsSubscribe();
                    _isDataSubscribed = true;
                }

                OnSet?.Invoke(this);

                Profiler.EndSample();

                return true;
            }

            return false;
        }

        public bool CanAddData(T data)
        {
            return CanAddDataFilter == null || CanAddDataFilter(data);
        }

        private bool _isDataSubscribed = false;

        internal virtual void DataEventsUnsubscribe(){}

        internal virtual void DataEventsSubscribe() {}

        public void ChangeData(T data)
        {
            if (SetData(data))
            {
                OnChange?.Invoke(this);
            }
        }

        public virtual void Refresh()
        {
            SetData(_data);
            OnRefresh?.Invoke();
        }


        public virtual void OnTooltipShow(UIBaseTooltipTarget t)
        {
            if(Data == null)
                return;

            Tooltip.SetData<T>(Data);
        }

        public object GetData()
        {
            return Data;
        }

        void IData.SetData(object data)
        {
            SetData(data);
        }
    }
}