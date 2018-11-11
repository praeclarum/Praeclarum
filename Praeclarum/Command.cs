using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Praeclarum
{
	public delegate Task AsyncAction ();

	public class Command
	{
		public string Name { get; set; }

		public AsyncAction Action { get; set; }

		public Command (string name, AsyncAction action = null)
		{
			Name = name.Localize ();
			Action = action;
		}

		public virtual async Task ExecuteAsync ()
		{
			if (Action != null)
				await Action ();
		}
	}


}

