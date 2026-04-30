#nullable enable

using System;

#if !__IOS__ && !__MACOS__ && !__TVOS__ && !__MACCATALYST__

namespace ObjCRuntime
{
    /// <summary>A simple interface that is used to expose the unmanaged object pointer in various classes in Xamarin.iOS.</summary>
    /// <remarks>
    ///   <para>All this interface requires is for a class to expose an IntPtr that points to the unmanaged pointer to the actual object.</para>
    /// </remarks>
    public interface INativeObject
    {
        /// <summary>Handle (pointer) to the unmanaged object representation.</summary>
        /// <value>A pointer</value>
        /// <remarks>
        ///   <para>This IntPtr is a handle to the underlying unmanaged representation for this object.</para>
        /// </remarks>
        NativeHandle Handle { get; }
    }

    public readonly struct NativeHandle : IEquatable<NativeHandle>
    {
        private readonly IntPtr handle;
        public static NativeHandle Zero;

        public IntPtr Handle => handle;

        public NativeHandle(IntPtr handle)
        {
            this.handle = handle;
        }

        public static bool operator ==(NativeHandle left, IntPtr right) => left.handle == right;

        public static bool operator ==(NativeHandle left, NativeHandle right) => left.handle == right.handle;

        public static bool operator ==(IntPtr left, NativeHandle right) => left == right.handle;

        public static bool operator !=(NativeHandle left, IntPtr right) => left.handle != right;

        public static bool operator !=(IntPtr left, NativeHandle right) => left != right.handle;

        public static bool operator !=(NativeHandle left, NativeHandle right) => left.handle != right.handle;

        public static implicit operator IntPtr(NativeHandle value) => value.handle;

        public static implicit operator NativeHandle(IntPtr value) => new NativeHandle(value);

        // public static explicit operator void*(NativeHandle value) => value.handle.ToPointer();

        // public static explicit operator NativeHandle(void* value) => new NativeHandle(new IntPtr(value));

        public override bool Equals(object? o) => o is NativeHandle other && Equals(other);

        public override int GetHashCode() => handle.GetHashCode();

        public bool Equals(NativeHandle other) => handle == other.handle;

        public override string ToString() => handle.ToString();
    }

    public class Selector : IEquatable<Selector>, INativeObject
    {
        static readonly System.Collections.Generic.Dictionary<string, NativeHandle> selectorCache = new ();
        static nint nextSelectorHandle = 0x5000; // Start at a high value to avoid conflicts with actual selector handles.

        // public Selector (NativeHandle sel);

        /// <param name="name">The selector name.</param>
        /// <summary>Creates a new selector and registers it with the Objective-C runtime.</summary>
        /// <remarks></remarks>
        public Selector (string name)
        {
            Name = name;
            Handle = GetHandle(name);
        }

        /// <summary>Handle (pointer) to the unmanaged selector representation.</summary>
        /// <value>A pointer to the unmanaged selector representation.</value>
        /// <remarks>
        ///   <para>This IntPtr is the handle to the underlying unmanaged representation for this selector.</para>
        /// </remarks>
        public NativeHandle Handle { get; }

        /// <summary>Name of this selector.</summary>
        /// <value />
        /// <remarks></remarks>
        public string Name { get; }

        public static bool operator !=(Selector left, Selector right)
        {
            return !(left == right);
        }

        public static bool operator ==(Selector left, Selector right)
        {
            if (ReferenceEquals(left, right))
                return true;
            if (left is null || right is null)
                return false;
            return left.Handle == right.Handle;
        }

        /// <param name="right">The other object to compare against.</param>
        /// <summary>Compares two objects for equality</summary>
        /// <returns>True if the objects represent the same object</returns>
        /// <remarks></remarks>
        public override bool Equals(object? right)
        {
            if (right is Selector other)
                return Equals(other);
            return false;
        }

        /// <param name="right">The other selector to compare against.</param>
        /// <summary>Compares two selectors for equality.</summary>
        /// <returns>True if the objects represent the same selector.</returns>
        /// <remarks></remarks>
        public bool Equals(Selector? right)
        {
            if (ReferenceEquals(this, right))
                return true;
            if (right is null)
                return false;
            return Handle == right.Handle;
        }

        /// <summary>Returns the Selector's hash code.</summary>
        public override int GetHashCode()
        {
            return Handle.GetHashCode();
        }

        // public static Selector? FromHandle(NativeHandle sel)
        // {
        //     if (sel == NativeHandle.Zero)
        //         return null;
        //     return new Selector(sel);
        // }

        /// <summary>Creates a managed Selector instance from a native selector.</summary>
        /// <param name="selector">The native selector handle.</param>
        /// <param name="owns">Whether the caller owns the native selector handle or not.</param>
        // public static Selector? FromHandle(NativeHandle selector, bool owns) => FromHandle(selector);

        // public static Selector Register(NativeHandle handle);

        /// <summary>Returns the handle to the specified Objective-C selector.</summary>
        /// <param name="name">Name of a selector</param>
        public static IntPtr GetHandle(string name)
        {
            lock (selectorCache)
            {
                if (!selectorCache.TryGetValue(name, out var handle))
                {
                    var handleValue = nextSelectorHandle;
                    nextSelectorHandle += 1; // Increment by a large value to avoid
                    handle = new NativeHandle(handleValue);
                    selectorCache[name] = handle;
                }
                return handle;
            }
        }
    }
}

#endif
