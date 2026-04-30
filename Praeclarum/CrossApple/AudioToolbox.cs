#nullable enable

using System;

#if !__IOS__ && !__MACOS__ && !__TVOS__ && !__MACCATALYST__

using Foundation;
using ObjCRuntime;

// ReSharper disable InconsistentNaming

namespace AudioToolbox
{
	public struct AudioStreamBasicDescription
	{
		public double SampleRate;
		public AudioFormatType Format;
		public AudioFormatFlags FormatFlags;
		public uint BytesPerPacket;
		public uint FramesPerPacket;
		public uint BytesPerFrame;
		public uint ChannelsPerFrame;
		public uint BitsPerChannel;
		public uint Reserved;
	}

	public enum AudioFormatType : uint
	{
		LinearPCM = 0x6c70636d,
	}

	[Flags]
	public enum AudioFormatFlags : uint
	{
		IsFloat = 1 << 0,
		IsBigEndian = 1 << 1,
		IsSignedInteger = 1 << 2,
		IsPacked = 1 << 3,
		IsAlignedHigh = 1 << 4,
		IsNonInterleaved = 1 << 5,
		IsNonMixable = 1 << 6,
	}

	public class BufferCompletedEventArgs : EventArgs
	{
		public IntPtr IntPtrBuffer { get; set; }
	}

	public class InputCompletedEventArgs : EventArgs
	{
		public IntPtr IntPtrBuffer { get; set; }
	}

	public abstract class AudioQueue : NSObject
	{
		public static void FillAudioData (IntPtr audioQueueBuffer, int offset, IntPtr source, int sourceOffset, int count) { }
		public virtual void Start () { }
		public virtual void Stop (bool immediate) { }
		public virtual void AllocateBuffer (int byteSize, out IntPtr buffer) { buffer = IntPtr.Zero; }
		public virtual void FreeBuffer (IntPtr buffer) { }
		public virtual void EnqueueBuffer (IntPtr audioQueueBuffer, int byteSize, NSObject? packetDesc) { }
	}

	public class OutputAudioQueue : AudioQueue
	{
		public OutputAudioQueue (AudioStreamBasicDescription desc) { }
		public event EventHandler<BufferCompletedEventArgs>? BufferCompleted;
		protected void OnBufferCompleted (BufferCompletedEventArgs e) => BufferCompleted?.Invoke (this, e);
	}

	public class InputAudioQueue : AudioQueue
	{
		public InputAudioQueue (AudioStreamBasicDescription desc) { }
		public event EventHandler<InputCompletedEventArgs>? InputCompleted;
		protected void OnInputCompleted (InputCompletedEventArgs e) => InputCompleted?.Invoke (this, e);
	}
}

#endif
