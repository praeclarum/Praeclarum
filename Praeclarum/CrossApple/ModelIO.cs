#nullable enable

using System;

#if !__IOS__ && !__MACOS__ && !__TVOS__ && !__MACCATALYST__

using CoreGraphics;
using Foundation;
using ObjCRuntime;

// ReSharper disable InconsistentNaming

namespace ModelIO
{
	public class MDLAsset : NSObject
	{
		public MDLAsset () { }
		public MDLAsset (NSUrl url) { }
		public nuint Count { get; set; }
		public MDLObject? this [nuint index] => null;
		public MDLObject? GetObject (nuint index) => null;
		public void AddObject (MDLObject obj) { }
		public bool ExportAssetToUrl (NSUrl url) => false;
		public static string[] CanImportFileExtensions { get; } = Array.Empty<string> ();
		public static string[] CanExportFileExtensions { get; } = Array.Empty<string> ();
	}

	public class MDLObject : NSObject
	{
		public string? Name { get; set; }
		public MDLObject? Parent { get; set; }
		public MDLTransform? Transform { get; set; }
		public MDLObject[] Children { get; set; } = Array.Empty<MDLObject> ();
		public void AddChild (MDLObject child) { }
	}

	public class MDLMesh : MDLObject
	{
		public MDLMesh () { }
		public static MDLMesh CreateBox (Vector3 dimensions, Vector3I segments, MDLGeometryType geometryType, bool inwardNormals, MDLMeshBufferAllocator? allocator) => new ();
	}

	public class MDLTransform : NSObject
	{
	}

	public abstract class MDLMeshBufferAllocator : NSObject { }

	public struct Vector3 { public float X, Y, Z; }
	public struct Vector3I { public int X, Y, Z; }

	public enum MDLGeometryType : long
	{
		Points, Lines, Triangles, TriangleStrips, Quads, VariableTopology,
	}

	public enum MDLAxis : long { X, Y, Z }
}

#endif
