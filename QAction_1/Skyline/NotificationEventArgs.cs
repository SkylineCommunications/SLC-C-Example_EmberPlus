namespace QAction_1.Skyline
{
	using System;

	public class NotificationEventArgs : EventArgs
	{
		public NotificationEventArgs(string message)
		{
			Message = message;
		}

		public string Message { get; }
	}
}