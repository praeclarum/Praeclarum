using System.IO;

namespace Praeclarum
{
	public interface IConsole
	{
		TextReader In { get; }
		TextWriter Out { get; }
	}
}

