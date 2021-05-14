using System.Collections.Generic;

namespace SimpleUI
{
    public class UIBaseDataView : UISelectableElement
    {
        public long ID;

        public UIBaseTooltipTarget Tooltip;

        public virtual void OnRemove()
        {
        }

        public virtual void OnCreate()
        {
        }

        public virtual void SetData(object data)
        {
        }
        
        public virtual void SetValue(int val)
        {
        
        }
        
        public virtual void SetValue(float val)
        {
        
        }
        
        public virtual void SetValue(string val)
        {
        
        }
    }
}