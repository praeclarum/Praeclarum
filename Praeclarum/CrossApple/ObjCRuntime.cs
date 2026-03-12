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

        public NativeHandle (IntPtr handle)
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
}

#endif
