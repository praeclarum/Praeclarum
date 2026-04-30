#nullable enable

using System;

#if !__IOS__ && !__MACOS__ && !__TVOS__ && !__MACCATALYST__

// ReSharper disable InconsistentNaming

namespace MobileCoreServices
{
	public static class UTType
	{
		public const string Item = "public.item";
		public const string Content = "public.content";
		public const string Data = "public.data";
		public const string Image = "public.image";
		public const string Audio = "public.audio";
		public const string Video = "public.movie";
		public const string Movie = "public.movie";
		public const string PNG = "public.png";
		public const string JPEG = "public.jpeg";
		public const string PDF = "com.adobe.pdf";
		public const string PlainText = "public.plain-text";
		public const string Text = "public.text";
		public const string UTF8PlainText = "public.utf8-plain-text";
		public const string Folder = "public.folder";
		public const string FileURL = "public.file-url";
		public const string URL = "public.url";
		public const string ZipArchive = "public.zip-archive";
		public const string ThreeDObject = "public.3d-content";
		public const string USDZ = "com.pixar.universal-scene-description-mobile";
	}
}

#endif
