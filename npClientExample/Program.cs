using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using static NamedPipeExample.npServer.NamedPipeServerAsync;

namespace npClient
{
	class Program
	{
		static void Main(string[] args)
		{
			var cli = new NamedPipeClientStream(".", "PingPong", PipeDirection.InOut);

			Console.WriteLine("Connecting to server...\n");
			cli.Connect();
			StreamString ss = new StreamString(cli);
			ss.WriteString("PING");
			var received = ss.ReadString();
			System.Diagnostics.Debug.Assert(received.StartsWith("PONG"));			
			cli.Close();

		}
	}
}
