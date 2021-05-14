using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SimpleUI
{
    public class InputManager {
        
        public Action OnUpdate;

        public Action<KeyCode> OnKeyRelease;
        public Action<KeyCode> OnKeyPress;
        public Action<KeyCode> OnKeyHold;

        public Action<KeyCode> OnMouseClick;
        public Action<KeyCode> OnPointerClick;
        public Action<KeyCode> OnMouseHold;

        private bool _prevIsOverUI;
        public bool IsOverUI;
        public bool HoldingDown;
        public KeyCode HoldingKeyCode;

        public Action OnEnterUI { get; set; }
        public Action OnExitUI { get; set; }
        
        private StandaloneInputModuleV2 currentInput;
        public StandaloneInputModuleV2 CurrentInput
        {
            get
            {
                if (currentInput == null)
                {
                    currentInput = EventSystem.current.currentInputModule as StandaloneInputModuleV2;
                    if (currentInput == null)
                    {
                        Debug.Log("Missing StandaloneInputModuleV2.");
                    }
                }

                return currentInput;
            }
        }

        public InputManager()
        {
            Init();
        }

        private void Init()
        {
        }

        bool isSubscribed = false;

        public void SubscribeToInput()
        {
            if (!isSubscribed)
            {
                //EventSystem.current.OnPointerEnterAsObservable().Subscribe(data => OnPointerEnter(data.pointerEnter));
                //EventSystem.current.OnPointerExitAsObservable().Subscribe(data => OnPointerExit(data.pointerEnter));

                isSubscribed = true;
            }
        }

        public Action<GameObject> OnPointerEnter;
        public Action<GameObject> OnPointerExit;

        private void OnPointerExited(GameObject gameObject)
        {
            OnPointerExit?.Invoke(gameObject);
        }

        private void OnPointerEntered(GameObject gameObject)
        {
            OnPointerEnter?.Invoke(gameObject);
        }

        public void Update()
        {
            SubscribeToInput();

            if (EventSystem.current.IsPointerOverGameObject())
            {
                IsOverUI = true;
            }
            else
            {
                IsOverUI = false;
            }

            if (_prevIsOverUI != IsOverUI)
            {
                if (IsOverUI)
                {
                    OnEnterUI?.Invoke();
                }
                else
                {
                    OnExitUI?.Invoke();
                }

                _prevIsOverUI = IsOverUI;
            }

            if (Input.anyKeyDown)
            {
                foreach (KeyCode kcode in Enum.GetValues(typeof(KeyCode)))
                {
                    if (Input.GetKeyDown(kcode))
                    {
                        OnKeyPress?.Invoke(kcode);
                    }
                }
            }

            if (Input.anyKey)
            {
                foreach (KeyCode kcode in Enum.GetValues(typeof(KeyCode)))
                {
                    if (Input.GetKey(kcode))
                    {
                        HoldingKeyCode = kcode;
                        HoldingDown = true;
                        OnKeyHold?.Invoke(kcode);
                    }
                }
            }

            if (!Input.anyKey && HoldingDown)
            {
                OnKeyRelease?.Invoke(HoldingKeyCode);
                HoldingKeyCode = KeyCode.None;
                HoldingDown = false;
            }

            // Check if the left mouse button was clicked
            if (Input.GetMouseButtonDown(0) && !IsOverUI)
            {
                OnMouseClick?.Invoke(KeyCode.Mouse0);
            }
            
            if (Input.GetMouseButton(0) && !IsOverUI)
            {
                OnMouseHold?.Invoke(KeyCode.Mouse0);
            }

            if (Input.GetMouseButtonDown(1) && !IsOverUI)
            {
                OnMouseClick?.Invoke(KeyCode.Mouse1);
            }

            if (Input.GetMouseButton(1) && !IsOverUI)
            {
                OnMouseHold?.Invoke(KeyCode.Mouse1);
            }
            
            if (Input.GetMouseButtonDown(0) && IsOverUI)
            {
                OnPointerClick?.Invoke(KeyCode.Mouse0);
            }
            
            if (Input.GetMouseButtonDown(1) && IsOverUI)
            {
                OnPointerClick?.Invoke(KeyCode.Mouse1);
            }

            OnUpdate?.Invoke();
        }

        public bool IsHolding(KeyCode keyCode)
        {
            return Input.GetKey(keyCode);
        }
        
        public Vector2 UIMousePos => ScreenToCanvas(Input.mousePosition);
        public Canvas Canvas => GM.UIManager.Canvas;
        
        public Vector3 ScreenToCanvas(Vector3 screenPosition)
        {
            Vector2 localPosition;
            Vector2 min;
            Vector2 max;
            var rectTransform = (RectTransform)Canvas.transform;
            var canvasSize = rectTransform.sizeDelta;

            if (Canvas.renderMode == RenderMode.ScreenSpaceOverlay || (Canvas.renderMode == RenderMode.ScreenSpaceCamera && Canvas.worldCamera == null))
            {
                localPosition = screenPosition;

                min = Vector2.zero;
                max = canvasSize;
            }
            else
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPosition,
                    Canvas.worldCamera, out localPosition);
            }

            return localPosition;
        }
    }
    
    public class StandaloneInputModuleV2 : StandaloneInputModule
    {
        public GameObject GameObjectUnderPointer(int pointerId)
        {
            var lastPointer = GetLastPointerEventData(pointerId);
            if (lastPointer != null)
                return lastPointer.pointerCurrentRaycast.gameObject;
            return null;
        }
 
        public GameObject GameObjectUnderPointer()
        {
            return GameObjectUnderPointer(PointerInputModule.kMouseLeftId);
        }
    }
}