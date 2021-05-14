namespace SimpleUI
{
    public class UISelectableIndicator : UIBaseElement
    {
        public UISelectableElement Selectable;
        public UIBaseElement Indicator;

        public override void Init()
        {
            base.Init();

            if (Selectable != null)
            {
                Selectable?.AddIndicator(this);
                Indicator?.SetVisible(Selectable.Selected, true);
            }
        }

        public void SetSelected(bool selected)
        {
            Indicator?.SetVisible(selected);
        }
    }
}