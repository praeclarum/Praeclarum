#nullable enable

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

#if !__IOS__ && !__MACOS__ && !__TVOS__ && !__MACCATALYST__

using ObjCRuntime;
using CloudKit;

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
        public ExportAttribute(string? selector)
        {
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
        // static T? CreateInstance<T>(NSCoder decoder) where T : NSObject, INSCoding;

        /// <summary>Encodes the state of the object using the provided encoder.</summary>
        /// <param name="encoder">The encoder object where the state of the object will be stored</param>
        // [Export("encodeWithCoder:")]
        // void EncodeTo(NSCoder encoder);
    }

    /// <summary>This interface represents the Objective-C protocol <c>NSCopying</c>.</summary>
    [Protocol(Name = "NSCopying")]
    public interface INSCopying : INativeObject, IDisposable
    {
        /// <summary>Performs a copy of the underlying Objective-C object.</summary>
        /// <param name="zone">Developers should pass <see langword="null" />.  Memory zones are no longer used.</param>
        /// <returns>The newly-allocated object.</returns>
        // [Export("copyWithZone:")]
        // NSObject Copy(NSZone? zone);
    }

    /// <summary>This interface represents the Objective-C protocol <c>NSItemProviderReading</c>.</summary>
    [Protocol(Name = "NSItemProviderReading")]
    public interface INSItemProviderReading : INativeObject, IDisposable
    {
        /// <param name="data">To be added.</param>
        /// <param name="typeIdentifier">To be added.</param>
        /// <param name="outError">To be added.</param>
        // [Export("objectWithItemProviderData:typeIdentifier:error:")]
        // static INSItemProviderReading? GetObject<T>(
        //     NSData data,
        //     string typeIdentifier,
        //     out NSError? outError)
        //     where T : NSObject, INSItemProviderReading;

        // static string[] GetReadableTypeIdentifiers<T>() where T : NSObject, INSItemProviderReading;
    }

    /// <summary>Interface used by <see cref="T:Foundation.NSItemProvider" /> for retrieving data from an object.</summary>
    [Protocol(Name = "NSItemProviderWriting")]
    public interface INSItemProviderWriting : INativeObject, IDisposable
    {
        // [Export("itemProviderVisibilityForRepresentationWithTypeIdentifier:")]
        // NSItemProviderRepresentationVisibility GetItemProviderVisibilityForTypeIdentifier(string typeIdentifier);

        /// <summary>Implement this method to customize the loading of data by an <see cref="T:Foundation.NSItemProvider" />.</summary>
        /// <param name="typeIdentifier">A Universal Type Identifier (UTI) indicating the type of data to load.</param>
        /// <param name="completionHandler">The method called after the data is loaded.</param>
        /// <returns>An <see cref="T:Foundation.NSProgress" /> object reflecting the data-loading operation.</returns>
        // [Export("loadDataWithTypeIdentifier:forItemProviderCompletionHandler:")]
        // NSProgress? LoadData(string typeIdentifier, Action<NSData, NSError> completionHandler);

        /// <summary>Asynchronously loads data for the identified type from an item provider, returning a task that contains the data.</summary>
        /// <param name="typeIdentifier">A Universal Type Identifier (UTI) indicating the type of data to load.</param>
        // Task<NSData> LoadDataAsync(string typeIdentifier);
        // Task<NSData> LoadDataAsync(string typeIdentifier, out NSProgress result);

        // string[] WritableTypeIdentifiersForItemProvider { [Export("writableTypeIdentifiersForItemProvider")] get; }

        // static string[] GetWritableTypeIdentifiers<T>() where T : NSObject, INSItemProviderWriting;
    }

    /// <summary>This interface represents the Objective-C protocol <c>NSMutableCopying</c>.</summary>
    [Protocol(Name = "NSMutableCopying")]
    public interface INSMutableCopying : INativeObject, IDisposable, INSCopying
    {
        /// <param name="zone">Zone to use to allocate this object, or null to use the default zone.</param>
        // [Export("mutableCopyWithZone:")]
        // NSObject MutableCopy(NSZone? zone);
    }

    /// <summary>The secure coding category.</summary>
    [Protocol(Name = "NSSecureCoding")]
    public interface INSSecureCoding : INativeObject, IDisposable, INSCoding
    {
    }

    [Register("NSCoder", true)]
    public class NSCoder : NSObject
    {
#if MORE_NSCODER
        /// <summary>Encodes the byte array using the specified associated key.</summary>
        /// <param name="buffer">Byte array to encode.</param>
        /// <param name="key">Key to associate with the object being encoded.</param>
        public void Encode(byte[] buffer, string key);

        /// <summary>Encodes the byte array of an unspecified type.</summary>
        /// <param name="buffer">Byte array to encode.</param>
        public void Encode(byte[] buffer);

        /// <summary>Encodes a segment of the buffer using the specified associated key.</summary>
        /// <param name="buffer">Byte array to encode.</param>
        /// <param name="offset">Starting point in the buffer to encode.</param>
        /// <param name="count">Number of bytes starting at the specified offset to encode.</param>
        /// <param name="key">Key to associate with the object being encoded.</param>
        public void Encode(byte[] buffer, int offset, int count, string key);

        /// <summary>Decodes the requested key as an array of bytes.</summary>
        /// <param name="key">The key identifying the item to decode.</param>
        /// <returns>The decoded array of bytes.</returns>
        public byte[]? DecodeBytes(string key);

        /// <remarks>The decoded array of bytes.</remarks>
        /// <summary>Decodes the next item as an array of bytes.</summary>
        /// <returns>The array of bytes decoded from the stream.</returns>
        public byte[]? DecodeBytes();

        /// <summary>Attempts to decode a boolean value associated with the specified key.</summary>
        /// <param name="key">The key identifying the item to decode.</param>
        /// <param name="result">When this method returns, contains the decoded boolean value if the key exists; otherwise, false.</param>
        /// <returns>
        /// <see langword="true" /> if the key exists and the value was decoded; otherwise, <see langword="false" />.</returns>
        public bool TryDecode(string key, out bool result);

        /// <summary>Attempts to decode a double value associated with the specified key.</summary>
        /// <param name="key">The key identifying the item to decode.</param>
        /// <param name="result">When this method returns, contains the decoded double value if the key exists; otherwise, 0.</param>
        /// <returns>
        /// <see langword="true" /> if the key exists and the value was decoded; otherwise, <see langword="false" />.</returns>
        public bool TryDecode(string key, out double result);

        /// <summary>Attempts to decode a float value associated with the specified key.</summary>
        /// <param name="key">The key identifying the item to decode.</param>
        /// <param name="result">When this method returns, contains the decoded float value if the key exists; otherwise, 0.</param>
        /// <returns>
        /// <see langword="true" /> if the key exists and the value was decoded; otherwise, <see langword="false" />.</returns>
        public bool TryDecode(string key, out float result);

        /// <summary>Attempts to decode an integer value associated with the specified key.</summary>
        /// <param name="key">The key identifying the item to decode.</param>
        /// <param name="result">When this method returns, contains the decoded integer value if the key exists; otherwise, 0.</param>
        /// <returns>
        /// <see langword="true" /> if the key exists and the value was decoded; otherwise, <see langword="false" />.</returns>
        public bool TryDecode(string key, out int result);

        /// <summary>Attempts to decode a long integer value associated with the specified key.</summary>
        /// <param name="key">The key identifying the item to decode.</param>
        /// <param name="result">When this method returns, contains the decoded long integer value if the key exists; otherwise, 0.</param>
        /// <returns>
        /// <see langword="true" /> if the key exists and the value was decoded; otherwise, <see langword="false" />.</returns>
        public bool TryDecode(string key, out long result);

        /// <summary>Attempts to decode a native integer value associated with the specified key.</summary>
        /// <param name="key">The key identifying the item to decode.</param>
        /// <param name="result">When this method returns, contains the decoded native integer value if the key exists; otherwise, 0.</param>
        /// <returns>
        /// <see langword="true" /> if the key exists and the value was decoded; otherwise, <see langword="false" />.</returns>
        public bool TryDecode(string key, out IntPtr result);

        /// <summary>Attempts to decode an NSObject associated with the specified key.</summary>
        /// <param name="key">The key identifying the item to decode.</param>
        /// <param name="result">When this method returns, contains the decoded NSObject if the key exists; otherwise, <see langword="null" />.</param>
        /// <returns>
        /// <see langword="true" /> if the key exists and the value was decoded; otherwise, <see langword="false" />.</returns>
        public bool TryDecode(string key, out NSObject? result);

        /// <summary>Attempts to decode a byte array associated with the specified key.</summary>
        /// <param name="key">The key identifying the item to decode.</param>
        /// <param name="result">When this method returns, contains the decoded byte array if the key exists; otherwise, <see langword="null" />.</param>
        /// <returns>
        /// <see langword="true" /> if the key exists and the value was decoded; otherwise, <see langword="false" />.</returns>
        public bool TryDecode(string key, out byte[]? result);

        /// <summary>Decodes a top-level object of the specified type associated with the specified key.</summary>
        /// <param name="type">The type of the object to decode.</param>
        /// <param name="key">The key identifying the item to decode.</param>
        /// <param name="error">When this method returns, contains an error object if decoding failed; otherwise, <see langword="null" />.</param>
        /// <returns>The decoded object, or <see langword="null" /> if decoding failed.</returns>
        public NSObject? DecodeTopLevelObject(Type type, string key, out NSError? error);

        /// <summary>Decodes a top-level object of one of the specified types associated with the specified key.</summary>
        /// <param name="types">An array of types that the decoded object can be. If <see langword="null" />, any type is allowed.</param>
        /// <param name="key">The key identifying the item to decode.</param>
        /// <param name="error">When this method returns, contains an error object if decoding failed; otherwise, <see langword="null" />.</param>
        /// <returns>The decoded object, or <see langword="null" /> if decoding failed.</returns>
        public NSObject? DecodeTopLevelObject(Type[]? types, string key, out NSError? error);

        /// <summary>Decode a single value of the specified <paramref name="type" /> into the provided <paramref name="data" /> buffer.</summary>
        /// <param name="type">The type of the value to decode.</param>
        /// <param name="data">The buffer to store the decoded value. The buffer must be big enough to hold the decoded value.</param>
        public void DecodeValue(Type type, Span<byte> data);

        /// <summary>The Objective-C class handle for this class.</summary>
        /// <value>The pointer to the Objective-C class.</value>
        /// <remarks>
        ///     Each managed class mirrors an unmanaged Objective-C class.
        ///     This value contains the pointer to the Objective-C class.
        ///     It is similar to calling the managed <see cref="M:ObjCRuntime.Class.GetHandle(System.String)" /> or the native <see href="https://developer.apple.com/documentation/objectivec/1418952-objc_getclass">objc_getClass</see> method with the type name.
        /// </remarks>
        public override NativeHandle ClassHandle { get; }

        /// <summary>Creates a new <see cref="T:Foundation.NSCoder" /> with default values.</summary>
        /// <appledoc>https://developer.apple.com/documentation/foundation/process/init()</appledoc>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        [Export("init")]
        public NSCoder();

        /// <summary>Constructor to call on derived classes to skip initialization and merely allocate the object.</summary>
        /// <param name="t">Unused sentinel value, pass NSObjectFlag.Empty.</param>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected NSCoder(NSObjectFlag t);

        /// <summary>A constructor used when creating managed representations of unmanaged objects. Called by the runtime.</summary>
        /// <param name="handle">Pointer (handle) to the unmanaged object.</param>
        /// <remarks>
        ///   <para>
        ///               This constructor is invoked by the runtime infrastructure (<see cref="M:ObjCRuntime.Runtime.GetNSObject(System.IntPtr)" />) to create a new managed representation for a pointer to an unmanaged Objective-C object.
        ///               Developers should not invoke this method directly, instead they should call <see cref="M:ObjCRuntime.Runtime.GetNSObject(System.IntPtr)" /> as it will prevent two instances of a managed object pointing to the same native object.
        ///           </para>
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected internal NSCoder(NativeHandle handle);

        /// <appledoc>https://developer.apple.com/documentation/foundation/nscoder/containsvalue(forkey:)</appledoc>
        [Export("containsValueForKey:")]
        public virtual bool ContainsKey(string key);

        /// <appledoc>https://developer.apple.com/documentation/foundation/nscoder/decodearrayofobjectsofclass:forkey:</appledoc>
        [Export("decodeArrayOfObjectsOfClass:forKey:")]
        [SupportedOSPlatform("tvos14.0")]
        [SupportedOSPlatform("ios14.0")]
        [SupportedOSPlatform("maccatalyst")]
        [SupportedOSPlatform("macos")]
        public virtual NSObject[]? DecodeArrayOfObjects(Class @class, string key);

        /// <appledoc>https://developer.apple.com/documentation/foundation/nscoder/decodearrayofobjectsofclasses:forkey:</appledoc>
        [Export("decodeArrayOfObjectsOfClasses:forKey:")]
        public virtual NSObject[]? DecodeArrayOfObjects(NSSet<Class> classes, string key);

        /// <appledoc>https://developer.apple.com/documentation/foundation/nscoder/decodebool(forkey:)</appledoc>
        [Export("decodeBoolForKey:")]
        public virtual bool DecodeBool(string key);

        /// <param name="key">The key identifying the item to decode.</param>
        /// <param name="length">Number of bytes in the returned block.</param>
        /// <summary>Low-level: decodes the item with the associated key into a memory block,
        /// and returns a pointer to it.</summary>
        /// <returns>Pointer to the block of memory that contains at least
        /// the number of bytes set on the lenght parameter.</returns>
        /// <remarks></remarks>
        /// <appledoc>https://developer.apple.com/documentation/foundation/nscoder/decodebytes(forkey:returnedlength:)</appledoc>
        [Export("decodeBytesForKey:returnedLength:")]
        public virtual IntPtr DecodeBytes(string key, out UIntPtr length);

        /// <param name="length">Number of bytes in the returned block.</param>
        /// <summary>Low-level: decodes the next item into a memory block,
        /// and returns a pointer to it.</summary>
        /// <returns>Pointer to the block of memory that contains at least
        /// the number of bytes set on the lenght parameter.</returns>
        /// <remarks></remarks>
        /// <appledoc>https://developer.apple.com/documentation/foundation/nscoder/decodebytes(withreturnedlength:)</appledoc>
        [Export("decodeBytesWithReturnedLength:")]
        public virtual IntPtr DecodeBytes(out UIntPtr length);

        /// <appledoc>https://developer.apple.com/documentation/foundation/nscoder/decodebytes(withminimumlength:)</appledoc>
        [Export("decodeBytesWithMinimumLength:")]
        [SupportedOSPlatform("tvos18.4")]
        [SupportedOSPlatform("ios18.4")]
        [SupportedOSPlatform("maccatalyst18.4")]
        [SupportedOSPlatform("macos15.4")]
        public virtual IntPtr DecodeBytes(UIntPtr minimumLength);

        /// <appledoc>https://developer.apple.com/documentation/foundation/nscoder/decodebytes(forkey:minimumlength:)</appledoc>
        [Export("decodeBytesForKey:minimumLength:")]
        [SupportedOSPlatform("tvos18.4")]
        [SupportedOSPlatform("ios18.4")]
        [SupportedOSPlatform("maccatalyst18.4")]
        [SupportedOSPlatform("macos15.4")]
        public virtual IntPtr DecodeBytes(string key, UIntPtr minimumLength);

        /// <appledoc>https://developer.apple.com/documentation/foundation/nscoder/decodedata()</appledoc>
        [Export("decodeDataObject")]
        public virtual NSData? DecodeDataObject();

        /// <appledoc>https://developer.apple.com/documentation/foundation/nscoder/decodedictionarywithkeysofclass:objectsofclass:forkey:</appledoc>
        [Export("decodeDictionaryWithKeysOfClass:objectsOfClass:forKey:")]
        [SupportedOSPlatform("tvos14.0")]
        [SupportedOSPlatform("ios14.0")]
        [SupportedOSPlatform("maccatalyst")]
        [SupportedOSPlatform("macos")]
        public virtual NSDictionary? DecodeDictionary(Class keyClass, Class objectClass, string key);

        /// <appledoc>https://developer.apple.com/documentation/foundation/nscoder/decodedictionarywithkeysofclasses:objectsofclasses:forkey:</appledoc>
        [Export("decodeDictionaryWithKeysOfClasses:objectsOfClasses:forKey:")]
        [SupportedOSPlatform("tvos14.0")]
        [SupportedOSPlatform("ios14.0")]
        [SupportedOSPlatform("maccatalyst")]
        [SupportedOSPlatform("macos")]
        public virtual NSDictionary? DecodeDictionary(
          NSSet<Class> keyClasses,
          NSSet<Class> objectClasses,
          string key);

        /// <appledoc>https://developer.apple.com/documentation/foundation/nscoder/decodedouble(forkey:)</appledoc>
        [Export("decodeDoubleForKey:")]
        public virtual double DecodeDouble(string key);

        /// <appledoc>https://developer.apple.com/documentation/foundation/nscoder/decodefloat(forkey:)</appledoc>
        [Export("decodeFloatForKey:")]
        public virtual float DecodeFloat(string key);

        /// <appledoc>https://developer.apple.com/documentation/foundation/nscoder/decodeint32(forkey:)</appledoc>
        [Export("decodeInt32ForKey:")]
        public virtual int DecodeInt(string key);

        /// <appledoc>https://developer.apple.com/documentation/foundation/nscoder/decodeint64(forkey:)</appledoc>
        [Export("decodeInt64ForKey:")]
        public virtual long DecodeLong(string key);

        /// <appledoc>https://developer.apple.com/documentation/foundation/nscoder/decodeinteger(forkey:)</appledoc>
        [Export("decodeIntegerForKey:")]
        public virtual IntPtr DecodeNInt(string key);

        /// <appledoc>https://developer.apple.com/documentation/foundation/nscoder/decodeobject()</appledoc>
        [Export("decodeObject")]
        public virtual NSObject? DecodeObject();

        /// <appledoc>https://developer.apple.com/documentation/foundation/nscoder/decodeobject(forkey:)</appledoc>
        [Export("decodeObjectForKey:")]
        public virtual NSObject? DecodeObject(string key);

        /// <appledoc>https://developer.apple.com/documentation/foundation/nscoder/decodeobjectofclass:forkey:</appledoc>
        [Export("decodeObjectOfClass:forKey:")]
        public virtual NSObject? DecodeObject(Class @class, string key);

        public NSObject? DecodeObject(Type type, string key);

        /// <appledoc>https://developer.apple.com/documentation/foundation/nscoder/decodeobjectofclasses:forkey:</appledoc>
        [Export("decodeObjectOfClasses:forKey:")]
        public virtual NSObject? DecodeObject(NSSet<Class>? classes, string key);

        public NSObject? DecodeObject(Type[] types, string key);

        /// <appledoc>https://developer.apple.com/documentation/foundation/nscoder/decodepropertylist(forkey:)</appledoc>
        [Export("decodePropertyListForKey:")]
        public virtual NSObject? DecodePropertyList(string key);

        /// <appledoc>https://developer.apple.com/documentation/foundation/nscoder/decodetoplevelobjectandreturnerror:</appledoc>
        [Export("decodeTopLevelObjectAndReturnError:")]
        public virtual NSObject? DecodeTopLevelObject(out NSError? error);

        /// <appledoc>https://developer.apple.com/documentation/foundation/nscoder/decodetoplevelobjectforkey:error:</appledoc>
        [Export("decodeTopLevelObjectForKey:error:")]
        public virtual NSObject? DecodeTopLevelObject(string key, out NSError? error);

        /// <appledoc>https://developer.apple.com/documentation/foundation/nscoder/decodetoplevelobjectofclass:forkey:error:</appledoc>
        [Export("decodeTopLevelObjectOfClass:forKey:error:")]
        public virtual NSObject? DecodeTopLevelObject(Class klass, string key, out NSError? error);

        /// <appledoc>https://developer.apple.com/documentation/foundation/nscoder/decodetoplevelobjectofclasses:forkey:error:</appledoc>
        [Export("decodeTopLevelObjectOfClasses:forKey:error:")]
        public virtual NSObject? DecodeTopLevelObject(
          NSSet<Class>? setOfClasses,
          string key,
          out NSError? error);

        /// <appledoc>https://developer.apple.com/documentation/foundation/nscoder/decodevalue(ofobjctype:at:size:)</appledoc>
        [Export("decodeValueOfObjCType:at:size:")]
        public virtual void DecodeValue(IntPtr objCTypeCode, IntPtr data, UIntPtr size);

        /// <appledoc>https://developer.apple.com/documentation/foundation/nscoder/encode(_:)-9648d</appledoc>
        [Export("encodeObject:")]
        public virtual void Encode(NSObject? obj);

        /// <appledoc>https://developer.apple.com/documentation/foundation/nscoder/encode(_:forkey:)-1mlmu</appledoc>
        [Export("encodeObject:forKey:")]
        public virtual void Encode(NSObject? val, string key);

        /// <appledoc>https://developer.apple.com/documentation/foundation/nscoder/encode(_:forkey:)-7o6mu</appledoc>
        [Export("encodeBool:forKey:")]
        public virtual void Encode(bool val, string key);

        /// <appledoc>https://developer.apple.com/documentation/foundation/nscoder/encode(_:forkey:)-9xiiu</appledoc>
        [Export("encodeDouble:forKey:")]
        public virtual void Encode(double val, string key);

        /// <appledoc>https://developer.apple.com/documentation/foundation/nscoder/encode(_:forkey:)-84cez</appledoc>
        [Export("encodeFloat:forKey:")]
        public virtual void Encode(float val, string key);

        /// <appledoc>https://developer.apple.com/documentation/foundation/nscoder/encode(_:forkey:)-5sk4z</appledoc>
        [Export("encodeInt32:forKey:")]
        public virtual void Encode(int val, string key);

        /// <appledoc>https://developer.apple.com/documentation/foundation/nscoder/encode(_:forkey:)-dixg</appledoc>
        [Export("encodeInt64:forKey:")]
        public virtual void Encode(long val, string key);

        /// <param name="val">Native integer value to encode.</param>
        /// <param name="key">Key to associate with the object being encoded.</param>
        /// <summary>Encodes the platform-specific native integer (32 or 64 bits) using the specified associated key.</summary>
        /// <appledoc>https://developer.apple.com/documentation/foundation/nscoder/encode(_:forkey:)-2dprz</appledoc>
        [Export("encodeInteger:forKey:")]
        public virtual void Encode(IntPtr val, string key);

        /// <appledoc>https://developer.apple.com/documentation/foundation/nscoder/encodebytes(_:length:)</appledoc>
        [Export("encodeBytes:length:")]
        public virtual void Encode(IntPtr bytes, IntPtr length);

        /// <appledoc>https://developer.apple.com/documentation/foundation/nscoder/encode(_:)-1qd1e</appledoc>
        [Export("encodeDataObject:")]
        public virtual void Encode(NSData data);

        /// <appledoc>https://developer.apple.com/documentation/foundation/nscoder/encodebytes(_:length:forkey:)</appledoc>
        [Export("encodeBytes:length:forKey:")]
        public virtual void EncodeBlock(IntPtr bytes, IntPtr length, string key);

        /// <appledoc>https://developer.apple.com/documentation/foundation/nscoder/encodebycopyobject(_:)</appledoc>
        [Export("encodeBycopyObject:")]
        public virtual void EncodeBycopyObject(NSObject? anObject);

        /// <appledoc>https://developer.apple.com/documentation/foundation/nscoder/encodebyrefobject(_:)</appledoc>
        [Export("encodeByrefObject:")]
        public virtual void EncodeByrefObject(NSObject? anObject);

        /// <appledoc>https://developer.apple.com/documentation/foundation/nscoder/encodeconditionalobject(_:forkey:)</appledoc>
        [Export("encodeConditionalObject:forKey:")]
        public virtual void EncodeConditionalObject(NSObject? val, string key);

        /// <appledoc>https://developer.apple.com/documentation/foundation/nsarchiver/encodeconditionalobject(_:)</appledoc>
        [Export("encodeConditionalObject:")]
        public virtual void EncodeConditionalObject(NSObject? value);

        /// <appledoc>https://developer.apple.com/documentation/foundation/nsarchiver/encoderootobject(_:)</appledoc>
        [Export("encodeRootObject:")]
        public virtual void EncodeRoot(NSObject obj);

        /// <appledoc>https://developer.apple.com/documentation/foundation/nscoder/failwitherror(_:)</appledoc>
        [Export("failWithError:")]
        public virtual void Fail(NSError error);

        /// <appledoc>https://developer.apple.com/documentation/foundation/nscoder/requiressecurecoding</appledoc>
        [Export("requiresSecureCoding")]
        public virtual bool RequiresSecureCoding();

        /// <appledoc>https://developer.apple.com/documentation/foundation/nscoder/allowedclasses</appledoc>
        public virtual NSSet? AllowedClasses { [Export("allowedClasses")] get; }

        /// <appledoc>https://developer.apple.com/documentation/foundation/nscoder/allowskeyedcoding</appledoc>
        public virtual bool AllowsKeyedCoding { [Export("allowsKeyedCoding")] get; }

        /// <appledoc>https://developer.apple.com/documentation/foundation/nscoder/decodingfailurepolicy-swift.property</appledoc>
        public virtual NSDecodingFailurePolicy DecodingFailurePolicy { [Export("decodingFailurePolicy")] get; }

        /// <appledoc>https://developer.apple.com/documentation/foundation/urlauthenticationchallenge/error</appledoc>
        public virtual NSError? Error { [Export("error", ArgumentSemantic.Copy)] get; }

        /// <appledoc>https://developer.apple.com/documentation/foundation/nscoder/systemversion</appledoc>
        public virtual uint SystemVersion { [Export("systemVersion")] get; }
#endif
    }

    /// <summary>Base class for all bound objects that map to Objective-C objects.</summary>
    [Register("NSObject", true)]
    [StructLayout(LayoutKind.Sequential)]
    public class NSObject :
        INativeObject,
        IEquatable<NSObject>,
        IDisposable
    //   INSObjectFactory,
    //   INSObjectProtocol
    {
        // public static readonly Assembly PlatformAssembly;

        /// <summary>Handle (pointer) to the unmanaged object representation.</summary>
        /// <value>A pointer</value>
        /// <remarks>This IntPtr is a handle to the underlying unmanaged representation for this object.</remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public NativeHandle Handle { get; set; }

        /// <appledoc>https://developer.apple.com/documentation/foundation/process/init()</appledoc>
        [Export("init")]
        public NSObject() {
            Handle = new NativeHandle ();
        }

        /// <summary>Constructor to call on derived classes to skip initialization and merely allocate the object.</summary>
        /// <param name="x">Unused sentinel value, pass NSObjectFlag.Empty.</param>
        public NSObject(NSObjectFlag x) {
            Handle = new NativeHandle ();
        }

        ~NSObject() {
            Dispose (false);
        }
        public void Dispose() {
            Dispose (true);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected NSObject(NativeHandle handle)
        {
            Handle = handle;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected NSObject(NativeHandle handle, bool alloced)
        {
            Handle = handle;
        }

        /// <summary>Generates a hash code for the current instance.</summary>
        /// <returns>A int containing the hash code for this instance.</returns>
        public override int GetHashCode() => Handle.GetHashCode();

        public override bool Equals(object? obj)
        {
            return obj is NSObject no && no.Handle == Handle;
        }

        public bool Equals (NSObject? obj) => obj?.Handle == Handle;

        /// <summary>Returns a string representation of the value of the current instance.</summary>
        // public override string ToString();

        /// <summary>Releases the resources used by the NSObject object.</summary>
        /// <param name="disposing">
        ///   <para>If set to <see langword="true" />, the method is invoked directly and will dispose managed and unmanaged resources;   If set to <see langword="false" /> the method is being called by the garbage collector finalizer and should only release unmanaged resources.</para>
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
        }

#if MORE_NSOBJECT
        /// <summary>Promotes a regular peer object (IsDirectBinding is true) into a toggleref object.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected void MarkDirty();

        [Export("conformsToProtocol:")]
        [Preserve]
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
        [EditorBrowsable(EditorBrowsableState.Never)]
        public NativeHandle SuperHandle { get; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected void InitializeHandle(NativeHandle handle);

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected void InitializeHandle(NativeHandle handle, string initSelector);

        /// <summary>Invokes asynchrously the specified code on the main UI thread.</summary>
        /// <param name="sel">Selector to invoke</param>
        /// <param name="obj">Object in which the selector is invoked</param>
        public void BeginInvokeOnMainThread(Selector sel, NSObject obj);

        /// <summary>Invokes synchrously the specified code on the main UI thread.</summary>
        /// <param name="sel">Selector to invoke</param>
        /// <param name="obj">Object in which the selector is invoked</param>
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

        /// <summary>Registers an object for being observed externally using an arbitrary method.</summary>
        public IDisposable AddObserver(
          string key,
          NSKeyValueObservingOptions options,
          Action<NSObservedChange> observer);

        /// <summary>Registers an object for being observed externally using an arbitrary method.</summary>
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
        public virtual void AddObserver(
          NSObject observer,
          NSString keyPath,
          NSKeyValueObservingOptions options,
          IntPtr context);

        public void AddObserver(
          NSObject observer,
          string keyPath,
          NSKeyValueObservingOptions options,
          IntPtr context);

        [Export("automaticallyNotifiesObserversForKey:")]
        public static bool AutomaticallyNotifiesObserversForKey(string key);

        [Export("awakeFromNib")]
        [SupportedOSPlatform("maccatalyst")]
        [SupportedOSPlatform("ios")]
        [SupportedOSPlatform("macos")]
        [SupportedOSPlatform("tvos")]
                [Advice("Overriding this method requires a call to the overriden method.")]
        [RequiresSuper]
        public virtual void AwakeFromNib();

        [Export("cancelPreviousPerformRequestsWithTarget:")]
                public static void CancelPreviousPerformRequest(NSObject aTarget);

        [Export("cancelPreviousPerformRequestsWithTarget:selector:object:")]
                public static void CancelPreviousPerformRequest(
          NSObject aTarget,
          Selector selector,
          NSObject? argument);

        /// <appledoc>https://developer.apple.com/documentation/foundation/nsportcoder/isbycopy</appledoc>
        [Export("copy")]
                [return: Release]
        public virtual NSObject Copy();

        [Export("didChange:valuesAtIndexes:forKey:")]
                public virtual void DidChange(NSKeyValueChange changeKind, NSIndexSet indexes, NSString forKey);

        [Export("didChangeValueForKey:withSetMutation:usingObjects:")]
                public virtual void DidChange(
          NSString forKey,
          NSKeyValueSetMutationKind mutationKind,
          NSSet objects);

        [Export("didChangeValueForKey:")]
                public virtual void DidChangeValue(string forKey);

        [Export("doesNotRecognizeSelector:")]
                public virtual void DoesNotRecognizeSelector(Selector sel);

        [Export("dictionaryWithValuesForKeys:")]
                public virtual NSDictionary GetDictionaryOfValuesFromKeys(NSString[] keys);

        [Export("keyPathsForValuesAffectingValueForKey:")]
                public static NSSet GetKeyPathsForValuesAffecting(NSString key);

        [Export("methodForSelector:")]
                public virtual IntPtr GetMethodForSelector(Selector sel);

        /// <summary>To be added.</summary>
        /// <returns>To be added.</returns>
        /// <remarks>To be added.</remarks>
        /// <appledoc>https://developer.apple.com/documentation/foundation/nshashtablecallbacks/hash</appledoc>
        [Export("hash")]
        [EditorBrowsable(EditorBrowsableState.Never)]
                public virtual UIntPtr GetNativeHash();

        /// <param name="anObject">To be added.</param>
        /// <summary>To be added.</summary>
        /// <returns>To be added.</returns>
        /// <remarks>To be added.</remarks>
        [Export("isEqual:")]
        [EditorBrowsable(EditorBrowsableState.Never)]
                public virtual bool IsEqual(NSObject? anObject);

        /// <param name="aClass">To be added.</param>
        /// <summary>To be added.</summary>
        /// <returns>To be added.</returns>
        /// <remarks>To be added.</remarks>
        [Export("isKindOfClass:")]
        [EditorBrowsable(EditorBrowsableState.Never)]
                public virtual bool IsKindOfClass(Class? aClass);

        /// <param name="aClass">To be added.</param>
        /// <summary>To be added.</summary>
        /// <returns>To be added.</returns>
        /// <remarks>To be added.</remarks>
        [Export("isMemberOfClass:")]
        [EditorBrowsable(EditorBrowsableState.Never)]
                public virtual bool IsMemberOfClass(Class? aClass);

        [Export("mutableCopy")]
                [return: Release]
        public virtual NSObject MutableCopy();

        [Export("observeValueForKeyPath:ofObject:change:context:")]
                public virtual void ObserveValue(
          NSString keyPath,
          NSObject ofObject,
          NSDictionary change,
          IntPtr context);

        [Export("performSelector:withObject:afterDelay:inModes:")]
                public virtual void PerformSelector(
          Selector selector,
          NSObject? withObject,
          double afterDelay,
          NSString[] nsRunLoopModes);

        [Export("performSelector:withObject:afterDelay:")]
                public virtual void PerformSelector(Selector selector, NSObject? withObject, double delay);

        [Export("performSelector:onThread:withObject:waitUntilDone:")]
                public virtual void PerformSelector(
          Selector selector,
          NSThread onThread,
          NSObject? withObject,
          bool waitUntilDone);

        [Export("performSelector:onThread:withObject:waitUntilDone:modes:")]
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
                public virtual NSObject PerformSelector(Selector aSelector);

        /// <param name="aSelector">To be added.</param>
        /// <param name="anObject">To be added.</param>
        /// <summary>To be added.</summary>
        /// <returns>To be added.</returns>
        /// <remarks>To be added.</remarks>
        [Export("performSelector:withObject:")]
                public virtual NSObject PerformSelector(Selector aSelector, NSObject? anObject);

        /// <param name="aSelector">To be added.</param>
        /// <param name="object1">To be added.</param>
        /// <param name="object2">To be added.</param>
        /// <summary>To be added.</summary>
        /// <returns>To be added.</returns>
        /// <remarks>To be added.</remarks>
        [Export("performSelector:withObject:withObject:")]
                public virtual NSObject PerformSelector(Selector aSelector, NSObject? object1, NSObject? object2);

        [Export("prepareForInterfaceBuilder")]
        [SupportedOSPlatform("maccatalyst")]
        [SupportedOSPlatform("ios")]
        [SupportedOSPlatform("macos")]
        [SupportedOSPlatform("tvos")]
                public virtual void PrepareForInterfaceBuilder();

        /// <appledoc>https://developer.apple.com/documentation/foundation/nsarray/removeobserver(_:forkeypath:context:)</appledoc>
        [Export("removeObserver:forKeyPath:context:")]
                public virtual void RemoveObserver(NSObject observer, NSString keyPath, IntPtr context);

                public void RemoveObserver(NSObject observer, string keyPath, IntPtr context);

        /// <appledoc>https://developer.apple.com/documentation/foundation/nsarray/removeobserver(_:forkeypath:)</appledoc>
        [Export("removeObserver:forKeyPath:")]
                public virtual void RemoveObserver(NSObject observer, NSString keyPath);

                public void RemoveObserver(NSObject observer, string keyPath);

        /// <param name="sel">To be added.</param>
        /// <summary>To be added.</summary>
        /// <returns>To be added.</returns>
        /// <remarks>To be added.</remarks>
        /// <appledoc>https://developer.apple.com/documentation/foundation/nsproxy/responds(to:)</appledoc>
        [Export("respondsToSelector:")]
        [EditorBrowsable(EditorBrowsableState.Never)]
                public virtual bool RespondsToSelector(Selector? sel);

        [Export("setNilValueForKey:")]
                public virtual void SetNilValueForKey(NSString key);

        /// <appledoc>https://developer.apple.com/documentation/foundation/nsarray/setvalue(_:forkey:)</appledoc>
        [Export("setValue:forKey:")]
                public virtual void SetValueForKey(NSObject value, NSString key);

        [Export("setValue:forKeyPath:")]
                public virtual void SetValueForKeyPath(NSObject value, NSString keyPath);

        [Export("setValue:forUndefinedKey:")]
                public virtual void SetValueForUndefinedKey(NSObject value, NSString undefinedKey);

        [Export("setValuesForKeysWithDictionary:")]
                public virtual void SetValuesForKeysWithDictionary(NSDictionary keyedValues);

        /// <appledoc>https://developer.apple.com/documentation/foundation/nsarray/value(forkey:)</appledoc>
        [Export("valueForKey:")]
        public virtual NSObject ValueForKey(NSString key);

        [Export("valueForKeyPath:")]
        public virtual NSObject ValueForKeyPath(NSString keyPath);

        [Export("valueForUndefinedKey:")]
        public virtual NSObject ValueForUndefinedKey(NSString key);

        [Export("willChange:valuesAtIndexes:forKey:")]
        public virtual void WillChange(NSKeyValueChange changeKind, NSIndexSet indexes, NSString forKey);

        [Export("willChangeValueForKey:withSetMutation:usingObjects:")]
        public virtual void WillChange(
          NSString forKey,
          NSKeyValueSetMutationKind mutationKind,
          NSSet objects);

        [Export("willChangeValueForKey:")]
        public virtual void WillChangeValue(string forKey);

        public virtual NSAttributedString[] AccessibilityAttributedUserInputLabels { [Export("accessibilityAttributedUserInputLabels")] get; [Export("setAccessibilityAttributedUserInputLabels:", ArgumentSemantic.Copy)] set; }

        public virtual bool AccessibilityRespondsToUserInteraction { [Export("accessibilityRespondsToUserInteraction")] get; [Export("setAccessibilityRespondsToUserInteraction:")] set; }

        public virtual string? AccessibilityTextualContext { [Export("accessibilityTextualContext")] get; [Export("setAccessibilityTextualContext:", ArgumentSemantic.Retain)] set; }

        public virtual string[] AccessibilityUserInputLabels { [Export("accessibilityUserInputLabels", ArgumentSemantic.Retain)] get; [Export("setAccessibilityUserInputLabels:", ArgumentSemantic.Retain)] set; }

        /// <summary>To be added.</summary>
        /// <value>To be added.</value>
        /// <remarks>To be added.</remarks>
        /// <appledoc>https://developer.apple.com/documentation/foundation/nsproxy/class()</appledoc>
                [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual Class Class { [Export("class")] get; }

        /// <appledoc>https://developer.apple.com/documentation/foundation/nsproxy/debugdescription</appledoc>
                public virtual string DebugDescription { [Export("debugDescription")] get; }

        /// <summary>To be added.</summary>
        /// <value>To be added.</value>
        /// <remarks>To be added.</remarks>
        /// <appledoc>https://developer.apple.com/documentation/foundation/xmlnode/description</appledoc>
                public virtual string Description { [Export("description")] get; }

        /// <summary>To be added.</summary>
        /// <value>To be added.</value>
        /// <remarks>To be added.</remarks>
        /// <appledoc>https://developer.apple.com/documentation/foundation/nsurlprotectionspace/isproxy</appledoc>
                [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual bool IsProxy { [Export("isProxy")] get; }

        /// <summary>To be added.</summary>
        /// <value>To be added.</value>
        /// <remarks>To be added.</remarks>
                public virtual UIntPtr RetainCount { [Export("retainCount")] get; }

        /// <summary>To be added.</summary>
        /// <value>To be added.</value>
        /// <remarks>To be added.</remarks>
                [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual NSObject Self { [Export("self")] get; }

        /// <summary>To be added.</summary>
        /// <value>To be added.</value>
        /// <remarks>To be added.</remarks>
                [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual Class Superclass { [Export("superclass")] get; }

        /// <summary>To be added.</summary>
        /// <value>To be added.</value>
        /// <remarks>To be added.</remarks>
        /// <appledoc>https://developer.apple.com/documentation/foundation/nsgarbagecollector/zone</appledoc>
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

    /// <summary>Sentinel class used by the MonoTouch framework.</summary>
    public enum NSObjectFlag
    {
        /// <summary>Sentinel instance.</summary>
        Empty,
    }

    public class NSString :
        NSObject,
        IComparable<NSString>,
        INSCoding,
        INSCopying,
        INSItemProviderReading,
        INSItemProviderWriting,
        INSMutableCopying,
        INSSecureCoding,
        ICKRecordValue
    {
        /// <summary>An <see cref="T:Foundation.NSString" /> instance for an empty (zero-length) string.</summary>
        public static readonly NSString Empty = new ("");

        string _str;

        public NSString(string str)
        {
            _str = str;
        }

        public override string ToString() => _str;

        public static implicit operator string?(NSString? str) => str?._str;
        public static explicit operator NSString?(string? str) => str is not null ? new (str) : null;

        public static bool operator ==(NSString? a, NSString? b) => a?._str == b?._str;
        public static bool operator !=(NSString? a, NSString? b) => a?._str != b?._str;

        public bool Equals(NSString? other) => _str == other?._str;
        public override bool Equals(object? obj) => obj is NSString s && _str == s._str;
        public override int GetHashCode() => _str.GetHashCode();

        int IComparable<NSString>.CompareTo(NSString? other) => string.Compare(_str, other?._str, StringComparison.Ordinal);
    }

    /// <summary>Attribute applied to interfaces that represent Objective-C protocols.</summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public sealed class ProtocolAttribute : Attribute
    {
        /// <summary>The type of a specific managed type that can be used to wrap an instane of this protocol.</summary>
        public Type? WrapperType { get; set; }

        /// <summary>The name of the protocol.</summary>
        public string? Name { get; set; }

        /// <summary>Whether the Objective-C protocol is an informal protocol.</summary>
        public bool IsInformal { get; set; }

        public string? FormalSince { get; set; }

        /// <summary>
        ///   <para>This property indicates whether the binding generator will generate backwards-compatible code for the protocol in question.</para>
        ///   <para>In particular, if this property is true, then the binding generator will generate extension methods for optional members and <see cref="T:Foundation.ProtocolMemberAttribute" /> attributes on the protocol interface for all protocol members.</para>
        /// </summary>
        public bool BackwardsCompatibleCodeGeneration { get; set; }
    }

    /// <summary>Used to register a class to the Objective-C runtime.</summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class RegisterAttribute : Attribute
    {
        // public RegisterAttribute();

        /// <param name="name">The name to use when exposing this class to the Objective-C world.</param>
        /// <summary>Used to specify how the ECMA class is exposed as an Objective-C class.</summary>
        public RegisterAttribute(string name)
        {
            Name = name;
        }

        /// <summary>Used to specify how the ECMA class is exposed as an Objective-C class.</summary>
        /// <param name="name">The name to use when exposing this class to the Objective-C world.</param>
        /// <param name="isWrapper">Used to specify if the class being registered is wrapping an existing Objective-C class, or if it's a new class.</param>
        public RegisterAttribute(string name, bool isWrapper)
        {
            Name = name;
            IsWrapper = isWrapper;
        }

        /// <summary>The name used to expose the class.</summary>
        public string? Name { get; set; }

        /// <summary>Specifies whether the class being registered is wrapping an existing Objective-C class, or if it's a new class.</summary>
        /// <value>True if the class being registered is wrapping an existing Objective-C class.</value>
        public bool IsWrapper { get; set; }

        public bool SkipRegistration { get; set; }

        /// <summary>
        /// Specifies whether the Objective-C class is a stub class.
        /// Objective-C stub classes are sometimes used when bridging Swift to Objective-C.
        /// </summary>
        public bool IsStubClass { get; set; }
    }
}

#endif
