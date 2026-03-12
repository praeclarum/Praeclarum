#nullable enable

using System;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Runtime.InteropServices;
// using System.Runtime.InteropServices.ObjectiveC;
using System.Runtime.Versioning;

#if !__IOS__ && !__MACOS__ && !__TVOS__ && !__MACCATALYST__

using ObjCRuntime;

namespace Foundation
{
    /// <summary>Exports a method or property to the Objective-C world.</summary>
    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property)]
    public class ExportAttribute : Attribute
    {
        /// <summary>Use this method to expose a C# method, property or constructor as a method that can be invoked from Objective-C.</summary>
        // protected ExportAttribute();

        /// <summary>Exports the given method or property to Objective-C land with the specified method name.</summary>
        /// <param name="selector">The selector name.</param>
        public ExportAttribute(string? selector) {
            Selector = selector;
        }

        /// <summary>Use this method to expose a C# method, property or constructor as a method that can be invoked from Objective-C.</summary>
        /// <param name="selector">The selector name.</param>
        /// <param name="semantic">The semantics of the setter (Assign, Copy or Retain).</param>
        // public ExportAttribute(string? selector, ArgumentSemantic semantic);

        /// <summary>The name of the C# selector if specified, or null if it is derived from the property name or method.</summary>
        public string? Selector { get; set; }

        /// <summary>The semantics for object ownership on setter properties or methods.</summary>
        /// <value>The assignment ownership semantics for setting the value.</value>
        // public ArgumentSemantic ArgumentSemantic { get; set; }

        // public bool IsVariadic { get; set; }

        // public ExportAttribute ToGetter(PropertyInfo prop);

        // public ExportAttribute ToSetter(PropertyInfo prop);
    }

    /// <summary>This interface represents the Objective-C protocol <c>NSCoding</c>.</summary>
    /// <remarks>
    ///   <para>A class that implements this interface (and subclasses <see cref="T:Foundation.NSObject" />) will be exported to Objective-C as implementing the Objective-C protocol this interface represents.</para>
    ///   <para>A class may also implement members from this interface to implement members from the protocol.</para>
    /// </remarks>
    public interface INSCoding : INativeObject, IDisposable
    {
        static T? CreateInstance<T>(NSCoder decoder) where T : NSObject, INSCoding;

        /// <summary>Encodes the state of the object using the provided encoder.</summary>
        /// <param name="encoder">The encoder object where the state of the object will be stored</param>
        [Export("encodeWithCoder:")]
        void EncodeTo(NSCoder encoder);
    }

    /// <summary>This interface represents the Objective-C protocol <c>NSCopying</c>.</summary>
    [Protocol(Name = "NSCopying", WrapperType = typeof(NSCopyingWrapper))]
    [ProtocolMember(IsRequired = true, IsProperty = false, IsStatic = false, Name = "Copy", Selector = "copyWithZone:", ReturnType = typeof(NSObject), ParameterType = new Type[] { typeof(NSZone) }, ParameterByRef = new bool[] { false })]
    public interface INSCopying : INativeObject, IDisposable
    {
        /// <param name="zone">Developers should pass <see langword="null" />.  Memory zones are no longer used.</param>
        /// <summary>Performs a copy of the underlying Objective-C object.</summary>
        /// <returns>The newly-allocated object.</returns>
        /// <remarks>
        ///   <para>This method performs a "shallow copy" of <see langword="this" />. If this object contains references to external objects, the new object will contain references to the same object.</para>
        /// </remarks>
        [RequiredMember]
        [Export("copyWithZone:")]
        [BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
        [return: Release]
        NSObject Copy(NSZone? zone);
    }

    /// <summary>This interface represents the Objective-C protocol <c>NSItemProviderReading</c>.</summary>
    /// <remarks>
    ///   <para>A class that implements this interface (and subclasses <see cref="T:Foundation.NSObject" />) will be exported to Objective-C as implementing the Objective-C protocol this interface represents.</para>
    ///   <para>A class may also implement members from this interface to implement members from the protocol.</para>
    /// </remarks>
    [SupportedOSPlatform("maccatalyst")]
    [SupportedOSPlatform("ios")]
    [SupportedOSPlatform("macos")]
    [SupportedOSPlatform("tvos")]
    [Protocol(Name = "NSItemProviderReading", WrapperType = typeof(NSItemProviderReadingWrapper))]
    [ProtocolMember(IsRequired = true, IsProperty = false, IsStatic = true, Name = "GetObject", Selector = "objectWithItemProviderData:typeIdentifier:error:", ReturnType = typeof(INSItemProviderReading), ParameterType = new Type[] { typeof(NSData), typeof(string), typeof(NSError) }, ParameterByRef = new bool[] { false, false, true })]
    [ProtocolMember(IsRequired = true, IsProperty = true, IsStatic = true, Name = "ReadableTypeIdentifiers", Selector = "readableTypeIdentifiersForItemProvider", PropertyType = typeof(string[]), GetterSelector = "readableTypeIdentifiersForItemProvider", ArgumentSemantic = ArgumentSemantic.Copy)]
    public interface INSItemProviderReading : INativeObject, IDisposable
    {
        /// <param name="data">To be added.</param>
        /// <param name="typeIdentifier">To be added.</param>
        /// <param name="outError">To be added.</param>
        /// <summary>To be added.</summary>
        /// <returns>To be added.</returns>
        /// <remarks>To be added.</remarks>
        [RequiredMember]
        [Export("objectWithItemProviderData:typeIdentifier:error:")]
        [BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
        static INSItemProviderReading? GetObject<T>(
          NSData data,
          string typeIdentifier,
          out NSError? outError)
          where T : NSObject, INSItemProviderReading;

        [RequiredMember]
        [BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
        static string[] GetReadableTypeIdentifiers<T>() where T : NSObject, INSItemProviderReading;
    }
    /// <summary>Interface used by <see cref="T:Foundation.NSItemProvider" /> for retrieving data from an object.</summary>
    /// <remarks>To be added.</remarks>
    [SupportedOSPlatform("maccatalyst")]
    [SupportedOSPlatform("ios")]
    [SupportedOSPlatform("macos")]
    [SupportedOSPlatform("tvos")]
    [Protocol(Name = "NSItemProviderWriting", WrapperType = typeof(NSItemProviderWritingWrapper))]
    [ProtocolMember(IsRequired = false, IsProperty = false, IsStatic = false, Name = "GetItemProviderVisibilityForTypeIdentifier", Selector = "itemProviderVisibilityForRepresentationWithTypeIdentifier:", ReturnType = typeof(NSItemProviderRepresentationVisibility), ParameterType = new Type[] { typeof(string) }, ParameterByRef = new bool[] { false })]
    [ProtocolMember(IsRequired = true, IsProperty = false, IsStatic = false, Name = "LoadData", Selector = "loadDataWithTypeIdentifier:forItemProviderCompletionHandler:", ReturnType = typeof(NSProgress), ParameterType = new Type[] { typeof(string), typeof(Action<NSData, NSError>) }, ParameterByRef = new bool[] { false, false }, ParameterBlockProxy = new Type[] { null, typeof(Trampolines.NIDActionArity2V19) })]
    [ProtocolMember(IsRequired = true, IsProperty = true, IsStatic = true, Name = "WritableTypeIdentifiers", Selector = "writableTypeIdentifiersForItemProvider", PropertyType = typeof(string[]), GetterSelector = "writableTypeIdentifiersForItemProvider", ArgumentSemantic = ArgumentSemantic.Copy)]
    [ProtocolMember(IsRequired = false, IsProperty = true, IsStatic = false, Name = "WritableTypeIdentifiersForItemProvider", Selector = "writableTypeIdentifiersForItemProvider", PropertyType = typeof(string[]), GetterSelector = "writableTypeIdentifiersForItemProvider", ArgumentSemantic = ArgumentSemantic.Copy)]
    public interface INSItemProviderWriting : INativeObject, IDisposable
    {
        /// <param name="typeIdentifier">To be added.</param>
        /// <summary>To be added.</summary>
        /// <returns>To be added.</returns>
        /// <remarks>To be added.</remarks>
        [OptionalMember]
        [Export("itemProviderVisibilityForRepresentationWithTypeIdentifier:")]
        [BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
        NSItemProviderRepresentationVisibility GetItemProviderVisibilityForTypeIdentifier(
          string typeIdentifier);

        /// <param name="typeIdentifier">A Universal Type Identifier (UTI) indicating the type of data to load.</param>
        /// <param name="completionHandler">The method called after the data is loaded.</param>
        /// <summary>Implement this method to customize the loading of data by an <see cref="T:Foundation.NSItemProvider" />.</summary>
        /// <returns>An <see cref="T:Foundation.NSProgress" /> object reflecting the data-loading operation.</returns>
        /// <remarks>
        ///   <para>The <paramref name="typeIdentifier" /> must be in the set of values returned by <see cref="M:Foundation.NSItemProviderWriting_Extensions.GetWritableTypeIdentifiersForItemProvider(Foundation.INSItemProviderWriting)" />.</para>
        /// </remarks>
        [RequiredMember]
        [Export("loadDataWithTypeIdentifier:forItemProviderCompletionHandler:")]
        [BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
        NSProgress? LoadData(string typeIdentifier, [BlockProxy(typeof(Trampolines.NIDActionArity2V19))] Action<NSData, NSError> completionHandler);

        /// <param name="typeIdentifier">A Universal Type Identifier (UTI) indicating the type of data to load.</param>
        /// <summary>Asynchronously loads data for the identified type from an item provider, returning a task that contains the data.</summary>
        /// <returns>To be added.</returns>
        /// <remarks>To be added.</remarks>
        [BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
        Task<NSData> LoadDataAsync(string typeIdentifier);

        [BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
        Task<NSData> LoadDataAsync(string typeIdentifier, out NSProgress result);

        /// <summary>To be added.</summary>
        /// <value>To be added.</value>
        /// <remarks>To be added.</remarks>
        [BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
        [OptionalMember]
        string[] WritableTypeIdentifiersForItemProvider { [Export("writableTypeIdentifiersForItemProvider", ArgumentSemantic.Copy)] get; }

        [RequiredMember]
        [BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
        static string[] GetWritableTypeIdentifiers<T>() where T : NSObject, INSItemProviderWriting;
    }

    /// <summary>This interface represents the Objective-C protocol <c>NSMutableCopying</c>.</summary>
    /// <remarks>
    ///   <para>A class that implements this interface (and subclasses <see cref="T:Foundation.NSObject" />) will be exported to Objective-C as implementing the Objective-C protocol this interface represents.</para>
    ///   <para>A class may also implement members from this interface to implement members from the protocol.</para>
    /// </remarks>
    [Protocol(Name = "NSMutableCopying", WrapperType = typeof(NSMutableCopyingWrapper))]
    [ProtocolMember(IsRequired = true, IsProperty = false, IsStatic = false, Name = "MutableCopy", Selector = "mutableCopyWithZone:", ReturnType = typeof(NSObject), ParameterType = new Type[] { typeof(NSZone) }, ParameterByRef = new bool[] { false })]
    public interface INSMutableCopying : INativeObject, IDisposable, INSCopying
    {
        /// <param name="zone">Zone to use to allocate this object, or null to use the default zone.</param>
        /// <summary>To be added.</summary>
        /// <returns>To be added.</returns>
        /// <remarks>To be added.</remarks>
        [RequiredMember]
        [Export("mutableCopyWithZone:")]
        [BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
        [return: Release]
        NSObject MutableCopy(NSZone? zone);
    }

    /// <summary>The secure coding category.</summary>
    /// <remarks>To be added.</remarks>
    [Protocol(Name = "NSSecureCoding", WrapperType = typeof(NSSecureCodingWrapper))]
    public interface INSSecureCoding : INativeObject, IDisposable, INSCoding
    {
    }


    /// <summary>Base class for all bound objects that map to Objective-C objects.</summary>
    [ObjectiveCTrackedType]
    [SupportedOSPlatform("ios")]
    [SupportedOSPlatform("maccatalyst")]
    [SupportedOSPlatform("macos")]
    [SupportedOSPlatform("tvos")]
    [Register("NSObject", true)]
    [StructLayout(LayoutKind.Sequential)]
    public class NSObject :
      INativeObject,
      IEquatable<
#nullable disable
      NSObject>,
      IDisposable
    //   INSObjectFactory,
    //   INSObjectProtocol
    {
        // public static readonly Assembly PlatformAssembly;

        /// <appledoc>https://developer.apple.com/documentation/foundation/process/init()</appledoc>
        [Export("init")]
        public NSObject();

        /// <summary>Constructor to call on derived classes to skip initialization and merely allocate the object.</summary>
        /// <param name="x">Unused sentinel value, pass NSObjectFlag.Empty.</param>
        public NSObject(NSObjectFlag x);

        ~NSObject();
        public void Dispose();

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected internal NSObject(NativeHandle handle);

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected NSObject(NativeHandle handle, bool alloced);

        /// <summary>Generates a hash code for the current instance.</summary>
        /// <returns>A int containing the hash code for this instance.</returns>
        public override int GetHashCode();

        public override bool Equals(object obj);

        public bool Equals(NSObject obj);

        /// <summary>Returns a string representation of the value of the current instance.</summary>
        public override string ToString();

        /// <summary>Releases the resources used by the NSObject object.</summary>
        /// <param name="disposing">
        ///   <para>If set to <see langword="true" />, the method is invoked directly and will dispose managed and unmanaged resources;   If set to <see langword="false" /> the method is being called by the garbage collector finalizer and should only release unmanaged resources.</para>
        /// </param>
        protected virtual void Dispose(bool disposing);

#if MORE_NSOBJECT
        /// <summary>Promotes a regular peer object (IsDirectBinding is true) into a toggleref object.</summary>
        /// <remarks>
        ///   This turns a regular peer object (one that has
        ///   IsDirectBinding set to true) into a toggleref object.  This
        ///   is necessary when you are storing to a backing field whose
        ///   objc_c semantics is not copy or retain.  This is an internal
        ///   method.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected void MarkDirty();

        [Export("conformsToProtocol:")]
        [Preserve]
        [BindingImpl(BindingImplOptions.Optimizable)]
        public virtual bool ConformsToProtocol(NativeHandle protocol);

        /// <summary>Calls the 'release' selector on this object.</summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public void DangerousRelease();

        /// <summary>Calls the 'retain' selector on this object.</summary>
        /// <returns>This object.</returns>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public NSObject DangerousRetain();

        /// <summary>Calls the 'autorelease' selector on this object.</summary>
        /// <returns>This object.</returns>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public NSObject DangerousAutorelease();

        /// <summary>Handle used to represent the methods in the base class for this NSObject.</summary>
        /// <value>An opaque pointer, represents an Objective-C objc_super object pointing to our base class.</value>
        /// <remarks>
        ///   This property is used to access members of a base class.
        ///   This is typically used when you call any of the Messaging
        ///   methods to invoke methods that were implemented in your base
        ///   class, instead of invoking the implementation in the current
        ///   class.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public NativeHandle SuperHandle { get; }

        /// <summary>Handle (pointer) to the unmanaged object representation.</summary>
        /// <value>A pointer</value>
        /// <remarks>This IntPtr is a handle to the underlying unmanaged representation for this object.</remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public NativeHandle Handle { get; set; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected void InitializeHandle(NativeHandle handle);

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected void InitializeHandle(NativeHandle handle, string initSelector);

        /// <param name="sel">Selector to invoke</param>
        /// <param name="obj">Object in which the selector is invoked</param>
        /// <summary>Invokes asynchrously the specified code on the main UI thread.</summary>
        /// <remarks>
        ///   <para>
        ///       	    You use this method from a thread to invoke the code in
        ///       	    the specified object that is exposed with the specified
        ///       	    selector in the UI thread.  This is required for most
        ///       	    operations that affect UIKit or AppKit as neither one of
        ///       	    those APIs is thread safe.
        ///       	  </para>
        ///   <para>
        ///       	    The code is executed when the main thread goes back to its
        ///       	    main loop for processing events.
        ///       	  </para>
        ///   <para>
        ///       	    Unlike <see cref="M:Foundation.NSObject.InvokeOnMainThread(ObjCRuntime.Selector,Foundation.NSObject)" />
        ///       	    this method merely queues the invocation and returns
        ///       	    immediately to the caller.
        ///       	  </para>
        /// </remarks>
        public void BeginInvokeOnMainThread(Selector sel, NSObject obj);

        /// <param name="sel">Selector to invoke</param>
        /// <param name="obj">Object in which the selector is invoked</param>
        /// <summary>Invokes synchrously the specified code on the main UI thread.</summary>
        /// <remarks>
        ///   <para>
        ///       	    You use this method from a thread to invoke the code in
        ///       	    the specified object that is exposed with the specified
        ///       	    selector in the UI thread.  This is required for most
        ///       	    operations that affect UIKit or AppKit as neither one of
        ///       	    those APIs is thread safe.
        ///       	  </para>
        ///   <para>
        ///       	    The code is executed when the main thread goes back to its
        ///       	    main loop for processing events.
        ///       	  </para>
        ///   <para>
        ///       	    Unlike <see cref="M:Foundation.NSObject.BeginInvokeOnMainThread(ObjCRuntime.Selector,Foundation.NSObject)" />
        ///       	    this method waits for the main thread to execute the method, and does not return until the code pointed by action has completed.
        ///       	  </para>
        /// </remarks>
        public void InvokeOnMainThread(Selector sel, NSObject obj);

        /// <param name="action">To be added.</param>
        /// <summary>To be added.</summary>
        /// <remarks>To be added.</remarks>
        public void BeginInvokeOnMainThread(Action action);

        /// <param name="action">To be added.</param>
        /// <summary>To be added.</summary>
        /// <remarks>To be added.</remarks>
        public void InvokeOnMainThread(Action action);

        /// <param name="obj">A ECMA CLI object.</param>
        /// <summary>Boxes an object into an NSObject.</summary>
        /// <returns>Boxed object or null if the type can not be boxed.</returns>
        public static NSObject FromObject(object obj);

        public void SetValueForKeyPath(NativeHandle handle, NSString keyPath);

        /// <param name="action">To be added.</param>
        /// <param name="delay">To be added.</param>
        public virtual void Invoke(Action action, double delay);

        public virtual void Invoke(Action action, TimeSpan delay);


        /// <param name="key">
        /// 
        /// 
        /// Key-path to use to perform the value lookup. The keypath consists of a series of lowercase ASCII-strings with no spaces in them separated by dot characters.
        /// 
        /// 
        ///   	   </param>
        /// <param name="options">
        /// 
        /// 
        /// Flags indicating which notifications you are interested in receiving (New, Old, Initial, Prior).
        /// 
        /// 
        ///   	   </param>
        /// <param name="observer">
        /// 
        /// 
        /// Method that will receive the observed changes.   It will receive a <see cref="T:Foundation.NSObservedChange" /> parameter with the information that was changed.
        /// 
        /// 
        ///   	   </param>
        /// <summary>Registers an object for being observed externally using an arbitrary method.</summary>
        /// <returns>
        /// 
        /// An IDisposable object.  Invoke the Dispose method on this object to remove the observer.
        /// 
        ///      </returns>
        /// <remarks>
        ///         <para>When the object is registered for observation, changes to the object specified in the keyPath that match the flags requested in options will be sent to the specied method (a lambda or method that matches the signature).</para>
        ///         <para>This version provides the convenience of exposing the changes as part of the strongly typed <see cref="T:Foundation.NSObservedChange" /> object that is received by the target.</para>
        ///         <para></para>
        ///         <example>
        ///           <code lang="csharp lang-csharp"><![CDATA[void Setup ()
        /// {
        ///     AddObserver (rateKey, NSKeyValueObservingOptions.Old | NSKeyValueObservingOptions.New, (observed) => {
        ///         Console.WriteLine ("Change: {0}", observed.Change);
        ///         Console.WriteLine ("NewValue: {0}", observed.NewValue);
        ///         Console.WriteLine ("OldValue: {0}", observed.OldValue);
        ///         Console.WriteLine ("Indexes: {0}", observed.Indexes);
        ///         Console.WriteLine ("IsPrior: {0}", observed.IsPrior);
        ///     });
        /// }]]></code>
        ///         </example>
        ///         <para></para>
        ///       </remarks>
        public IDisposable AddObserver(
          string key,
          NSKeyValueObservingOptions options,
          Action<NSObservedChange> observer);

        /// <param name="key">
        /// 
        /// 
        /// Key-path to use to perform the value lookup. The keypath consists of a series of lowercase ASCII-strings with no spaces in them separated by dot characters.
        /// 
        /// 
        ///   	   </param>
        /// <param name="options">
        /// 
        /// 
        /// Flags indicating which notifications you are interested in receiving (New, Old, Initial, Prior).
        /// 
        /// 
        ///   	   </param>
        /// <param name="observer">
        /// 
        /// 
        /// Method that will receive the observed changes.   It will receive a <see cref="T:Foundation.NSObservedChange" /> parameter with the information that was changed.
        /// 
        /// 
        ///   	   </param>
        /// <summary>Registers an object for being observed externally using an arbitrary method.</summary>
        /// <returns>
        /// 
        /// 
        /// An IDisposable object.  Invoke the Dispose method on this object to remove the observer.
        /// 
        ///      </returns>
        /// <remarks>
        ///         <para>When the object is registered for observation, changes to the object specified in the keyPath that match the flags requested in options will be sent to the specied method (a lambda or method that matches the signature).</para>
        ///         <para>This version provides the convenience of exposing the changes as part of the strongly typed <see cref="T:Foundation.NSObservedChange" /> object that is received by the target.</para>
        ///         <para></para>
        ///         <example>
        ///           <code lang="csharp lang-csharp"><![CDATA[void Setup ()
        /// {
        ///     AddObserver (rateKey, NSKeyValueObservingOptions.Old | NSKeyValueObservingOptions.New, (observed) => {
        ///         Console.WriteLine ("Change: {0}", observed.Change);
        ///         Console.WriteLine ("NewValue: {0}", observed.NewValue);
        ///         Console.WriteLine ("OldValue: {0}", observed.OldValue);
        ///         Console.WriteLine ("Indexes: {0}", observed.Indexes);
        ///         Console.WriteLine ("IsPrior: {0}", observed.IsPrior);
        ///     });
        /// }]]></code>
        ///         </example>
        ///       </remarks>
        public IDisposable AddObserver(
          NSString key,
          NSKeyValueObservingOptions options,
          Action<NSObservedChange> observer);

        /// <param name="kls">To be added.</param>
        /// <summary>To be added.</summary>
        /// <returns>To be added.</returns>
        /// <remarks>To be added.</remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static NSObject Alloc(Class kls);

        /// <summary>To be added.</summary>
        /// <remarks>To be added.</remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Init();

        /// <param name="action">To be added.</param>
        /// <summary>To be added.</summary>
        /// <remarks>To be added.</remarks>
        public static void InvokeInBackground(Action action);

        /// <summary>The Objective-C class handle for this class.</summary>
        /// <value>The pointer to the Objective-C class.</value>
        /// <remarks>
        ///     Each managed class mirrors an unmanaged Objective-C class.
        ///     This value contains the pointer to the Objective-C class.
        ///     It is similar to calling the managed <see cref="M:ObjCRuntime.Class.GetHandle(System.String)" /> or the native <see href="https://developer.apple.com/documentation/objectivec/1418952-objc_getclass">objc_getClass</see> method with the type name.
        /// </remarks>
        public virtual NativeHandle ClassHandle { get; }

        /// <appledoc>https://developer.apple.com/documentation/foundation/nsarray/addobserver(_:forkeypath:options:context:)</appledoc>
        [Export("addObserver:forKeyPath:options:context:")]
        [BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
        public virtual void AddObserver(
#nullable enable
          NSObject observer,
          NSString keyPath,
          NSKeyValueObservingOptions options,
          IntPtr context);

        [BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
        public void AddObserver(
          NSObject observer,
          string keyPath,
          NSKeyValueObservingOptions options,
          IntPtr context);

        [Export("automaticallyNotifiesObserversForKey:")]
        [BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
        public static bool AutomaticallyNotifiesObserversForKey(string key);

        [Export("awakeFromNib")]
        [SupportedOSPlatform("maccatalyst")]
        [SupportedOSPlatform("ios")]
        [SupportedOSPlatform("macos")]
        [SupportedOSPlatform("tvos")]
        [BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
        [Advice("Overriding this method requires a call to the overriden method.")]
        [RequiresSuper]
        public virtual void AwakeFromNib();

        [Export("cancelPreviousPerformRequestsWithTarget:")]
        [BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
        public static void CancelPreviousPerformRequest(NSObject aTarget);

        [Export("cancelPreviousPerformRequestsWithTarget:selector:object:")]
        [BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
        public static void CancelPreviousPerformRequest(
          NSObject aTarget,
          Selector selector,
          NSObject? argument);

        /// <appledoc>https://developer.apple.com/documentation/foundation/nsportcoder/isbycopy</appledoc>
        [Export("copy")]
        [BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
        [return: Release]
        public virtual NSObject Copy();

        [Export("didChange:valuesAtIndexes:forKey:")]
        [BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
        public virtual void DidChange(NSKeyValueChange changeKind, NSIndexSet indexes, NSString forKey);

        [Export("didChangeValueForKey:withSetMutation:usingObjects:")]
        [BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
        public virtual void DidChange(
          NSString forKey,
          NSKeyValueSetMutationKind mutationKind,
          NSSet objects);

        [Export("didChangeValueForKey:")]
        [BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
        public virtual void DidChangeValue(string forKey);

        [Export("doesNotRecognizeSelector:")]
        [BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
        public virtual void DoesNotRecognizeSelector(Selector sel);

        [Export("dictionaryWithValuesForKeys:")]
        [BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
        public virtual NSDictionary GetDictionaryOfValuesFromKeys(NSString[] keys);

        [Export("keyPathsForValuesAffectingValueForKey:")]
        [BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
        public static NSSet GetKeyPathsForValuesAffecting(NSString key);

        [Export("methodForSelector:")]
        [BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
        public virtual IntPtr GetMethodForSelector(Selector sel);

        /// <summary>To be added.</summary>
        /// <returns>To be added.</returns>
        /// <remarks>To be added.</remarks>
        /// <appledoc>https://developer.apple.com/documentation/foundation/nshashtablecallbacks/hash</appledoc>
        [Export("hash")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
        public virtual UIntPtr GetNativeHash();

        /// <param name="anObject">To be added.</param>
        /// <summary>To be added.</summary>
        /// <returns>To be added.</returns>
        /// <remarks>To be added.</remarks>
        [Export("isEqual:")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
        public virtual bool IsEqual(NSObject? anObject);

        /// <param name="aClass">To be added.</param>
        /// <summary>To be added.</summary>
        /// <returns>To be added.</returns>
        /// <remarks>To be added.</remarks>
        [Export("isKindOfClass:")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
        public virtual bool IsKindOfClass(Class? aClass);

        /// <param name="aClass">To be added.</param>
        /// <summary>To be added.</summary>
        /// <returns>To be added.</returns>
        /// <remarks>To be added.</remarks>
        [Export("isMemberOfClass:")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
        public virtual bool IsMemberOfClass(Class? aClass);

        [Export("mutableCopy")]
        [BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
        [return: Release]
        public virtual NSObject MutableCopy();

        [Export("observeValueForKeyPath:ofObject:change:context:")]
        [BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
        public virtual void ObserveValue(
          NSString keyPath,
          NSObject ofObject,
          NSDictionary change,
          IntPtr context);

        [Export("performSelector:withObject:afterDelay:inModes:")]
        [BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
        public virtual void PerformSelector(
          Selector selector,
          NSObject? withObject,
          double afterDelay,
          NSString[] nsRunLoopModes);

        [Export("performSelector:withObject:afterDelay:")]
        [BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
        public virtual void PerformSelector(Selector selector, NSObject? withObject, double delay);

        [Export("performSelector:onThread:withObject:waitUntilDone:")]
        [BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
        public virtual void PerformSelector(
          Selector selector,
          NSThread onThread,
          NSObject? withObject,
          bool waitUntilDone);

        [Export("performSelector:onThread:withObject:waitUntilDone:modes:")]
        [BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
        public virtual void PerformSelector(
          Selector selector,
          NSThread onThread,
          NSObject? withObject,
          bool waitUntilDone,
          NSString[]? nsRunLoopModes);

        /// <param name="aSelector">To be added.</param>
        /// <summary>To be added.</summary>
        /// <returns>To be added.</returns>
        /// <remarks>To be added.</remarks>
        [Export("performSelector:")]
        [BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
        public virtual NSObject PerformSelector(Selector aSelector);

        /// <param name="aSelector">To be added.</param>
        /// <param name="anObject">To be added.</param>
        /// <summary>To be added.</summary>
        /// <returns>To be added.</returns>
        /// <remarks>To be added.</remarks>
        [Export("performSelector:withObject:")]
        [BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
        public virtual NSObject PerformSelector(Selector aSelector, NSObject? anObject);

        /// <param name="aSelector">To be added.</param>
        /// <param name="object1">To be added.</param>
        /// <param name="object2">To be added.</param>
        /// <summary>To be added.</summary>
        /// <returns>To be added.</returns>
        /// <remarks>To be added.</remarks>
        [Export("performSelector:withObject:withObject:")]
        [BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
        public virtual NSObject PerformSelector(Selector aSelector, NSObject? object1, NSObject? object2);

        [Export("prepareForInterfaceBuilder")]
        [SupportedOSPlatform("maccatalyst")]
        [SupportedOSPlatform("ios")]
        [SupportedOSPlatform("macos")]
        [SupportedOSPlatform("tvos")]
        [BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
        public virtual void PrepareForInterfaceBuilder();

        /// <appledoc>https://developer.apple.com/documentation/foundation/nsarray/removeobserver(_:forkeypath:context:)</appledoc>
        [Export("removeObserver:forKeyPath:context:")]
        [BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
        public virtual void RemoveObserver(NSObject observer, NSString keyPath, IntPtr context);

        [BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
        public void RemoveObserver(NSObject observer, string keyPath, IntPtr context);

        /// <appledoc>https://developer.apple.com/documentation/foundation/nsarray/removeobserver(_:forkeypath:)</appledoc>
        [Export("removeObserver:forKeyPath:")]
        [BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
        public virtual void RemoveObserver(NSObject observer, NSString keyPath);

        [BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
        public void RemoveObserver(NSObject observer, string keyPath);

        /// <param name="sel">To be added.</param>
        /// <summary>To be added.</summary>
        /// <returns>To be added.</returns>
        /// <remarks>To be added.</remarks>
        /// <appledoc>https://developer.apple.com/documentation/foundation/nsproxy/responds(to:)</appledoc>
        [Export("respondsToSelector:")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
        public virtual bool RespondsToSelector(Selector? sel);

        [Export("setNilValueForKey:")]
        [BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
        public virtual void SetNilValueForKey(NSString key);

        /// <appledoc>https://developer.apple.com/documentation/foundation/nsarray/setvalue(_:forkey:)</appledoc>
        [Export("setValue:forKey:")]
        [BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
        public virtual void SetValueForKey(NSObject value, NSString key);

        [Export("setValue:forKeyPath:")]
        [BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
        public virtual void SetValueForKeyPath(NSObject value, NSString keyPath);

        [Export("setValue:forUndefinedKey:")]
        [BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
        public virtual void SetValueForUndefinedKey(NSObject value, NSString undefinedKey);

        [Export("setValuesForKeysWithDictionary:")]
        [BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
        public virtual void SetValuesForKeysWithDictionary(NSDictionary keyedValues);

        /// <appledoc>https://developer.apple.com/documentation/foundation/nsarray/value(forkey:)</appledoc>
        [Export("valueForKey:")]
        [BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
        public virtual NSObject ValueForKey(NSString key);

        [Export("valueForKeyPath:")]
        [BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
        public virtual NSObject ValueForKeyPath(NSString keyPath);

        [Export("valueForUndefinedKey:")]
        [BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
        public virtual NSObject ValueForUndefinedKey(NSString key);

        [Export("willChange:valuesAtIndexes:forKey:")]
        [BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
        public virtual void WillChange(NSKeyValueChange changeKind, NSIndexSet indexes, NSString forKey);

        [Export("willChangeValueForKey:withSetMutation:usingObjects:")]
        [BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
        public virtual void WillChange(
          NSString forKey,
          NSKeyValueSetMutationKind mutationKind,
          NSSet objects);

        [Export("willChangeValueForKey:")]
        [BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
        public virtual void WillChangeValue(string forKey);

        [BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
        [SupportedOSPlatform("tvos13.0")]
        [SupportedOSPlatform("ios13.0")]
        [UnsupportedOSPlatform("macos")]
        [SupportedOSPlatform("maccatalyst")]
        public virtual NSAttributedString[] AccessibilityAttributedUserInputLabels { [Export("accessibilityAttributedUserInputLabels", ArgumentSemantic.Copy)] get; [Export("setAccessibilityAttributedUserInputLabels:", ArgumentSemantic.Copy)] set; }

        [BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
        [SupportedOSPlatform("tvos13.0")]
        [SupportedOSPlatform("ios13.0")]
        [UnsupportedOSPlatform("macos")]
        [SupportedOSPlatform("maccatalyst")]
        public virtual bool AccessibilityRespondsToUserInteraction { [Export("accessibilityRespondsToUserInteraction")] get; [Export("setAccessibilityRespondsToUserInteraction:")] set; }

        [BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
        [SupportedOSPlatform("tvos13.0")]
        [SupportedOSPlatform("ios13.0")]
        [UnsupportedOSPlatform("macos")]
        [SupportedOSPlatform("maccatalyst")]
        public virtual string? AccessibilityTextualContext { [Export("accessibilityTextualContext", ArgumentSemantic.Retain)] get; [Export("setAccessibilityTextualContext:", ArgumentSemantic.Retain)] set; }

        [BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
        [SupportedOSPlatform("tvos13.0")]
        [SupportedOSPlatform("ios13.0")]
        [UnsupportedOSPlatform("macos")]
        [SupportedOSPlatform("maccatalyst")]
        public virtual string[] AccessibilityUserInputLabels { [Export("accessibilityUserInputLabels", ArgumentSemantic.Retain)] get; [Export("setAccessibilityUserInputLabels:", ArgumentSemantic.Retain)] set; }

        /// <summary>To be added.</summary>
        /// <value>To be added.</value>
        /// <remarks>To be added.</remarks>
        /// <appledoc>https://developer.apple.com/documentation/foundation/nsproxy/class()</appledoc>
        [BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual Class Class { [Export("class")] get; }

        /// <appledoc>https://developer.apple.com/documentation/foundation/nsproxy/debugdescription</appledoc>
        [BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
        public virtual string DebugDescription { [Export("debugDescription")] get; }

        /// <summary>To be added.</summary>
        /// <value>To be added.</value>
        /// <remarks>To be added.</remarks>
        /// <appledoc>https://developer.apple.com/documentation/foundation/xmlnode/description</appledoc>
        [BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
        public virtual string Description { [Export("description")] get; }

        /// <summary>To be added.</summary>
        /// <value>To be added.</value>
        /// <remarks>To be added.</remarks>
        /// <appledoc>https://developer.apple.com/documentation/foundation/nsurlprotectionspace/isproxy</appledoc>
        [BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual bool IsProxy { [Export("isProxy")] get; }

        /// <summary>To be added.</summary>
        /// <value>To be added.</value>
        /// <remarks>To be added.</remarks>
        [BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
        public virtual UIntPtr RetainCount { [Export("retainCount")] get; }

        /// <summary>To be added.</summary>
        /// <value>To be added.</value>
        /// <remarks>To be added.</remarks>
        [BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual NSObject Self { [Export("self")] get; }

        /// <summary>To be added.</summary>
        /// <value>To be added.</value>
        /// <remarks>To be added.</remarks>
        [BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual Class Superclass { [Export("superclass")] get; }

        /// <summary>To be added.</summary>
        /// <value>To be added.</value>
        /// <remarks>To be added.</remarks>
        /// <appledoc>https://developer.apple.com/documentation/foundation/nsgarbagecollector/zone</appledoc>
        [BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual NSZone Zone { [Export("zone")] get; }

        /// <summary>Represents the value associated with the constant 'NSKeyValueChangeIndexesKey'.</summary>
        [Field("NSKeyValueChangeIndexesKey", "Foundation")]
        public static NSString ChangeIndexesKey { get; }

        /// <summary>Represents the value associated with the constant 'NSKeyValueChangeKindKey'.</summary>
        [Field("NSKeyValueChangeKindKey", "Foundation")]
        public static NSString ChangeKindKey { get; }

        /// <summary>Represents the value associated with the constant 'NSKeyValueChangeNewKey'.</summary>
        [Field("NSKeyValueChangeNewKey", "Foundation")]
        public static NSString ChangeNewKey { get; }

        /// <summary>Represents the value associated with the constant 'NSKeyValueChangeNotificationIsPriorKey'.</summary>
        [Field("NSKeyValueChangeNotificationIsPriorKey", "Foundation")]
        public static NSString ChangeNotificationIsPriorKey { get; }

        /// <summary>Represents the value associated with the constant 'NSKeyValueChangeOldKey'.</summary>
        [Field("NSKeyValueChangeOldKey", "Foundation")]
        public static NSString ChangeOldKey { get; }
#endif // MORE_NSOBJECT
    }

    public class NSString :
  NSObject,
  IComparable<NSString>,
  INSCoding,
  INativeObject,
  IDisposable,
  INSCopying,
  INSItemProviderReading,
  INSItemProviderWriting,
  INSMutableCopying,
  INSSecureCoding,
  ICKRecordValue
    {
        /// <summary>An <see cref="T:Foundation.NSString" /> instance for an empty (zero-length) string.</summary>
        public static readonly NSString Empty;
    }

    /// <summary>Attribute applied to interfaces that represent Objective-C protocols.</summary>
    /// <remarks>
    ///   <para>
    ///                Xamarin.iOS will export any interfaces with this attribute as a protocol to Objective-C,
    ///                and any classes that implement these interfaces will be marked as implementing
    ///                the corresponding protocol when exported to Objective-C.
    ///              </para>
    ///   <example>
    ///     <code lang="csharp lang-csharp"><![CDATA[
    ///          // This will create an Objective-C protocol called 'IMyProtocol', with one required member ('requiredMethod')
    ///          [Protocol ("IMyProtocol")]
    ///          interface IMyProtocol
    ///          {
    ///            [Export ("requiredMethod")]
    ///            void RequiredMethod ();
    ///          }
    /// 
    ///          // This will export the equivalent of "@interface MyClass : NSObject <IMyProtocol>" to Objective-C.
    ///          class MyClass : NSObject, IMyProtocol
    ///          {
    ///            void RequiredMethod ()
    ///            {
    ///            }
    ///          }
    ///                ]]></code>
    ///   </example>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public sealed class ProtocolAttribute : Attribute
    {
        /// <summary>The type of a specific managed type that can be used to wrap an instane of this protocol.</summary>
        /// <value>To be added.</value>
        /// <remarks>Objective-C protocols are bound as interfaces in managed code, but sometimes a class is needed (in certain
        /// scenarios our Objective-C-managed bridge have the pointer to an instance of a native object and we only know that it
        /// implements a particular protocol; in that case we might need a managed type that can wrap this instance, because the
        /// actual type of the object may not formally implement the interface).</remarks>
        public Type? WrapperType { get; set; }

        /// <summary>The name of the protocol.</summary>
        /// <value>To be added.</value>
        /// <remarks>To be added.</remarks>
        public string? Name { get; set; }

        /// <summary>Whether the Objective-C protocol is an informal protocol.</summary>
        /// <value>To be added.</value>
        /// <remarks>An informal protocol is the old name for an Objective-C category.</remarks>
        public bool IsInformal { get; set; }

        /// <summary>To be added.</summary>
        /// <value>To be added.</value>
        /// <remarks>To be added.</remarks>
        public string? FormalSince { get; set; }

        /// <summary>
        ///   <para>This property indicates whether the binding generator will generate backwards-compatible code for the protocol in question.</para>
        ///   <para>In particular, if this property is true, then the binding generator will generate extension methods for optional members and <see cref="T:Foundation.ProtocolMemberAttribute" /> attributes on the protocol interface for all protocol members.</para>
        /// </summary>
        /// <remarks>This property is by default true.</remarks>
        public bool BackwardsCompatibleCodeGeneration { get; set; }
    }
}

#endif
