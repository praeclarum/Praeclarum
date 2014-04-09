using System;
using System.Collections.Generic;

namespace Praeclarum
{
	public delegate object NativeConstructor ();

	public static class Native
	{
		static readonly Dictionary<Type, NativeConstructor> registry =
			new Dictionary<Type, NativeConstructor> ();

		public static void Register<T> (NativeConstructor constructor)
			where T : INative
		{
			var type = typeof (T);
			registry [type] = constructor;
		}

		public static T Create<T> ()
			where T : INative
		{
			var type = typeof (T);
			NativeConstructor ctor;
			if (!registry.TryGetValue (type, out ctor))
				throw new ArgumentException ("Cannot create native objects of type " + type.FullName);
			var obj = ctor ();
			if (obj == null)
				throw new Exception ("Native object constructor returned null for " + type.FullName);
			return (T)obj;
		}
	}
}

