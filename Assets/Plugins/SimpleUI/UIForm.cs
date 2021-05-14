using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace SimpleUI
{
    using UnityEngine;

    [Serializable]
    public abstract class UIForm : UIBaseElement
    {
        public long ID;

        public bool Core = false;
        public bool FitToContainer = false;
        public bool Embedded = false;
        public bool UserCanClose = true;
        public bool DestroyOnHide = true;
        public bool HideIfClickOutside = false;
        public bool DisableHotkeys = false;
        public int Priority = 0;

            
        internal List<UIForm> ChildForms = new List<UIForm>();

        public RectTransform RectTransform { get; private set; }

        private UIForm _parentForm;
        
        public override void Init()
        {
            base.Init();

            RectTransform = GetComponent<RectTransform>();
            
            GM.InputManager.OnKeyPress += OnKeyPress;
        }

        private void OnKeyPress(KeyCode keyCode)
        {
            if (keyCode == KeyCode.Mouse0
                || keyCode == KeyCode.Mouse1)
            {
                CheckHide();
            }
        }

        private void CheckHide()
        {
            if (this == null)
            {
                Debug.LogError("Form is null - " + name);
                return;
            }
                
            if (HideIfClickOutside)
            {
                GameObject objectUnderPointer = GM.InputManager.CurrentInput.GameObjectUnderPointer();

                if (EventSystem.current.IsPointerOverGameObject() && objectUnderPointer != null)
                {
                    if (!objectUnderPointer.transform.IsChildOf(gameObject.transform))
                    {
                        Hide();
                    }
                }

                else if (!RectTransformUtility.RectangleContainsScreenPoint(this.GetComponent<RectTransform>(),Input.mousePosition,Camera.main))
                {
                    Hide();
                }
            }
        }

        public virtual void Show(RectTransform parent = null)
        {
            Show(false);

            if (parent != null)
            {
                transform.SetParent(parent);
                transform.localScale = Vector3.one;
                gameObject.layer = parent.gameObject.layer;
                
                if (RectTransform.anchorMin == Vector2.zero && RectTransform.anchorMax == Vector2.one)
                {
                    RectTransform.offsetMin = Vector2.zero;
                    RectTransform.offsetMax = Vector2.zero;
                }

                if (FitToContainer)
                {
                    RectTransform.anchorMin = Vector2.zero;
                    RectTransform.anchorMax = Vector2.one;
                    
                    RectTransform.offsetMin= Vector2.zero;
                    RectTransform.offsetMax = Vector2.zero;
                }

                var form = parent.GetComponent<UIForm>();

                if (form != null)
                {
                    form.AddChildForm(this);
                }
            }
        }

        public override void Show(bool immediate = false)
        {
            base.Show(immediate);

            _showHideController?.Show();

            if (Core)
            {
                GM.UIManager.HideCoreForms(this);
            }

            GM.UIManager.FormShow(this);
        }

        public void AddChildForm(UIForm form)
        {
            form.Embedded = true;
            form._parentForm = this;

            ChildForms.RemoveAll(f => f == null);
            ChildForms.Add(form);
        }

        public override void Hide()
        {
            base.Hide();

            if (this == null || gameObject == null)
                return;

            _showHideController?.Hide();

            ChildForms.RemoveAll(f => f == null);
            foreach (UIForm form in ChildForms)
            {
                form?.Hide();
            }

            ChildForms.Clear();

            GM.UIManager.FormHide(this);

            if (DestroyOnHide)
            {
                Destroy();
            }
        }

        public virtual void SetPos(Vector2 pos)
        {
            //pos -= GM.UIManager.Canvas.pixelRect.size / 2f;
            RectTransform.localPosition = pos;
        }

        public void SetSortingPriority(int priority)
        {
            Priority = priority;
            GM.UIManager.SortForms();
        }

        public override void Destroy()
        {
            base.Destroy();

            GM.InputManager.OnKeyPress -= OnKeyPress;
        }
    }
}