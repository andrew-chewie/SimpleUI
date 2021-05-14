using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SimpleUI
{
    public class UIButton : UIBaseElement
    {
        [HideInInspector]
        public Button Button;
        public UIBaseView Text;
        public Button.ButtonClickedEvent onClick => Button?.onClick;

#if UNITY_EDITOR
        void OnValidate()
        {
            Init();
        }
#endif

        public void Click()
        {
            onClick?.Invoke();
        }

        public override void Init()
        {
            base.Init();
            Button = GetComponent<Button>();
            Button?.onClick?.AddListener(OnClick);
            if (Text == null)
            {
                Text = GetComponentInChildren<UITextView>();
            }
        }

        private void OnClick()
        {
            EventSystem.current?.SetSelectedGameObject(null);
        }

        public override void SetAvailable(bool available)
        {
            base.SetAvailable(available);
            if (Button != null) Button.interactable = available;
        }
    }
}