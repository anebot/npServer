using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;

namespace NamedPipeExample.npServer
{
	public class PingPongServer : NamedPipeServerAsync, IRemoteNamedPipeServer
	{
		public event EventHandler<LogMessageEventArgs> LogEvent;
		
		public PingPongServer(String EntryPoint) : base(EntryPoint) {
		}
		
		/// <summary>
		/// When client is connected and we can interactuate with it.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected override void OnClientConnectionEstablished(object sender, RequestDigestEventArgs e)
		{
			String answer = "";
			Boolean isSuccess = false;
			try
			{
				String receivedToken = e.StreamString.ReadString();
				LogMessage(LogMessageEventArgs.Severity.Info, $"'{receivedToken}' received!");
				if (receivedToken.ToUpper() == "PING")
				{
					answer = "PONG";
				}
				else {
					answer = "COMMAND NOT FOUND";
				}
				isSuccess = true;
			}
			catch (Exception ex)
			{
				LogMessage(LogMessageEventArgs.Severity.Error, $"Exception '{ex.Message}'!");
			}
			finally
			{
				try
				{
					String responseMessage = "";
					responseMessage = (isSuccess) ? answer : "FATAL ERROR";
					e.StreamString.WriteString(responseMessage);
				}
				catch (Exception)
				{
					LogMessage(LogMessageEventArgs.Severity.Error, "Connection lost! Connection closed by client!");
				}
			}
		}

		private void LogMessage(LogMessageEventArgs.Severity Severity,String Message) {
			this.LogEvent?.Invoke(this, new LogMessageEventArgs(Severity,Message));
		}

	}

	public class LogMessageEventArgs : EventArgs
	{
		public enum Severity { Debug, Info ,  Warning, Error };

		public Severity MsgSeverity;
		public String Message { get; set; }
		public LogMessageEventArgs(Severity MsgSeverity, String Message) { this.Message = Message; this.MsgSeverity = MsgSeverity; }
	}
	
}
