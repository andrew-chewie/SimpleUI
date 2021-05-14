namespace SimpleUI
{
    public abstract class UIBaseView: UIBaseElement
    {
        public virtual void SetValue(int value) { }
        public virtual void SetValue(float value) { }
        public virtual void SetValue(string value) { }
        public virtual void SetID(long id) { }
    }
}