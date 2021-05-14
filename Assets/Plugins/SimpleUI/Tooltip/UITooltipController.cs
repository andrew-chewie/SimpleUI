using UnityEngine;

namespace SimpleUI
{
    public class UITooltipController : MonoBehaviour
    {
        public static UITooltipController instance;

        private RectTransform _rect;
        public Vector2 additionalOffset = new Vector2(0f, 30f);

        public GameObject Content;
        public UIFlipAnchor FlipAnchor;
        public RectTransform ContentRect;

        public object Data { get; set; }

        private Camera _UICamera => GM.UIManager.Camera;
        private static float AppearDuration = 0.2f;

        private void OnValidate()
        {
            instance = this;
        }

        private void Start()
        {
            instance = this;

            GM.InputManager.OnPointerEnter += OnPointerEnter;
            GM.InputManager.OnPointerExit += OnPointerExit;
        }

        void Destroy()
        {
            GM.InputManager.OnPointerEnter -= OnPointerEnter;
            GM.InputManager.OnPointerExit -= OnPointerExit;
        }

        private void OnPointerExit(GameObject gameObject)
        {
            //Hide();
        }

        private void OnPointerEnter(GameObject gameObject)
        {
            if (gameObject == null)
                return;

            var hasTooltip = gameObject?.GetComponent<IHasTooltip>();
            if (hasTooltip != null)
            {
                Show(hasTooltip);
            }
        }

        private void Show(IHasTooltip hasTooltip)
        {
            hasTooltip.ShowTooltip();
        }

        public static void Show(Vector3 worldPos)
        {
            instance._rect = instance.GetComponent<RectTransform>();

            var CanvasRect = GM.UIManager.CanvasRect;

            Vector2 viewportPosition = instance._UICamera.WorldToViewportPoint(worldPos);
            var objectScreenPosition = new Vector2(
                viewportPosition.x * CanvasRect.sizeDelta.x - CanvasRect.sizeDelta.x * 0.5f,
                viewportPosition.y * CanvasRect.sizeDelta.y - CanvasRect.sizeDelta.y * 0.5f);

            Show(objectScreenPosition);
        }

        public static void Show(Vector2 screenPos)
        {
            DestroyContent();

            if (instance != null)
            {
                instance._rect = instance.GetComponent<RectTransform>();
                instance.SetPositionFromScreen(GM.InputManager.UIMousePos);
                instance.gameObject.SetActive(true);
                instance.transform.SetAsLastSibling();
            }
        }

        public void SetPositionFromWorld(Vector3 worldPosition)
        {
            var pos = RectTransformUtility.WorldToScreenPoint(_UICamera, worldPosition);
            SetPositionFromScreen(pos);
        }

        public void SetPositionFromScreen(Vector2 screenPosition)
        {
            if (_rect == null)
                return;

            if (ContentRect.pivot.x == 1)
            {
                _rect.anchoredPosition = screenPosition - additionalOffset;
                ContentRect.anchoredPosition = -additionalOffset;
            }
            else
            {
                _rect.anchoredPosition = screenPosition + additionalOffset;
                ContentRect.anchoredPosition = additionalOffset;
            }
        }

        void Update()
        {
            if (GM.InputManager.UIMousePos.x > 0)
            {
                ContentRect.anchoredPosition = -additionalOffset;
            }
            else
            {
                ContentRect.anchoredPosition = additionalOffset;
            }
        }

        internal static void SetContent(GameObject go)
        {
            DestroyContent();

            go.transform.SetParent(instance.Content.transform);
            go.transform.localScale = Vector3.one;
            go.transform.localPosition = Vector3.zero;
            instance.FlipAnchor.TargetTransform = go.transform;
        }

        private static void DestroyContent()
        {
            if (instance?.Content != null)
            {
                foreach (Transform t in instance.Content.transform)
                {
                    var baseElement = t.GetComponent<UIBaseElement>();

                    if (baseElement != null)
                    {
                        baseElement.Destroy();
                    }
                    else
                    {
                        Destroy(t?.gameObject);
                    }
                }

                instance.FlipAnchor.TargetTransform = null;
            }
        }

        public static void Hide()
        {
            if (instance == null)
                return;

            instance?.gameObject?.SetActive(false);
            DestroyContent();
        }

        public static void SetData<T>(T data)
        {
            instance.Data = data;
            instance.ShowData(data);
        }

        private void ShowData<T>(T data)
        {
            var dataView = (UIBaseDataView<T>) Content.GetComponentInChildren(typeof(UIBaseDataView<T>), true);
            FlipAnchor.TargetTransform = dataView?.transform;
            dataView?.SetData(data);
        }
    }
}