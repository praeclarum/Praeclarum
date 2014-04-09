using System;

namespace Praeclarum.UI
{
	public interface ITextEditor : IView
	{
		void Modify (Action action);

		StringRange SelectedRange { get; set; }
		void ReplaceText (StringRange range, string text);

	}
}

