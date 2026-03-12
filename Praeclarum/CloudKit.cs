#nullable enable

using System;
using System.Runtime.Versioning;

#if !__IOS__ && !__MACOS__ && !__TVOS__ && !__MACCATALYST__

using Foundation;
using ObjCRuntime;

namespace CloudKit
{
    [SupportedOSPlatform("maccatalyst")]
    [SupportedOSPlatform("ios")]
    [SupportedOSPlatform("macos")]
    [SupportedOSPlatform("tvos")]
    [Protocol(Name = "CKRecordValue", WrapperType = typeof(CKRecordValueWrapper))]
    public interface ICKRecordValue : INativeObject, IDisposable
    {
    }
}

#endif
