using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NamedPipeExample.npServer
{
	public interface IRemoteNamedPipeServer
	{
		void Run();
		Boolean Stop();
		event EventHandler<LogMessageEventArgs> LogEvent;
		string EntryPoint { get; }
	}
}
