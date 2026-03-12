#nullable enable

using System;
using System.Runtime.Versioning;

#if !__IOS__ && !__MACOS__ && !__TVOS__ && !__MACCATALYST__

using Foundation;
using ObjCRuntime;

namespace CloudKit
{
    [Protocol(Name = "CKRecordValue")]
    public interface ICKRecordValue : INativeObject, IDisposable
    {
    }
}

#endif
