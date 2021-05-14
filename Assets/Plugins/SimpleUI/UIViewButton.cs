using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SimpleUI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button))]
    public class UIViewButton : UIBaseElement,IPointerClickHandler,IPointerDownHandler
    {
        public Button Button;
        public UISelectableElement UIElement;

        [SerializeField]
        private string ActionTypes = "Click";

        public bool SelectOnClick = true;

        [HideInInspector]
        public Action OnMouseLeftClick;
        [HideInInspector]
        public Action OnMouseRightClick;

#if UNITY_EDITOR
        private void Validate()
        {
            if (Button == null)
                Button = GetComponent<Button>();

            if (UIElement == null)
                UIElement = GetComponent<UISelectableElement>();
        }
#endif

        public override void Init()
        {
            base.Init();

            if (Button == null)
                Button = GetComponent<Button>();

            if (UIElement == null)
                UIElement = GetComponent<UISelectableElement>();

            Button.onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            //EventSystem.current?.SetSelectedGameObject(null);
            if (Actions.ContainsKey(ActionTypes))
                Actions[ActionTypes]?.Invoke();
        }

        public List<string> Items => Actions.Keys.ToList();

        public Dictionary<string, Action> Actions;

        public UIViewButton()
        {
            Actions = new Dictionary<string, Action>()
            {
                {"Click",Click},
                {"Select",Select},
                {"Remove",Remove},
            };
        }

        private void Select()
        {
            UIElement.Select();
        }

        private void Click()
        {
            if (SelectOnClick)
                Select();

            UIElement.Click();
        }

        private void Remove()
        {
            UIElement.Remove();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                //Button?.onClick?.Invoke();
            }
            else if (eventData.button == PointerEventData.InputButton.Middle)
            {
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                UIElement.RightClick();
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            OnPointerClick(eventData);
        }
    }
}