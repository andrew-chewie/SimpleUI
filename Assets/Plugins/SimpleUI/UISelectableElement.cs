using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace SimpleUI
{
    public class UISelectableElement : UIBaseElement
    {
        internal Action OnClick;
        internal Action OnRightClick;
        internal Action OnRemoveClick;
        internal Action<bool> OnSelectClick;

        public bool IsSelectable = true;
        public bool Selected = false;

        private bool _active = true;
        private List<UISelectableIndicator> _selectableIndicators = new List<UISelectableIndicator>();

        public override void Init()
        {
            Selected = false;
            base.Init();
        }

        public virtual void SwitchSelected()
        {
            SetSelected(!Selected);
        }

        public virtual void Click()
        {
            OnClick?.Invoke();
        }

        public virtual void RightClick()
        {
            OnRightClick?.Invoke();
        }

        public virtual void Remove()
        {
            OnRemoveClick?.Invoke();
        }

        public virtual void Select()
        {
            SetSelected(true);
        }

        public virtual void Deselect()
        {
            SetSelected(false);
        }

        public virtual void SetSelected(bool selected = true)
        {
            if(!IsSelectable)
                return;

            if (Selected != selected)
            {
                Selected = selected;
                OnSelectClick?.Invoke(Selected);

                foreach (var indicator in _selectableIndicators)
                {
                    indicator?.SetSelected(Selected);
                }

                if (selected)
                {
                    EventSystem.current.SetSelectedGameObject(gameObject);
                }
            }
        }

        public override void Destroy()
        {
            base.Destroy();
            UIEventsUnsubscribe();
            SetSelected(false);
        }

        public void UIEventsUnsubscribe()
        {
            OnClick = null;
            OnRightClick = null;
            OnRemoveClick = null;
            OnSelectClick = null;
        }
        
        public void AddIndicator(UISelectableIndicator indicator)
        {
            _selectableIndicators.Add(indicator);
        }
    }
}