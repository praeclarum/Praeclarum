#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

// ReSharper disable InconsistentNaming

namespace Praeclarum.App;

public class AIChat
{
	public ObservableCollection<Message> Messages { get; } = [];

	public class Message : INotifyPropertyChanged
	{
		private string _text = "";
		private MessageType _type = MessageType.Assistant;

		public string Text
		{
			get => _text;
			set
			{
				if (value == _text)
				{
					return;
				}
				_text = value;
				OnPropertyChanged ();
			}
		}

		public MessageType Type
		{
			get => _type;
			set
			{
				if (value == _type)
				{
					return;
				}
				_type = value;
				OnPropertyChanged ();
				OnPropertyChanged (nameof(IsSystem));
				OnPropertyChanged (nameof(IsUser));
				OnPropertyChanged (nameof(IsAssistant));
				OnPropertyChanged (nameof(IsError));
			}
		}

		public bool IsSystem => Type == MessageType.System;
		public bool IsUser => Type == MessageType.User;
		public bool IsAssistant => Type == MessageType.Assistant;
		public bool IsError => Type == MessageType.Error;
		public event PropertyChangedEventHandler? PropertyChanged;

		protected virtual void OnPropertyChanged ([CallerMemberName] string? propertyName = null)
		{
			PropertyChanged?.Invoke (this, new PropertyChangedEventArgs (propertyName));
		}
	}

	public enum MessageType
	{
		System,
		User,
		Assistant,
		Error
	}
}

public class AIChatHistory : INotifyPropertyChanged
{
	private int _activeChatIndex = 0;
	public ObservableCollection<AIChat> Chats { get; } = [new ()];

	public int ActiveChatIndex
	{
		get => _activeChatIndex;
		set
		{
			var safeValue = Math.Clamp(value, 0, Chats.Count - 1);
			if (safeValue == _activeChatIndex)
			{
				return;
			}

			_activeChatIndex = safeValue;
			OnPropertyChanged ();
			OnPropertyChanged (nameof(ActiveChat));
		}
	}

	public AIChat ActiveChat => Chats[ActiveChatIndex];
	public event PropertyChangedEventHandler? PropertyChanged;

	protected virtual void OnPropertyChanged ([CallerMemberName] string? propertyName = null)
	{
		PropertyChanged?.Invoke (this, new PropertyChangedEventArgs (propertyName));
	}
}
