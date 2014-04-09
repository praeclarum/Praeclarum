using System;
using Praeclarum.Graphics;

namespace Praeclarum.UI
{
	public interface IView
	{
		Color BackgroundColor { get; set; }

		RectangleF Bounds { get; }
	}
}

