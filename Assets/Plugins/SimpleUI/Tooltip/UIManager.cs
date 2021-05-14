using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace SimpleUI
{
    public class UIManager : Manager
    {
        public Camera Camera { get; set; }

        public Action<UIForm> OnFormShow;
        public Action<UIForm> OnFormHide;

        private List<UIForm> ActiveForms = new List<UIForm>();
        
        private List<GameObject> FormPrefabs;
        private List<GameObject> ElementPrefabs;

        public Canvas Canvas;
        public RectTransform CanvasRect;

        public Canvas StaticCanvas;
        public RectTransform StaticCanvasRect;

        public static string FormsPath = "Data/UI/Prefabs_UIForms_DataSet";
        public static string ElementsPath = "Data/UI/Prefabs_UIElements_DataSet";

        protected override void Init()
        {
            base.Init();
            Stopwatch timer = new Stopwatch();
            timer.Start();

            UIPrefabsDataSet formsDataSet = Resources.Load<UIPrefabsDataSet>(FormsPath);
            UIPrefabsDataSet elementsDataSet = Resources.Load<UIPrefabsDataSet>(ElementsPath);
            FormPrefabs = formsDataSet?.Prefabs;
            ElementPrefabs = elementsDataSet?.Prefabs;

            timer.Stop();
            Debug.Log("UIManager loaded - " + timer.ElapsedMilliseconds / 1000f + "s");

            InitInput();
        }

        public void SetCanvas(Canvas canvas)
        {
            if (canvas == null)
            {
                Debug.LogError("Canvas is NULL");
                return;
            }

            canvas.worldCamera = GM.UIManager.Camera;
            GM.UIManager.Canvas = canvas;
            GM.UIManager.CanvasRect = canvas.GetComponent<RectTransform>();

            CreateCoreUI();
        }

        private void CreateCoreUI()
        {
        }

        private void InitInput()
        {
        }

        public UIForm CreateForm(Type T,Transform parent = null)
        {
            return CreateForm(T,parent,null);
        }

        public UIForm CreateForm(Type T, Transform parent, UIForm parentForm)
        {
            RemoveNullForms();
            UIForm activeForm = ActiveForms.Find(x => x.GetComponent(T) != null);

            if (activeForm != null)
            {
                UIForm form = activeForm.GetComponent(T) as UIForm;
                if (form != null)
                    return form;
            }

            GameObject formPrefab = FormPrefabs.Find(x => x.GetComponent(T));

            if (formPrefab == null)
            {
                Debug.LogError("No prefab found for this form - " + T);
                return null;
            }

            GameObject formObject = null;
            var transformParent = parent == null ? Canvas?.transform : parent;

            var usePooling = formPrefab.GetComponent<UIBaseElement>()?.UsePooling ?? false;

            //POOL MANAGER LOGIC GOES HERE
            // if (usePooling && PoolController.instance != null)
            // {
            //     formObject = PoolManager.Pools["UI"].Spawn(formPrefab).gameObject;
            //     formObject.transform.SetParent(transformParent);
            //     formObject.transform.localPosition = Vector3.zero;
            //     formObject.transform.localScale = Vector3.one;
            //
            //     var objRectTransform = (RectTransform)formObject.transform;
            //     var rectTransform = (RectTransform)formPrefab.transform;
            //
            //     objRectTransform.anchorMin = rectTransform.anchorMin;
            //     objRectTransform.anchorMax = rectTransform.anchorMax;
            //     objRectTransform.anchoredPosition = rectTransform.anchoredPosition;
            //     objRectTransform.sizeDelta = rectTransform.sizeDelta;
            // }
            // else
            {
                formObject = GameObject.Instantiate(formPrefab, transformParent);
            }

            var createdForm = formObject.GetComponent(T) as UIForm;

            if (parentForm != null)
            {
                parentForm.AddChildForm(createdForm);
            }

            AddActiveForm(createdForm);
            SortForms();

            return createdForm;
        }

        public T CreateForm<T>(UIForm parentForm) where T : UIForm
        {
            return CreateForm<T>(parentForm.transform, parentForm);
        }

        public T CreateForm<T>(Transform parent = null) where T : UIForm
        {
            return CreateForm<T>(parent,null);
        }

        public T CreateForm<T>(Transform parent,UIForm parentForm) where T : UIForm
        {
            return CreateForm(typeof(T), parent, parentForm) as T;
        }

        private void RemoveNullForms()
        {
            ActiveForms.RemoveAll(form => form == null);
        }

        public void SortForms()
        {
            RemoveNullForms();

            ActiveForms = ActiveForms.OrderBy(form => form.Priority).ToList();
            for (int i = 0; i < ActiveForms.Count; i++)
            {
                ActiveForms[i].transform.SetAsLastSibling();
            }
        }

        public void HideCoreForms(UIForm exceptForm = null)
        {
            List<UIForm> coreForms = ActiveForms.FindAll(form => form.Core);

            foreach (UIForm form in coreForms)
            {
                if (form != exceptForm)
                    form.Hide();
            }
        }

        public T CreateElement<T>(Transform parent = null)
        {
            GameObject elementPrefab = ElementPrefabs.Find(x => x.GetComponent<T>() != null);
            GameObject elementObject = GameObject.Instantiate(elementPrefab, parent == null ? Canvas.transform : parent);
            elementObject.transform.localPosition = Vector3.zero;

            return elementObject.GetComponent<T>();
        }

        public T CreateElementFromPrefab<T>(GameObject prefab,Transform parent = null)
        {
            GameObject elementObject = CreateElementFromPrefab(prefab, parent);

            return elementObject.GetComponent<T>();
        }

        public GameObject CreateElementFromPrefab(GameObject prefab, Transform parent = null)
        {
            var usePooling = prefab.GetComponent<UIBaseElement>()?.UsePooling ?? false;
            GameObject elementObject;

            if (usePooling)
            {
                elementObject = GetPoolObject(prefab);
            }
            else
            {
                elementObject = GameObject.Instantiate(prefab);
            }

            elementObject.transform.SetParent(parent == null ? Canvas.transform : parent);
            elementObject.transform.localScale = Vector3.one;
            elementObject.transform.localPosition = Vector3.zero;

            return elementObject;
        }

        public GameObject CreateElementByName(string prefabName, Transform parent = null)
        {
            GameObject elementPrefab = ElementPrefabs.Find(x => x.name.ToLower() == prefabName.ToLower());

            if (elementPrefab == null)
                return null;

            GameObject elementObject = GameObject.Instantiate(elementPrefab, parent == null ? Canvas.transform : parent);
            elementObject.transform.localPosition = Vector3.zero;

            return elementObject;
        }

        private GameObject GetPoolObject(GameObject prefab)
        {
            //POOL MANAGER LOGIC GOES HERE
            // if (PoolManager.Pools.ContainsKey("UI"))
            // {
            //     return PoolManager.Pools["UI"].Spawn(prefab.transform).gameObject;
            // }

            return GameObject.Instantiate(prefab);
        }

        public T GetForm<T>() where T : UIForm
        {
            RemoveNullForms();
            var form = ActiveForms.Find(x => x.GetComponent<T>() != null);
            return form as T;
        }

        public void AddActiveForm(UIForm form)
        {
            //if(form.Embeded)
            //    return;

            if (!ActiveForms.Contains(form))
                ActiveForms.Add(form);
        }

        public void RemoveActiveForm(UIForm form)
        {
            ActiveForms.Remove(form);
            SortForms();
        }

        public void FormShow(UIForm form)
        {
            AddActiveForm(form);
            OnFormShow?.Invoke(form);
        }

        public void FormHide(UIForm form)
        {
            RemoveActiveForm(form);
            OnFormHide?.Invoke(form);
        }

        public T OpenForm<T>() where T : UIForm
        {
            CreateForm<T>().Show();
            return CreateForm<T>();
        }
        
        private bool HideTopActiveForm()
        {
            if(ActiveForms.IsNullOrEmpty())
                return false;

            var forms = ActiveForms.FindAll(form => form.UserCanClose);

            if (forms.IsNullOrEmpty())
                return false;

            var last = forms.Last();
            last.Hide();
            forms.Remove(last);

            return true;
        }
    }
}