using NamedPipeExample.npServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NamedPipeExample
{
	class Program
	{
		static void Main(string[] args)
		{
			IRemoteNamedPipeServer serv = new PingPongServer("PingPong");
			serv.LogEvent += (sender, e) => { Console.WriteLine(e.Message); };
			
			Console.WriteLine("Press any key to stop...");
			Console.ReadKey();

		}
	}
}
