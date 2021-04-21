using System;
using FCData.Attributes;
using FCData.Common;

namespace Base
{
	[FCRegister(true)]
	public class FCUnityLogger : IFCLogger
	{
		public void Log(object sender, string message, LogLevel logLevel)
		{
			string name = sender.GetType().Name;
			switch (logLevel)
			{
				case LogLevel.EditorInfo:
					EditorInfo("Editor", message);
					break;
				case LogLevel.Debug:
					Debug(name, message);
					break;
				case LogLevel.Info:
					Info(name, message);
					break;
				case LogLevel.Warning:
					Warning(name, message);
					break;
				case LogLevel.Error:
					Error(name, message);
					break;
				case LogLevel.ServiceNetworkError:
					ServiceNetworkError(name, message);
					break;
				case LogLevel.CheatingError:
					CheatingError(name, message);
					break;
				case LogLevel.ToDo:
					ToDo(name, message);
					break;
				case LogLevel.DevelopmentInfo:
					DevelopmentInfo(name, message);
					break;
			}
		}

		public void Log(object sender, Exception e)
		{
			Log(sender, e.ToString(), LogLevel.Error);
		}

		private void Debug(string senderName, string message)
		{
			UnityEngine.Debug.LogFormat("<color=green>{0} </color>: {1}", senderName, message);
		}

		private void Info(string senderName, string message)
		{
			UnityEngine.Debug.LogFormat("<color=green>{0} </color>: {1}", senderName, message);
		}

		private void Warning(string senderName, string message)
		{
			UnityEngine.Debug.LogWarningFormat("<color=yellow>{0} </color>: {1}", senderName, message);
		}

		private void Error(string senderName, string message)
		{
			UnityEngine.Debug.LogErrorFormat("<color=red>{0} </color>: {1}", senderName, message);
		}

		private void ServiceNetworkError(string senderName, string message)
		{
			UnityEngine.Debug.LogErrorFormat("<color=magenta>{0} </color>: {1}", senderName, message);
		}

		private void CheatingError(string senderName, string message)
		{
			UnityEngine.Debug.LogFormat("<color=orange>{0} </color>: {1}", senderName, message);
		}

		private void ToDo(string senderName, string message)
		{
			UnityEngine.Debug.LogFormat("<color=purple>{0} </color>: {1}", senderName, message);
		}

		private void DevelopmentInfo(string senderName, string message)
		{
			UnityEngine.Debug.LogFormat("<color=pink>{0} </color>: {1}", senderName, message);
		}

		private void EditorInfo(string senderName, string message)
		{
			UnityEngine.Debug.LogFormat("<color=cyan>{0} </color>: {1}", senderName, message);
		}
	}
}