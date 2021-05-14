using UnityEditor;

namespace SimpleUI
{
    public static class GM
    {
        public static InputManager InputManager;
        public static UIManager UIManager;

        [InitializeOnLoadMethod]
        public static void Init()
        {
            InputManager = new InputManager();
            UIManager = new UIManager();
        }
    }
}