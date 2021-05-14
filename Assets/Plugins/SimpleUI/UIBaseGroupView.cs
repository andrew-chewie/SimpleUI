using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;

namespace SimpleUI
{
    public abstract class UIBaseGroupView<T> : UIBaseElement, IDataList
    {
        [SerializeField]
        private string OnClickAction = "Select";

        private List<string> Items => Actions.Keys.ToList();
        private Dictionary<string, Action<T>> Actions;

        public Transform Content;

        public bool CreateDefault = false;
        public bool NewOnTop = false;
        public int MaxCount = -1;
        public bool UseSubscribeToSourceDataListChanges;

        public GameObject ItemPrefab;
        public List<UIBaseDataView<T>> ViewsList = new List<UIBaseDataView<T>>();

        public Action OnRefresh { get; set; }

        internal Action<UIBaseDataView<T>> OnCreateItem;
        internal Action<T> OnClickItem;
        internal Action<T> OnRightClickItem;
        internal Action<T> OnRemoveItem;
        internal Action<List<T>> OnRemoveItems;
        internal Action<T> OnAddItem;
        internal Action<List<T>> OnAddItemsList;
        internal Action<T> OnSelectItem;
        internal Action OnCreateGroup;
        internal T SelectedData;

        internal List<T> DataList = new List<T>();

        internal Func<T, bool> CanAddDataFilter = data => true;
        internal Func<T, bool> CanRemoveDataFilter = data => true;

        internal Comparison<T> SortingComparison;
        internal HashSet<Func<T, bool>> _visibilityFilters = new HashSet<Func<T, bool>>();
        internal HashSet<Func<T, bool>> _availabilityFilters = new HashSet<Func<T, bool>>();

        internal List<T> SourceDataList = null;

        public bool IsEmpty => DataList.Count == 0;

        protected UIBaseGroupView()
        {
            Actions = new Dictionary<string, Action<T>>()
            {
                {"Click",ClickAction},
                {"Select",SelectAction},
                {"Remove",RemoveAction},
                {"NONE",null},
            };
        }

        public bool SelectOnClick = true;

        private void ClickAction(T data)
        {
            if (SelectOnClick)
            {
                SelectAction(data);
            }
        }

        private void RemoveAction(T data)
        {
            RemoveItem(data);
        }

        private void SelectAction(T data)
        {
            Select(data);
        }

        internal virtual void OnClick(T data)
        {
            if (Actions.ContainsKey(OnClickAction))
                Actions[OnClickAction]?.Invoke(data);

            OnDataClick?.Invoke(data);
        }

        public override void Init()
        {
            base.Init();
            if (CreateDefault) CreateDefaultGroup();
        }

        public virtual void Refresh()
        {
            ViewsList.RemoveAll(view => view == null);

            if (SourceDataList != null)
            {
                DataList = new List<T>(SourceDataList);
            }

            foreach (var view in ViewsList)
            {
                if (DataList.Contains(view.Data))
                {
                    view.Refresh();
                }
                else
                {
                    view.Destroy();
                }
            }

            ViewsList.RemoveAll(view => view == null || !DataList.Contains(view.Data));

            foreach (var data in DataList)
            {
                if (!ViewsList.Find(view => Compare(view.Data ,data)))
                {
                    CreateItem(data);
                }
            }
            
            OnRefresh?.Invoke();
        }

        public virtual void CreateDefaultGroup()
        {
        }

        public void RemoveAvailableByFilter(Func<T, bool> filter, bool immediate = true)
        {
            if (filter == null)
                return;

            _availabilityFilters.Remove(filter);
            _availabilityFilters.RemoveWhere(func => func == null);

            if (immediate)
                UpdateAvailabilityFilters();
        }

        public void AddAvailableByFilter(Func<T, bool> filter,bool immediate = true)
        {
            if (filter == null)
                return;

            _availabilityFilters.Add(filter);
            _availabilityFilters.RemoveWhere(func => func == null);

            if(immediate)
                UpdateAvailabilityFilters();
        }

        public HashSet<Func<T, bool>> GetAvailableFilters()
        {
            return _availabilityFilters;
        }

        public void ClearAvailabilityFilters(bool immediate = false)
        {
            _availabilityFilters.Clear();

            if(immediate)
                UpdateAvailabilityFilters();
        }
        
        public void RemoveVisibleByFilter(Func<T, bool> filter, bool immediate = true)
        {
            if (filter == null)
                return;

            _visibilityFilters.Remove(filter);
            _visibilityFilters.RemoveWhere(func => func == null);

            if (immediate)
                UpdateAvailabilityFilters();
        }

        public void AddVisibleByFilter(Func<T, bool> filter,bool immediate = false)
        {
            if (filter == null)
                return;

            _visibilityFilters.Add(filter);
            _visibilityFilters.RemoveWhere(func => func == null);

            if(immediate)
                UpdateVisibilityFilters();
        }

        public HashSet<Func<T, bool>> GetVisibilityFilters()
        {
            return _visibilityFilters;
        }

        public void ClearVisibilityFlters(bool immediate = false)
        {
            _visibilityFilters.Clear();

            if(immediate)
                UpdateAvailabilityFilters();
        }

        public List<T> GetVisibleDataList()
        {
            List<T> dataList = new List<T>();
            ViewsList.RemoveAll(view => view == null);

            foreach (var view in ViewsList)
            {
                dataList.Add(view.Data);
            }

            return dataList;
        }

        private List<T> collection;

        public virtual void CreateGroup(List<T> dataList)
        {
            if (!isInit)
                Init();

            DestroyGroup();

            //if (dataList.IsNullOrEmpty())
            //{
            //    DataList = new List<T>();
            //    return;
            //}

            Profiler.BeginSample("UIBaseGroupView - CreateGroup - " + this.name, this);

            SourceDataList = dataList;
            DataList = new List<T>(dataList);

            foreach (var data in DataList) CreateItem(data);

            SortItems();

            OnCreateGroup?.Invoke();

            if (!isDataSubscribed)
            {
                DataEventsSubscribe();
                isDataSubscribed = true;
            }

            Profiler.EndSample();
        }

        public virtual void SortItems()
        {
            Profiler.BeginSample("UIBaseGroupView - SortItems - " + this.name, this);

            if (SortingComparison != null)
            {
                DataList.Sort(SortingComparison);
            }

            ViewsList = ViewsList.OrderBy(v => DataList.IndexOf(v.Data)).ToList();

            foreach (var view in ViewsList) view.transform.SetAsLastSibling();

            Profiler.EndSample();
        }

        public virtual bool RemoveItem(T data)
        {
            var dataView = ViewsList.Find(view => Compare(view.Data, data));

            if(dataView == null)
                return false;

            Profiler.BeginSample("UIBaseGroupView - RemoveItem - " + this.name, this);

            dataView.OnRemove();
            ViewsList.Remove(dataView);
            DataList.Remove(data);

            dataView.Destroy();

            OnRemoveItem?.Invoke(data);

            Profiler.EndSample();

            return true;
        }

        public virtual void DestroyGroup()
        {
            Profiler.BeginSample("UIBaseGroupView - DestroyGroup - " + this.name, this);

            if (isDataSubscribed)
            {
                DataEventsUnsubscribe();
                isDataSubscribed = false;
            }

            try
            {
                foreach (var view in ViewsList) view?.Destroy();
            }
            catch (Exception e)
            {
                Debug.LogError($"{this}\n{e}");
            }

            DataList.Clear();
            ViewsList.Clear();

            SourceDataList = null;

            Profiler.EndSample();
        }

        public virtual UIBaseDataView<T> AddItem(T data)
        {
            if (!CanAdd(data))
                return null;

            Profiler.BeginSample("UIBaseGroupView - AddItem - " + this.name, this);

            var item = CreateItem(data);
            OnAddItem?.Invoke(data);

            Profiler.EndSample();

            return item;
        }

        public void AddItems(List<T> dataList)
        {
            if(dataList.IsNullOrEmpty())
                return;

            if (dataList.Count == 1)
            {
                AddItem(dataList[0]);
            }
            else
            {
                foreach (var data in dataList)
                {
                    var item = CreateItem(data);
                }

                OnAddItemsList?.Invoke(dataList);
            }
        }

        protected virtual UIBaseDataView<T> CreateItem(T data)
        {
            if (MaxCount != -1 && DataList.Count >= MaxCount) return null;

            if (IsItemVisible(data))
            {
                var go = CreateGameObject(data, GetPrefab(data));
                var view = go.GetComponent<UIBaseDataView<T>>();

                if (view == null)
                    Debug.LogError("No view found - " + typeof(T), this);

                AddView(data, view);

                view.SetAvailable(IsItemAvailable(data));
                view.Visible = view.gameObject.activeSelf;

                return view;
            }

            return null;
        }

        internal virtual GameObject GetPrefab(T data)
        {
            return ItemPrefab;
        }

        public virtual bool IsItemAvailable(T data)
        {
            if (data == null)
                return false;

            if (!_availabilityFilters.IsNullOrEmpty())
            {
                foreach (var filter in _availabilityFilters)
                {
                    if (filter == null)
                    {
                        continue;
                    }

                    var available = filter.Invoke(data);
                    if (!available)
                        return false;
                }
            }

            return true;
        }

        public virtual bool IsItemVisible(T data)
        {
            if (data == null)
                return false;

            if (!_visibilityFilters.IsNullOrEmpty())
            {
                foreach (var filter in _visibilityFilters)
                {
                    if (filter == null)
                    {
                        continue;
                    }

                    var visible = filter.Invoke(data);
                    if (!visible)
                        return false;
                }
            }

            return true;
        }

        public virtual void UpdateAvailabilityFilters()
        {
            foreach (var view in ViewsList)
            {
                if (_availabilityFilters.IsNullOrEmpty())
                {
                    view.SetAvailable(true);
                }
                else
                {
                    bool available = true;

                    foreach (var filter in _availabilityFilters)
                    {
                        available = filter(view.Data);

                        if (!available)
                            break;
                    }

                    view.SetAvailable(available);
                }
            }
        }

        public virtual void UpdateVisibilityFilters()
        {
            Profiler.BeginSample("UIBaseGroupView - UpdateVisibilityFilters - " + this.name, this);

            CreateGroup(new List<T>(DataList));

            Profiler.EndSample();
        }

        public UIBaseDataView<T> GetView(T data)
        {
            return ViewsList?.Find(view => Compare(view.Data, data));
        }

        public void AddView(T data, UIBaseDataView<T> view)
        {
            Profiler.BeginSample("UIBaseGroupView - AddView - " + this.name, this);

            if (view != null)
            {
                view.SetData(data);

                view.OnSelectClick += selected =>
                {
                    if (selected)
                    {
                        SelectedData = data;
                        OnSelectItem?.Invoke(view.Data);
                    }
                };
                view.OnClick += () =>
                {
                    OnClick(view.Data);
                    OnClickItem?.Invoke(view.Data);
                };
                view.OnRightClick += () => { OnRightClickItem?.Invoke(view.Data); };
                view.OnRemoveClick += () => { OnRemoveItem?.Invoke(view.Data); };
                view.OnRefresh += OnViewRefresh;

                if (DataList != null && !DataList.Contains(data))
                    DataList?.Add(data);

                ViewsList?.Add(view);

                view?.OnCreate();
                OnCreateItem?.Invoke(view);
            }

            Profiler.EndSample();
        }

        private void OnViewRefresh()
        {
            OnRefresh?.Invoke();
        }

        public virtual GameObject CreateGameObject(T data, GameObject prefab)
        {
            if (prefab == null)
                return null;

            Profiler.BeginSample("UIBaseGroupView - CreateGameObject - " + this.name, this);

            var prefabUsePooling = prefab.GetComponent<UIBaseDataView>()?.UsePooling ?? false;

            GameObject go = null;

            //POOL MANAGER LOGIC GOES HERE
            // if (prefabUsePooling && PoolController.instance != null)
            // {
            //     go = PathologicalGames.PoolManager.Pools["UI"].Spawn(prefab.transform,Vector3.zero, transform.rotation).gameObject;
            // }
            // else
            {
                go = Instantiate(prefab, Vector3.zero, transform.rotation, Content);
            }

            go.transform.SetParent(Content, false);
            go.transform.localScale = Vector3.one;
            go.transform.localPosition = Vector3.zero;

            if (NewOnTop)
            {
                go.transform.SetAsFirstSibling();
            }

            Profiler.EndSample();

            return go;
        }


        public virtual void DeselectAll()
        {
            T data = default;
            Select(data);
            OnSelectItem?.Invoke(data);
        }

        public void Select(T data)
        {
            SelectedData = data;
            ViewsList.RemoveAll(view => view == null);

            foreach (var view in ViewsList)
            {
                view.GetComponent<UIBaseDataView<T>>().SetSelected(Compare(view.Data, data));
            }
        }

        public override void Destroy()
        {
            DestroyGroup();
            base.Destroy();
            
            OnRefresh = null;
            OnAddItem = null;
            OnCreateItem = null;
            OnCreateGroup = null;

            OnClickItem = null;

            OnRemoveItem = null;
            OnRemoveItems = null;
            
            OnShow = null;
            OnHide = null;
        }

        public bool Compare<T>(T x, T y)
        {
            return EqualityComparer<T>.Default.Equals(x, y);
        }

        public bool CanBeModified()
        {
            return CanRemove(DataList[0]) && CanAdd(DataList[0]);
        }

        public bool AddData(object data)
        {
            if (data is T d)
            {
                return AddItem(d);
            }

            return false;
        }

        private bool CanAdd(T d)
        {
            var canAdd = true;

            if (CanAddDataFilter != null)
            {
                canAdd = CanAddDataFilter.Invoke(d);
            }

            return canAdd;
        }

        private bool CanRemove(T d)
        {
            var canRemove = true;

            if (CanRemoveDataFilter != null)
            {
                canRemove = CanRemoveDataFilter.Invoke(d);
            }

            return canRemove;
        }

        public bool RemoveData(object data)
        {
            if (data is T d && CanRemove(d))
            {
                return RemoveItem(d);
            }

            return false;
        }

        private bool isDataSubscribed = false;
        IDisposable _subscribeSorceChange;

        internal virtual void DataEventsUnsubscribe()
        {
            if (UseSubscribeToSourceDataListChanges)
            {
                _subscribeSorceChange?.Dispose();
            }
        }

        internal virtual void DataEventsSubscribe()
        {
            if (UseSubscribeToSourceDataListChanges)
            {
                //_subscribeSorceChange = SourceDataList?.ObserveEveryValueChanged(list => list.Count).Subscribe(count => Refresh());
            }
        }

        public void SetDataList(object dataList)
        {
            if (dataList is List<T> d)
            {
                CreateGroup(d);
            }
        }

        public object GetDataList()
        {
            return GetVisibleDataList();
        }

        public Action<object> OnDataClick { get; set; }
    }

    public interface IDataList
    {
        bool CanBeModified();
        bool AddData(object data);
        bool RemoveData(object data);
        void SetDataList(object dataList);
        object GetDataList();

        Action<object> OnDataClick { get; set; }
    }
}