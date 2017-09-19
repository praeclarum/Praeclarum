using System;
using AppKit;
using System.Drawing;

namespace Praeclarum.UI
{
	public class UserInterfaceWindow : NSWindow
	{
//		UserInterface ui;

		public UserInterfaceWindow (UserInterface ui, RectangleF frame, NSScreen screen)
			: base (frame,
			        NSWindowStyle.Titled | NSWindowStyle.Resizable | NSWindowStyle.Closable | NSWindowStyle.Miniaturizable,
			        NSBackingStore.Buffered,
			        false,
			        screen)
		{
//			this.ui = ui;

			var view = ui.View as NSView;
			if (view != null) {

				this.ContentView = view;

			}
		}
	}
}

