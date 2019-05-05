using System;
using System.ComponentModel;

namespace dg.Utilities
{
    [AttributeUsage(AttributeTargets.Property)]
    internal sealed class HiddenForDataBindingAttribute : System.Attribute
    {
        // Fields
        private readonly bool _isHidden;

        // Methods
        public HiddenForDataBindingAttribute()
        {
        }

        public HiddenForDataBindingAttribute(bool isHidden)
        {
            this._isHidden = isHidden;
        }

        // Properties
        public bool IsHidden
        {
            get
            {
                return this._isHidden;
            }
        }
    }
    public interface IBindableCollection
    {
        // Support for Data Binding
        PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[] listAccessors);
    }
}
