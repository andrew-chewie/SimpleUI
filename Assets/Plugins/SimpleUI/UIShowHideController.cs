using UnityEngine;

namespace SimpleUI
{
    public class UIShowHideController : MonoBehaviour
    {
        public GameObject ShowEffect;
        public GameObject HideEffect;

        public bool ShowOnAwake = false;
        public bool HideOnAwake = false;

        void Awake()
        {
            if (ShowOnAwake)
            {
                Show(true);
            }
            else if (HideOnAwake)
            {
                Hide(true);
            }
        }

        public void Show(bool immediate = false)
        {
            Hide(true);
            if (ShowEffect != null)
            {
                ShowEffect.GetComponent<IShow>()?.Show(immediate);
            }
        }

        public void Hide(bool immediate = false)
        {
            if (HideEffect != null)
            {
                HideEffect.GetComponent<IHide>()?.Hide(immediate);
            }
        }
    }
}

namespace SimpleUI
{
}