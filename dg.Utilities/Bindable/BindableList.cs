﻿using System.Collections.Generic;
using System.ComponentModel;

namespace dg.Utilities
{
    public abstract class BindableList<T> : List<T>, IBindableCollection
    {
        // Support for Data Binding
        public PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[] listAccessors)
        {
            if ((listAccessors != null) && (listAccessors.Length != 0))
            {
                return null;
            }
            return BindableCollection.GetPropertyDescriptors(this.GetType());
        }
    }
}
