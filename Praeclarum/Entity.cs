#nullable enable

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

using Newtonsoft.Json;

#if __IOS__ || __MACOS__ || __MACCATALYST__
using SceneKit;
using UIKit;
#endif

namespace Praeclarum
{
    public abstract class Entity : INotifyPropertyChanged
    {
        Dictionary<string, object>? propertySetters;

        long lastTriggeredUndoEventId;

        /// <summary>
        /// Notification that a property of this entity has changed.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Notification that this entity or one of its references has registered an undo action.
        /// By default, these notifications are ignored. However, you can create an undo manager
        /// by listening for these events and recording them.
        /// </summary>
        public event EventHandler<UndoEventArgs>? RegisteredUndo;

        /// <summary>
        /// Sets the property if it has changed and registers an undo action for it.
        /// </summary>
        protected bool SetProperty<T> (ref T backingField, T newValue, Action? action = null, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals (backingField, newValue))
                return false;

            var oldValue = backingField;
            Unbind (propertyName, oldValue, false);
            backingField = newValue;
            Bind (propertyName, newValue, false);

            action?.Invoke ();
            OnPropertyChanged (propertyName);

            return true;
        }

        /// <summary>
        /// Sets the property if it has changed and registers an undo action for it.
        /// </summary>
        protected bool SetUndoableProperty<T> (ref T backingField, T newValue, Action? action = null, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals (backingField, newValue))
                return false;

            var oldValue = backingField;
            Unbind (propertyName, oldValue, true);
            backingField = newValue;
            Bind (propertyName, newValue, true);

            RegisterUndo (propertyName, oldValue);

            action?.Invoke ();
            OnPropertyChanged (propertyName);

            return true;
        }

        protected void RegisterUndo<T> (string propertyName, T oldValue)
        {
            var ru = RegisteredUndo;
            if (ru != null) {
                var e = new UndoEventArgs ("Change {0}".Localize (propertyName.Localize ()),
                                           () => GetPropertySetter<T> (propertyName) (oldValue));
                Interlocked.Exchange (ref lastTriggeredUndoEventId, e.EventId);
                ru (this, e);
            }
        }

        void Bind (string propertyName, object? value, bool undoable)
        {
            if (value == null) return;

            if (value is Entity oe) {
                BindReference (propertyName, oe, undoable);
            }
            else if (value is INotifyCollectionChanged oce) {
                BindReferenceList (propertyName, oce, undoable);
            }
        }

        void Unbind (string propertyName, object? value, bool undoable)
        {
            if (value == null) return;

            if (value is Entity oe) {
                UnbindReference (propertyName, oe, undoable);
            }
            else if (value is INotifyCollectionChanged oce) {
                UnbindReferenceList (propertyName, oce, undoable);
            }
        }

        /// <summary>
        /// Triggers the <see cref="PropertyChanged"/> event.
        /// Override to add your own handling of property changed events.
        /// </summary>
        protected virtual void OnPropertyChanged (string propertyName)
        {
            var pc = PropertyChanged;
            if (pc != null) {
                pc (this, new PropertyChangedEventArgs (propertyName));
            }
        }

        /// <summary>
        /// Gets an action to set the named property.
        /// Calling this method is much more efficient than using reflection.
        /// </summary>
        protected Action<T> GetPropertySetter<T> (string propertyName)
        {
            if (propertySetters != null) {
                lock (propertySetters) {
                    if (propertySetters.TryGetValue (propertyName, out var o) && o is Action<T> cachedAction)
                        return cachedAction;
                }
            }

            Action<T> newAction = _ => { };
            var method = GetType ().GetProperty (propertyName)?.SetMethod;
            if (method != null) {
                newAction = (Action<T>)method.CreateDelegate (typeof (Action<T>), this);
            }
            else if (this is Praeclarum.Inspector.IHasEditInfos hei) {
                newAction = newValue => {
                    hei.SetEditValue (propertyName, newValue);
                };
            }
            else {
                throw new InvalidOperationException ($"Cannot find property setter for {GetType ().FullName}.{propertyName}");
            }

            if (propertySetters == null) {
                propertySetters = new Dictionary<string, object> ();
            }
            lock (propertySetters) {
                propertySetters[propertyName] = newAction;
            }
            return newAction;
        }

        /// <summary>
        /// Attach any event handlers and update any state needed
        /// when the given entity is referenced by this entity.
        /// The default implementation subscribes to the <see cref="RegisteredUndo"/> and <see cref="PropertyChanged"/> events.
        /// </summary>
        protected virtual void BindReference (string propertyName, Entity reference, bool undoable)
        {
            reference.PropertyChanged += OnReferencePropertyChanged;
            if (undoable)
                reference.RegisteredUndo += OnReferenceRegisteredUndo;
        }

        /// <summary>
        /// Detach any event handlers and update any state needed
        /// when the given entity is no longer referenced by the property on this entity.
        /// The default implementation unsubscribes from the <see cref="RegisteredUndo"/> and <see cref="PropertyChanged"/> events.
        /// </summary>
        protected virtual void UnbindReference (string propertyName, Entity reference, bool undoable)
        {
            reference.PropertyChanged -= OnReferencePropertyChanged;
            if (undoable)
                reference.RegisteredUndo -= OnReferenceRegisteredUndo;
        }

        void BindReferenceList (string propertyName, INotifyCollectionChanged references, bool undoable)
        {
            if (undoable)
                references.CollectionChanged += OnUndoableReferenceListChanged;
            else
                references.CollectionChanged += OnBoringReferenceListChanged;

            if (references is IEnumerable e) {
                foreach (var i in e) {
                    Bind ("", i, undoable);
                }
            }
        }

        void UnbindReferenceList (string propertyName, INotifyCollectionChanged references, bool undoable)
        {
            if (undoable)
                references.CollectionChanged -= OnUndoableReferenceListChanged;
            else
                references.CollectionChanged -= OnBoringReferenceListChanged;

            if (references is IEnumerable e) {
                foreach (var i in e) {
                    Unbind ("", i, undoable);
                }
            }
        }

        /// <summary>
        /// Event handler for when a referenced entity registers an undo action.
        /// The default implementation forwards the event using <see cref="RegisteredUndo"/>.
        /// </summary>
        void OnReferenceRegisteredUndo (object sender, UndoEventArgs e)
        {
            // Relay undo events
            var lastId = Interlocked.Exchange (ref lastTriggeredUndoEventId, e.EventId);
            if (lastId != e.EventId)
                RegisteredUndo?.Invoke (sender, e);
        }

        /// <summary>
        /// Event handler for when a referenced entity's property has changed.
        /// The default implementation does nothing.
        /// </summary>
        protected virtual void OnReferencePropertyChanged (object sender, PropertyChangedEventArgs e)
        {
        }

        /// <summary>
        /// Event handler for when the structure of a list property changes.
        /// This includes adding, removing, and updating (change the reference, not mutating) items.
        /// </summary>
        protected virtual void OnReferenceListChanged (object sender, NotifyCollectionChangedEventArgs e, bool undoable)
        {
            //
            // Unbind the old and bind the new items
            //
            if (e.OldItems != null) {
                foreach (var i in e.OldItems) {
                    Unbind ("", i, undoable);
                }
            }
            if (e.NewItems != null) {
                foreach (var i in e.NewItems) {
                    Bind ("", i, undoable);
                }
            }

            //
            // Convert collection changed events into undoable actions
            //
            if (!undoable || !(sender is IList list))
                return;

            var ru = RegisteredUndo;
            if (ru != null) {
                Action? undo = null;
                string message = "";
                switch (e.Action) {
                    case NotifyCollectionChangedAction.Add:
                        if (e.NewItems != null) {
                            undo = () => {
                                var n = e.NewItems.Count;
                                for (var i = 0; i < n; i++)
                                    list.Remove (e.NewItems[i]);
                            };
                            message = e.NewItems.Count > 1 ? "Add Many".Localize () : "Add {0}".Localize (e.NewItems[0]);
                        }
                        break;
                    case NotifyCollectionChangedAction.Move:
                        throw new NotSupportedException ("Cannot handle moving items in a reference list.");
                    case NotifyCollectionChangedAction.Remove:
                        if (e.OldItems != null) {
                            undo = () => {
                                var n = e.OldItems.Count;
                                for (var i = 0; i < n; i++)
                                    list.Insert (e.OldStartingIndex + i, e.OldItems[i]);
                            };
                            message = "Remove".Localize();
                        }
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        if (e.NewItems != null && e.OldItems != null) {
                            undo = () => {
                                var nn = e.NewItems.Count;
                                for (var i = 0; i < nn; i++)
                                    list.Remove (e.NewItems[i]);
                                var on = e.OldItems.Count;
                                for (var i = 0; i < on; i++)
                                    list.Insert (e.OldStartingIndex + i, e.OldItems[i]);
                            };
                            message = "Replace".Localize();
                        }
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        if (e.OldItems != null) {
                            undo = () => {
                                var n = e.OldItems.Count;
                                for (var i = 0; i < n; i++)
                                    list.Insert (e.OldStartingIndex + i, e.OldItems[i]);
                            };
                            message = "Reset".Localize();
                        }
                        break;
                }
                if (undo != null)
                    ru (this, new UndoEventArgs (message, undo));
            }
        }
        
        void OnUndoableReferenceListChanged (object sender, NotifyCollectionChangedEventArgs e)
        {
            OnReferenceListChanged (sender, e, true);
        }

        void OnBoringReferenceListChanged (object sender, NotifyCollectionChangedEventArgs e)
        {
            OnReferenceListChanged (sender, e, false);
        }

        public class UndoEventArgs : EventArgs
        {
            static long nextEventId = 1;

            /// <summary>
            /// A process-unique id for this undo event. This is used to prevent
            /// recursive and duplicate broadcasts of undo events for cyclic graphs.
            /// </summary>
            /// <value>The event identifier.</value>
            public long EventId { get; }

            /// <summary>
            /// Gets a summary of what caused this undo event to be emitted.
            /// </summary>
            public string Message { get; }

            /// <summary>
            /// The action to execute to undo whatever caused this event to be emitted.
            /// </summary>
            public Action UndoAction { get; }

            public UndoEventArgs (string message, Action undoAction)
            {
                EventId = Interlocked.Increment (ref nextEventId);
                Message = message;
                UndoAction = undoAction;
            }

            public override string ToString () => $"#{EventId} {Message}";
        }

        [JsonIgnore, Praeclarum.Inspector.InspectorIgnore]
        public string Json {
            get {
                try
                {
	                var settings = GetJsonSerializerSettings (GetType ().Assembly);
                    return JsonConvert.SerializeObject (this, settings);
                }
                catch (Exception ex) {
                    Log.Error ($"Failed to serialize {this}", ex);
                    return "";
                }
            }
        }

        public static JsonSerializerSettings GetJsonSerializerSettings(Assembly assembly) {
            return new JsonSerializerSettings {
                Formatting = Formatting.Indented,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                TypeNameHandling = TypeNameHandling.Auto,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                Converters = {
	                #if __MACOS__ || __IOS__ || __MACCATALYST__
                    new SceneKitConverter ()
                    #endif
                },
                SerializationBinder = new AssemblylessTypeBinder (assembly),
            };
        }

        class AssemblylessTypeBinder : Newtonsoft.Json.Serialization.ISerializationBinder
        {
	        private readonly Assembly assembly;

	        public AssemblylessTypeBinder(Assembly assembly)
	        {
		        this.assembly = assembly;
	        }

	        public void BindToName (Type serializedType, out string? assemblyName, out string typeName)
            {
                assemblyName = null;
                typeName = serializedType.FullName;
            }

            public Type BindToType (string? assemblyName, string typeName)
            {
                var t = assembly.GetType (typeName);
                if (t is null)
                {
	                t = Type.GetType (typeName);
                }
                if (t == null || t.IsAbstract) {
                    t = typeof (Praeclarum.Entity);
                }
                return t;
            }
        }

		#if __MACOS__ || __IOS__ || __MACCATALYST__
        class SceneKitConverter : JsonConverter
        {
            public override void WriteJson (JsonWriter writer, object? value, JsonSerializer serializer)
            {
                if (value is SCNMatrix4 m) {
                    serializer.Serialize (writer, new[] { m.Row0, m.Row1, m.Row2, m.Row3 });
                }
                else if (value is SCNVector4 v4) {
                    serializer.Serialize (writer, new[] { v4.X, v4.Y, v4.Z, v4.W });
                }
                else if (value is SCNVector3 v3) {
                    serializer.Serialize (writer, new[] { v3.X, v3.Y, v3.Z });
                }
                else {
                    throw new NotSupportedException (String.Format ("Cannot serialize {0} ({1})", value, value?.GetType ()));
                }
            }

            public override object? ReadJson (JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
            {
                if (objectType == typeof (SCNMatrix4)) {
                    var rows = serializer.Deserialize<SCNVector4[]> (reader);
                    if (rows == null)
                        return null;
                    return new SCNMatrix4 (rows[0], rows[1], rows[2], rows[3]);
                }
                else if (objectType == typeof (SCNVector4)) {
                    var components = serializer.Deserialize<float[]> (reader);
                    if (components == null)
                        return null;
                    return new SCNVector4 (components[0], components[1], components[2], components[3]);
                }
                else if (objectType == typeof (SCNVector3)) {
                    var components = serializer.Deserialize<float[]> (reader);
                    if (components == null)
                        return null;
                    return new SCNVector3 (components[0], components[1], components[2]);
                }
                throw new NotSupportedException (String.Format ("Cannot serialize type {0}", objectType));
            }

            public override bool CanConvert (Type objectType)
            {
                return objectType == typeof (SCNMatrix4)
                    || objectType == typeof (SCNVector4)
                    || objectType == typeof (SCNVector3);
            }
        }

        class UIKitConverter : JsonConverter
        {
            public override void WriteJson (JsonWriter writer, object? value, JsonSerializer serializer)
            {
                if (value is UIColor c) {
                    var comps = c.CGColor.Components;
                    if (comps.Length == 2) {
                        serializer.Serialize (writer, new[] { (float)comps[0], (float)comps[0], (float)comps[0], (float)comps[1] });
                    }
                    else if (comps.Length == 4) {
                        serializer.Serialize (writer, new[] { (float)comps[0], (float)comps[1], (float)comps[2], (float)comps[3] });
                    }
                    else {
                        serializer.Serialize (writer, new[] { 0.0f, 0.0f, 0.0f, 1.0f });
                    }
                }
                else {
                    throw new NotSupportedException (String.Format ("Cannot serialize {0} ({1})", value, value?.GetType ()));
                }
            }

            public override object? ReadJson (JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
            {
                if (objectType == typeof (UIColor)) {
                    var components = serializer.Deserialize<float[]> (reader);
                    if (components == null)
                        return null;
                    var a = components.Length > 3 ? components[3] : 1;
                    return UIColor.FromRGBA (components[0], components[1], components[2], a);
                }
                throw new NotSupportedException (String.Format ("Cannot serialize type {0}", objectType));
            }

            public override bool CanConvert (Type objectType)
            {
                return objectType == typeof (UIColor);
            }
        }
        #endif

        class OldMatrixConverter : JsonConverter
        {
            public override void WriteJson (JsonWriter writer, object? value, JsonSerializer serializer)
            {
                if (value is OldMatrix4 m) {
                    serializer.Serialize (writer, new[] { m.Row0, m.Row1, m.Row2, m.Row3 });
                }
                else if (value is OldVector4 v4) {
                    serializer.Serialize (writer, new[] { v4.X, v4.Y, v4.Z, v4.W });
                }
                else if (value is OldVector3 v3) {
                    serializer.Serialize (writer, new[] { v3.X, v3.Y, v3.Z });
                }
                else {
                    throw new NotSupportedException (String.Format ("Cannot serialize {0} ({1})", value, value?.GetType ()));
                }
            }

            public override object? ReadJson (JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
            {
                if (objectType == typeof (OldMatrix4)) {
                    var rows = serializer.Deserialize<OldVector4[]> (reader);
                    if (rows == null)
                        return null;
                    return new OldMatrix4 (rows[0], rows[1], rows[2], rows[3]);
                }
                else if (objectType == typeof (OldVector4)) {
                    var components = serializer.Deserialize<float[]> (reader);
                    if (components == null)
                        return null;
                    return new OldVector4 (components[0], components[1], components[2], components[3]);
                }
                else if (objectType == typeof (OldVector3)) {
                    var components = serializer.Deserialize<float[]> (reader);
                    if (components == null)
                        return null;
                    return new OldVector3 (components[0], components[1], components[2]);
                }
                throw new NotSupportedException (String.Format ("Cannot serialize type {0}", objectType));
            }

            public override bool CanConvert (Type objectType)
            {
                return objectType == typeof (OldMatrix4)
                    || objectType == typeof (OldVector4)
                    || objectType == typeof (OldVector3);
            }
        }
    }
}
