#nullable enable

using System;

#if !__IOS__ && !__MACOS__ && !__TVOS__ && !__MACCATALYST__

using Foundation;
using ObjCRuntime;

// ReSharper disable InconsistentNaming

namespace AVFoundation
{
	public class AVAudioEngine : NSObject
	{
		public AVAudioNode MainMixerNode { get; } = new AVAudioMixerNode ();
		public AVAudioNode OutputNode { get; } = new AVAudioOutputNode ();
		public bool Running { get; private set; }
		public void AttachNode (AVAudioNode node) { }
		public void DetachNode (AVAudioNode node) { }
		public void Connect (AVAudioNode src, AVAudioNode dst, AVAudioFormat? format) { }
		public bool StartAndReturnError (out NSError? error) { error = null; Running = true; return true; }
		public void Stop () { Running = false; }
		public void Pause () { }
		public void Reset () { }
	}

	public class AVAudioNode : NSObject { }
	public class AVAudioMixerNode : AVAudioNode { }
	public class AVAudioOutputNode : AVAudioNode { }

	public class AVAudioPlayerNode : AVAudioNode
	{
		public bool Playing { get; private set; }
		public void Play () { Playing = true; }
		public void Pause () { Playing = false; }
		public void Stop () { Playing = false; }
		public void ScheduleBuffer (AVAudioPcmBuffer buffer, AVAudioTime? when, AVAudioPlayerNodeBufferOptions options, Action? completionHandler) =>
			completionHandler?.Invoke ();
		public void ScheduleBuffer (AVAudioPcmBuffer buffer, Action? completionHandler) => completionHandler?.Invoke ();
	}

	public class AVAudioFormat : NSObject
	{
		public AVAudioCommonFormat CommonFormat { get; }
		public double SampleRate { get; }
		public uint ChannelCount { get; }
		public bool Interleaved { get; }
		public AVAudioFormat (AVAudioCommonFormat format, double sampleRate, uint channels, bool interleaved)
		{
			CommonFormat = format;
			SampleRate = sampleRate;
			ChannelCount = channels;
			Interleaved = interleaved;
		}
	}

	public enum AVAudioCommonFormat : ulong
	{
		Other, PCMFloat32, PCMFloat64, PCMInt16, PCMInt32,
	}

	public class AVAudioPcmBuffer : NSObject
	{
		public AVAudioFormat Format { get; }
		public uint FrameCapacity { get; }
		public uint FrameLength { get; set; }
		public IntPtr FloatChannelData { get; set; }
		public IntPtr Int16ChannelData { get; set; }
		public IntPtr Int32ChannelData { get; set; }
		public AVAudioPcmBuffer (AVAudioFormat format, uint frameCapacity)
		{
			Format = format;
			FrameCapacity = frameCapacity;
		}
	}

	public class AVAudioTime : NSObject { }

	[Flags]
	public enum AVAudioPlayerNodeBufferOptions : ulong
	{
		Loops = 1,
		Interrupts = 2,
		InterruptsAtLoop = 4,
	}
}

#endif
