using UnityEngine;
using UnityEngine.UI;

namespace SimpleUI
{
    public class UIFlipAnchor : MonoBehaviour
    {
        public Transform TargetTransform;

        public bool FlipX = true;
        public bool FlipY = true;
        
        public bool UseLayoutGroupsFlip = false;
        public bool UseUpdate = false;

        private RectTransform RectTransform;

        private Rect itemRect;
        private Rect canvasRect;

        void Start()
        {
            RectTransform = GetComponent<RectTransform>();
            CheckFlip();
        }

        void OnEnable()
        {
            CheckFlip();
        }

        void OnDisable()
        {
            CheckFlip();
        }

        void Update()
        {
            if (UseUpdate)
            {
                CheckFlip();
            }
        }

        private void CheckFlip()
        {
            Vector2 corner = new Vector2(
                ((Input.mousePosition.x > (Screen.width / 2f)) ? 1f : 0f),
                ((Input.mousePosition.y > (Screen.height / 2f)) ? 1f : 0f)
            );

            if (TargetTransform == null)
                TargetTransform = this.transform;
            
            var pivot = (TargetTransform as RectTransform).pivot;

            if (FlipX)
            {
                pivot.x = corner.x;
            }

            if (FlipY)
            {
                pivot.y = corner.y;
            }

            (TargetTransform as RectTransform).pivot = pivot;

            if (UseLayoutGroupsFlip)
            {
                FlipLayoutGroup();
            }
        }

        private void FlipLayoutGroup()
        {
            var layoutGroup = TargetTransform.GetComponent<HorizontalOrVerticalLayoutGroup>();
            if (layoutGroup != null)
            {
                if (Input.mousePosition.x > Screen.width / 2f)
                {
                    switch (layoutGroup.childAlignment)
                    {
                        case TextAnchor.UpperLeft:
                            layoutGroup.childAlignment = TextAnchor.UpperRight;
                            break;
                        case TextAnchor.MiddleLeft:
                            layoutGroup.childAlignment = TextAnchor.MiddleRight;
                            break;
                        case TextAnchor.LowerLeft:
                            layoutGroup.childAlignment = TextAnchor.LowerRight;
                            break;
                    }
                }
                else
                {
                    switch (layoutGroup.childAlignment)
                    {
                        case TextAnchor.UpperRight:
                            layoutGroup.childAlignment = TextAnchor.UpperLeft;
                            break;
                        case TextAnchor.MiddleRight:
                            layoutGroup.childAlignment = TextAnchor.MiddleLeft;
                            break;
                        case TextAnchor.LowerRight:
                            layoutGroup.childAlignment = TextAnchor.LowerLeft;
                            break;
                    }
                }
            }
        }
    }
}