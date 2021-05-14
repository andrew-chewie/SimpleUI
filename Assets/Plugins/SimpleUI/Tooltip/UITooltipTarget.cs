using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SimpleUI
{
    [DisallowMultipleComponent]
    public class UITooltipTarget : UIBaseTooltipTarget
    {
        public GameObject TooltipPrefab;
        public Transform Anchor;

        #if UNITY_EDITOR
        private void OnValidate()
        {
            if (TooltipPrefab == null)
            {
                TooltipPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Prefabs/UIElements/Tooltips/UI_Text_Tooltip.prefab");
            }
        }
        #endif

        internal override void HideTooltip()
        {
            UITooltipController.Hide();
        }

        internal override void SetData<T>(T data)
        {
            CreateTooltipContent<T>(data);
        }

        internal override void ShowTooltip()
        {
            Vector2 centerPos;

            var targetTransform = transform;

            if (Anchor != null)
                targetTransform = Anchor;

            centerPos = RectTransformToScreenSpace((RectTransform)targetTransform).center;
            centerPos.y = Screen.height - centerPos.y;

            Vector2 worldToScreenPoint = RectTransformUtility.WorldToScreenPoint(null, targetTransform.position);

            Vector2 localpoint;
            RectTransform rectTransform = (RectTransform)targetTransform;

            var canvas = GM.UIManager.Canvas;
            var rect = canvas.GetComponent<RectTransform>();
        
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, worldToScreenPoint, null, out localpoint);

            //=============================================================================
            // Calculate the screen offset
            var uiOffset = new Vector2((float)rect.sizeDelta.x / 2f, (float)rect.sizeDelta.y / 2f); 

            // Get the position on the canvas
            Vector2 ViewportPosition = GM.UIManager.Camera.WorldToViewportPoint(targetTransform.position);
            Vector2 proportionalPosition = new Vector2(ViewportPosition.x * rect.sizeDelta.x, ViewportPosition.y * rect.sizeDelta.y);

            // Set the position and remove the screen offset
            var pos = proportionalPosition - uiOffset;
            //=============================================================================

            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                pos = localpoint;
            }

            UITooltipController.Show(pos);

            TooltipShowed(this);
        }

        private GameObject _viewGameObject;

        private void CreateTooltipContent<T>(T data) 
        {
            if(TooltipPrefab.GetComponentInChildren<UIBaseDataView<T>>() == null)
                return;

            GameObject go = Instantiate(TooltipPrefab);
            UIBaseDataView<T> view = go.GetComponent<UIBaseDataView<T>>();

            if (view == null)
            {
                view = go.GetComponentInChildren<UIBaseDataView<T>>();
            }

            view?.SetData(data);
            _viewGameObject = view?.gameObject;

            UITooltipController.SetContent(go);
        }

        public GameObject GetView()
        {
            return _viewGameObject;
        }

        public static Rect RectTransformToScreenSpace(RectTransform transform)
        {
            Vector2 size = Vector2.Scale(transform.rect.size, transform.lossyScale);
            Rect rect = new Rect(transform.position.x, Screen.height - transform.position.y, size.x, size.y);
            rect.x -= (transform.pivot.x * size.x);
            rect.y -= ((1.0f - transform.pivot.y) * size.y);
            return rect;
        }
    }
}