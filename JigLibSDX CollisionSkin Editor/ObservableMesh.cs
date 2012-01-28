using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using SlimDX;
using SlimDX.Direct3D9;
using MonitoredUndo;

namespace JigLibSDX_CSE
{
    public class ObservableCollisionPrimitiveInfo : CollisionPrimitiveInfo, INotifyPropertyChanged, ISupportsUndo
    {
        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raise the PropertyChanged event for the 
        /// specified property.
        /// </summary>
        /// <param name="propertyName">
        /// A string representing the name of 
        /// the property that changed.</param>
        /// <remarks>
        /// Only raise the event if the value of the property 
        /// has changed from its previous value</remarks>
        protected void OnPropertyChanged(string propertyName)
        {
            // Validate the property name in debug builds
            VerifyProperty(propertyName);

            if (null != PropertyChanged)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Verifies whether the current class provides a property with a given
        /// name. This method is only invoked in debug builds, and results in
        /// a runtime exception if the <see cref="OnPropertyChanged"/> method
        /// is being invoked with an invalid property name. This may happen if
        /// a property's name was changed but not the parameter of the property's
        /// invocation of <see cref="OnPropertyChanged"/>.
        /// </summary>
        /// <param name="propertyName">The name of the changed property.</param>
        [System.Diagnostics.Conditional("DEBUG")]
        private void VerifyProperty(string propertyName)
        {
            Type type = this.GetType();

            // Look for a *public* property with the specified name
            System.Reflection.PropertyInfo pi = type.GetProperty(propertyName);
            if (pi == null)
            {
                // There is no matching property - notify the developer
                string msg = "OnPropertyChanged was invoked with invalid " +
                                "property name {0}. {0} is not a public " +
                                "property of {1}.";
                msg = String.Format(msg, propertyName, type.FullName);
                System.Diagnostics.Debug.Fail(msg);
            }
        }

        #endregion

        #region Undo

        /// <summary>
        /// Call this when a property changes that should be tracked for undo.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="oldValue">The previous value.</param>
        /// <param name="newValue">The new value.</param>
        protected void UndoablePropertyChanging(string propertyName, object oldValue, object newValue)
        {
            DefaultChangeFactory.OnChanging(this, propertyName, oldValue, newValue);
        }

        /// <summary>
        /// Call this when the ViewModel has an ObservableCollection that changed.
        /// The method will attempt to create an undo change item for it.
        /// </summary>
        /// <param name="propertyName">The name of the property that holds the collection.</param>
        /// <param name="collection">The collection instance.</param>
        /// <param name="e">The event args raised by the INotifyCollectionChanged.CollectionChanged event.</param>
        protected void UndoableCollectionChanged(string propertyName, object collection, NotifyCollectionChangedEventArgs e)
        {
            DefaultChangeFactory.OnCollectionChanged(this, propertyName, collection, e);
        }

        public object GetUndoRoot()
        {
            return this;
        }

        #endregion
    }
}
