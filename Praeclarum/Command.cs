using System;
using System.Collections.Generic;
using System.Linq;

namespace Praeclarum
{
	public class Command
	{
		public string Name { get; set; }

		public Action Action { get; set; }

		public Command (string name, Action action = null)
		{
			Name = name;
			Action = action;
		}

		public virtual void Execute ()
		{
			if (Action != null)
				Action ();
		}
	}


}

