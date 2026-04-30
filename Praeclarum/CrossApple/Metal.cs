#nullable enable

using System;

#if !__IOS__ && !__MACOS__ && !__TVOS__ && !__MACCATALYST__

using CoreGraphics;
using Foundation;
using ObjCRuntime;

// ReSharper disable InconsistentNaming

namespace Metal
{
	public interface IMTLDevice : INativeObject, IDisposable
	{
		string Name { get; }
		IMTLCommandQueue CreateCommandQueue ();
		IMTLLibrary CreateDefaultLibrary ();
		IMTLBuffer? CreateBuffer (nuint length, MTLResourceOptions options);
		IMTLTexture? CreateTexture (MTLTextureDescriptor descriptor);
	}

	public interface IMTLCommandQueue : INativeObject, IDisposable
	{
		IMTLCommandBuffer CommandBuffer ();
	}

	public interface IMTLCommandBuffer : INativeObject, IDisposable
	{
		void Commit ();
		void WaitUntilCompleted ();
	}

	public interface IMTLLibrary : INativeObject, IDisposable { }

	public interface IMTLBuffer : INativeObject, IDisposable
	{
		nuint Length { get; }
		IntPtr Contents { get; }
	}

	public interface IMTLTexture : INativeObject, IDisposable
	{
		nuint Width { get; }
		nuint Height { get; }
		MTLPixelFormat PixelFormat { get; }
		void ReplaceRegion (MTLRegion region, nuint mipmapLevel, IntPtr pixelBytes, nuint bytesPerRow);
		void ReplaceRegion (MTLRegion region, nuint level, nuint slice, IntPtr pixelBytes, nuint bytesPerRow, nuint bytesPerImage);
	}

	public struct MTLOrigin
	{
		public nuint X, Y, Z;
		public MTLOrigin (nuint x, nuint y, nuint z) { X = x; Y = y; Z = z; }
		public MTLOrigin (int x, int y, int z) : this ((nuint)x, (nuint)y, (nuint)z) { }
	}

	public struct MTLSize
	{
		public nuint Width, Height, Depth;
		public MTLSize (nuint w, nuint h, nuint d) { Width = w; Height = h; Depth = d; }
		public MTLSize (int w, int h, int d) : this ((nuint)w, (nuint)h, (nuint)d) { }
	}

	public struct MTLRegion
	{
		public MTLOrigin Origin;
		public MTLSize Size;
		public MTLRegion (MTLOrigin origin, MTLSize size) { Origin = origin; Size = size; }
	}

	public enum MTLLoadAction : ulong { DontCare, Load, Clear }
	public enum MTLStoreAction : ulong { DontCare, Store, MultisampleResolve, StoreAndMultisampleResolve, Unknown, CustomSampleDepthStore }

	public class MTLRenderPassAttachmentDescriptor : NSObject
	{
		public IMTLTexture? Texture { get; set; }
		public nuint Level { get; set; }
		public nuint Slice { get; set; }
		public nuint DepthPlane { get; set; }
		public IMTLTexture? ResolveTexture { get; set; }
		public MTLLoadAction LoadAction { get; set; }
		public MTLStoreAction StoreAction { get; set; }
	}

	public class MTLRenderPassColorAttachmentDescriptor : MTLRenderPassAttachmentDescriptor
	{
		public MTLClearColor ClearColor { get; set; }
	}

	public class MTLRenderPassDepthAttachmentDescriptor : MTLRenderPassAttachmentDescriptor
	{
		public double ClearDepth { get; set; } = 1.0;
	}

	public class MTLRenderPassStencilAttachmentDescriptor : MTLRenderPassAttachmentDescriptor
	{
		public uint ClearStencil { get; set; }
	}

	public struct MTLClearColor
	{
		public double Red, Green, Blue, Alpha;
		public MTLClearColor (double r, double g, double b, double a) { Red = r; Green = g; Blue = b; Alpha = a; }
	}

	public class MTLRenderPassColorAttachmentDescriptorArray
	{
		private readonly MTLRenderPassColorAttachmentDescriptor[] _items =
			new[] { new MTLRenderPassColorAttachmentDescriptor (), new MTLRenderPassColorAttachmentDescriptor (), new MTLRenderPassColorAttachmentDescriptor (), new MTLRenderPassColorAttachmentDescriptor () };
		public MTLRenderPassColorAttachmentDescriptor this [nint index]
		{
			get => _items[(int)index];
			set => _items[(int)index] = value;
		}
	}

	public class MTLRenderPassDescriptor : NSObject
	{
		public MTLRenderPassColorAttachmentDescriptorArray ColorAttachments { get; } = new ();
		public MTLRenderPassDepthAttachmentDescriptor DepthAttachment { get; set; } = new ();
		public MTLRenderPassStencilAttachmentDescriptor StencilAttachment { get; set; } = new ();
		public static MTLRenderPassDescriptor CreateRenderPassDescriptor () => new ();
	}

	public class MTLDevice : NSObject, IMTLDevice
	{
		public string Name { get; set; } = "Stub";
		public IMTLCommandQueue CreateCommandQueue () => new MTLCommandQueueStub ();
		public IMTLLibrary CreateDefaultLibrary () => new MTLLibraryStub ();
		public IMTLBuffer? CreateBuffer (nuint length, MTLResourceOptions options) => null;
		public IMTLTexture? CreateTexture (MTLTextureDescriptor descriptor) => null;
		public static IMTLDevice? SystemDefault => null;
		public static IMTLDevice[] GetAllDevices () => Array.Empty<IMTLDevice> ();
	}

	internal class MTLCommandQueueStub : NSObject, IMTLCommandQueue
	{
		public IMTLCommandBuffer CommandBuffer () => throw new PlatformNotSupportedException ();
	}

	internal class MTLLibraryStub : NSObject, IMTLLibrary { }

	public static class MTLDeviceExtensions
	{
		public static IMTLDevice? GetSystemDefault () => null;
	}

	public class MTLTextureDescriptor : NSObject
	{
		public MTLPixelFormat PixelFormat { get; set; }
		public nuint Width { get; set; }
		public nuint Height { get; set; }
		public nuint Depth { get; set; } = 1;
		public nuint MipmapLevelCount { get; set; } = 1;
		public nuint SampleCount { get; set; } = 1;
		public nuint ArrayLength { get; set; } = 1;
		public MTLResourceOptions ResourceOptions { get; set; }
		public MTLTextureUsage Usage { get; set; }
		public MTLStorageMode StorageMode { get; set; }
		public MTLTextureType TextureType { get; set; }
		public static MTLTextureDescriptor CreateTexture2DDescriptor (MTLPixelFormat format, nuint width, nuint height, bool mipmapped) =>
			new () { PixelFormat = format, Width = width, Height = height };
	}

	public enum MTLPixelFormat : ulong
	{
		Invalid = 0,
		R32Float = 55,
		BGRA8Unorm = 80,
		BGRA8Unorm_sRGB = 81,
		RGBA8Unorm = 70,
		RGBA8Unorm_sRGB = 71,
		Depth32Float = 252,
	}

	[Flags]
	public enum MTLResourceOptions : ulong
	{
		CpuCacheModeDefault = 0,
		StorageModeShared = 0,
		StorageModePrivate = 16,
	}

	public enum MTLStorageMode : ulong { Shared, Managed, Private, Memoryless }

	public enum MTLTextureType : ulong { k1D, k1DArray, k2D, k2DArray, k2DMultisample, kCube, kCubeArray, k3D }

	[Flags]
	public enum MTLTextureUsage : ulong
	{
		Unknown = 0,
		ShaderRead = 1,
		ShaderWrite = 2,
		RenderTarget = 4,
	}
}

#endif
