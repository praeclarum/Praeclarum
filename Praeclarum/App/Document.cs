using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.IO;

namespace Praeclarum.App
{
	public abstract class Document : INotifyPropertyChanged
	{
		public event EventHandler<CommittedEventArgs> Committed;

		public string Title { get; private set; }

		public Document ()
		{
			Title = "Untitled";
		}

		#region File Operations

		public void Open (string path, TextReader reader)
		{
			var state = reader.ReadToEnd ();
			RestoreState (state);
			InitializeUndo ();
		}

		public void Save (TextWriter writer)
		{
			var state = GetState ();
			writer.Write (state);
			HasUnsavedChanges = false;
		}

#if !PORTABLE

		public string Path { get; set; }

		public void Open (string path)
		{
			Path = path;
			Title = System.IO.Path.GetFileNameWithoutExtension (Path);
			var state = File.ReadAllText (path);
			RestoreState (state);
			InitializeUndo ();
		}

		public void Save ()
		{
			Save (Path);
		}

		public void Save (string path)
		{
			Path = path;
			Title = System.IO.Path.GetFileNameWithoutExtension (Path);
			var state = GetState ();
			File.WriteAllText (path, state, Encoding.UTF8);
			HasUnsavedChanges = false;
		}
#endif

		#endregion

		#region Undo

		bool hasUnsavedChanges = false;

		public bool HasUnsavedChanges
		{
			get { return hasUnsavedChanges; }
			set
			{
				if (hasUnsavedChanges == value)
					return;
				hasUnsavedChanges = value;
				OnPropertyChanged ("HasUnsavedChanges");
			}
		}

		public void InitializeUndo ()
		{
			commitStates.Clear ();
			activeCommitState = -1;
			Commit ("Initial");
			HasUnsavedChanges = false;
		}

		class CommitState
		{
			public string Message;
			public string State;
		}

		readonly List<CommitState> commitStates = new List<CommitState> ();
		int activeCommitState = -1;

		protected abstract string GetState ();

		protected abstract void RestoreState (string state);

		public void Commit (string message)
		{
			//
			// Serialize the document
			//
			var state = GetState ();

			//
			// Don't commit non-changes
			//
			if (commitStates.Count != 0 &&
			    state == commitStates[activeCommitState].State) return;

			//
			// Remove undone states
			//
			if (commitStates.Count - 1 > activeCommitState) {
				commitStates.RemoveRange (
					activeCommitState + 1,
					commitStates.Count - activeCommitState - 1);
			}

			//
			// Set this as the active state
			//
			commitStates.Add (new CommitState {
				State = state,
				Message = message,
			});
			activeCommitState = commitStates.Count - 1;

			//
			// Notify
			//
			HasUnsavedChanges = true;

			var ev = Committed;
			if (ev != null) {
				var prevState = activeCommitState > 0 ?
					commitStates[activeCommitState-1].State :
						"";
				ev (this, new CommittedEventArgs (message, prevState, state));
			}

			OnPropertyChanged ("CanUndo");
			OnPropertyChanged ("CanRedo");
		}

		public bool CanUndo { get { return activeCommitState > 0; } }

		public string UndoMessage { get { return CanUndo ? commitStates[activeCommitState].Message : ""; } }

		public void Undo ()
		{
			if (CanUndo) {
				activeCommitState--;

				RestoreState (commitStates[activeCommitState].State);

				HasUnsavedChanges = true;

				OnPropertyChanged ("CanUndo");
				OnPropertyChanged ("CanRedo");
			}
		}

		public bool CanRedo { get { return activeCommitState + 1 < commitStates.Count; } }

		public string RedoMessage { get { return CanRedo ? commitStates[activeCommitState + 1].Message : ""; } }

		public void Redo ()
		{
			if (CanRedo) {
				activeCommitState++;

				RestoreState (commitStates[activeCommitState].State);

				HasUnsavedChanges = true;

				OnPropertyChanged ("CanUndo");
				OnPropertyChanged ("CanRedo");
			}
		}

		#endregion

		public event PropertyChangedEventHandler PropertyChanged;

		void OnPropertyChanged (string name)
		{
			var ev = PropertyChanged;
			if (ev != null) {
				ev (this, new PropertyChangedEventArgs (name));
			}
		}
	}

	public class CommittedEventArgs : EventArgs
	{
		public string Message { get; private set; }
		public string PreviousState { get; private set; }
		public string State { get; private set; }
		public CommittedEventArgs (string message, string previousState, string state)
		{
			Message = message;
			PreviousState = previousState;
			State = state;
		}
	}
}

