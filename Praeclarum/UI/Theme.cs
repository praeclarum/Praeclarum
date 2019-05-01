using System;

namespace Praeclarum.UI
{
#if !(__IOS__ || __MACOS__)
	public class Theme
	{
		public Graphics.Color DocumentBackgroundColor = Graphics.Color.FromWhite (0.0f, 0.0f);
	}
#endif
}
