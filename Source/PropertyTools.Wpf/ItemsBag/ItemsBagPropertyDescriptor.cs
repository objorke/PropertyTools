// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ItemsBagPropertyDescriptor.cs" company="PropertyTools">
//   Copyright (c) 2014 PropertyTools contributors
// </copyright>
// <summary>
//   Provides a property descriptor for an object in the ItemsBag.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PropertyTools.Wpf
{
    using System;
    using System.ComponentModel;

    /// <summary>
    /// Provides a property descriptor for an object in the <see cref="ItemsBag" />.
    /// </summary>
    /// <remarks>
    /// In order to support "indeterminate values" of the objects in the bag (at least one object have a 
    /// value that is different from the others), this property descriptor returns nullable types for value 
    /// types. Such indeterminate values are represented by <c>null</c> values.
    /// </remarks>
    public class ItemsBagPropertyDescriptor : PropertyDescriptor
    {
        /// <summary>
        /// The component type.
        /// </summary>
        private readonly Type componentType;

        /// <summary>
        /// The default descriptor.
        /// </summary>
        private readonly PropertyDescriptor defaultDescriptor;

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemsBagPropertyDescriptor" /> class.
        /// </summary>
        /// <param name="defaultDescriptor">The default descriptor.</param>
        /// <param name="componentType">Type of the component.</param>
        public ItemsBagPropertyDescriptor(PropertyDescriptor defaultDescriptor, Type componentType)
            : base(defaultDescriptor)
        {
            this.defaultDescriptor = defaultDescriptor;
            this.componentType = componentType;
        }

        /// <summary>
        /// When overridden in a derived class, gets the type of the component this property is bound to.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// A <see cref="T:System.Type" /> that represents the type of component this property is bound to. When the <see
        /// cref="M:System.ComponentModel.PropertyDescriptor.GetValue(System.Object)" /> or <see
        /// cref="M:System.ComponentModel.PropertyDescriptor.SetValue(System.Object,System.Object)" /> methods are invoked, the object specified might be an instance of this type.
        /// </returns>
        public override Type ComponentType
        {
            get
            {
                return this.componentType;
            }
        }

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether this property is read-only.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// <c>true</c> if the property is read-only; otherwise, <c>false</c>.
        /// </returns>
        public override bool IsReadOnly
        {
            get
            {
                return this.defaultDescriptor.IsReadOnly;
            }
        }

        /// <summary>
        /// When overridden in a derived class, gets the type of the property.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// A <see cref="T:System.Type" /> that represents the type of the property.
        /// </returns>
        public override Type PropertyType
        {
            get
            {
                return this.defaultDescriptor.PropertyType;
            }
        }

        /// <summary>
        /// When overridden in a derived class, returns whether resetting an object changes its value.
        /// </summary>
        /// <param name="component">The component to test for reset capability.</param>
        /// <returns>
        /// <c>true</c> if resetting the component changes its value; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanResetValue(object component)
        {
            return false;
        }

        /// <summary>
        /// When overridden in a derived class, gets the current value of the property on a component.
        /// </summary>
        /// <param name="component">The component with the property for which to retrieve the value.</param>
        /// <returns>
        /// The value of a property for a given component.
        /// </returns>
        public override object GetValue(object component)
        {
            var bag = (ItemsBag)component;
            object value = null;
            bool isFirst = true;
            foreach (var obj in bag.Objects)
            {
                var type = obj.GetType();
                var pi = type.GetProperty(this.Name);
                if (pi == null)
                {
                    continue;
                }

                var itemValue = pi.GetValue(obj, null);
                if (value != null && !value.Equals(itemValue))
                {
                    value = null;
                }

                if (isFirst)
                {
                    value = itemValue;
                    isFirst = false;
                }
            }

            return value;
        }

        /// <summary>
        /// When overridden in a derived class, resets the value for this property of the component to the default value.
        /// </summary>
        /// <param name="component">The component with the property value that is to be reset to the default value.</param>
        public override void ResetValue(object component)
        {
        }

        /// <summary>
        /// When overridden in a derived class, sets the value of the component to a different value.
        /// </summary>
        /// <param name="component">The component with the property value that is to be set.</param>
        /// <param name="value">The new value.</param>
        public override void SetValue(object component, object value)
        {
            var bag = (ItemsBag)component;

            foreach (var obj in bag.Objects)
            {
                var type = obj.GetType();
                var pi = type.GetProperty(this.Name);
                pi.SetValue(obj, value, null);
            }

            bag.RaisePropertyChanged(this.Name);
        }

        /// <summary>
        /// When overridden in a derived class, determines a value indicating whether the value of this property needs to be persisted.
        /// </summary>
        /// <param name="component">The component with the property to be examined for persistence.</param>
        /// <returns>
        /// <c>true</c> if the property should be persisted; otherwise, <c>false</c>.
        /// </returns>
        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }
    }
}