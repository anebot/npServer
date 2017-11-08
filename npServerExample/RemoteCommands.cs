using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DTV.Capture.RemoteControl
{
	/*
		HOW ADD A NEW COMMAND : 
		- Add REMOTE_COMMAND enum entry.
		- Build a new Command class by implementing IRemoteCommand and define the desired behaviour on Run() method.
		- Build your custom Command Builder inheriting from RemoteCommandBuilder, and set all required resources.
		 */

	// Enum with command definitions
	enum REMOTE_COMMAND
	{
		UKNONW
		, MUX // MUX {IdMux} {START|STOP}
	};
	
	public interface IRemoteCommand
	{
		RemoteCommandResponse Run();
	}

	public class MuxControlCommand : IRemoteCommand
	{
		public IList<TunerController> TunnerController;
		public int MuxId { get; set; }
		public Mux_action Action { get; set; }
		public enum Mux_action { START, STOP };
		public RemoteCommandResponse Run()
		{
			var response = new RemoteCommandResponse();
			try
			{
				var tc = this.TunnerController.FirstOrDefault(t => t.MUXID == this.MuxId);
				if (tc == null)
				{
					throw new ArgumentException($"Mux '{this.MuxId}' not found!");
				}
				tc.CanRun = (this.Action == Mux_action.START) ? true : false;
				switch (this.Action)
				{
					case Mux_action.START:
						tc.CanRun = true;
						break;
					case Mux_action.STOP:
						tc.CanRun = false;
						tc.Dispose();
						break;
				}
				response.Success = true;
			}
			catch (Exception ex)
			{
				response.Message = ex.Message;
				response.Success = false;
			}

			return response;
		}
	}
	public class RemoteCommandResponse
	{
		public Boolean Success = false;
		public String Message = String.Empty;
	}
	/// <summary>
	/// Parse command string and generate IRemoteCommand with command to execute.
	/// </summary>
	public class RemoteCommandBuilder
	{
		public IRemoteCommand Command { get; protected set; }
		public RemoteCommandBuilder(String Command)
		{

		}

		/// <summary>
		/// Parse #1 commandStr token which contains the remote command and returns REMOTE_COMMAND enum entry.
		/// </summary>
		/// <param name="commandStr"></param>
		/// <returns></returns>
		internal static REMOTE_COMMAND GetCommand(String commandStr)
		{
			REMOTE_COMMAND cmd;
			if (String.IsNullOrEmpty(commandStr))
			{
				throw new ArgumentNullException("Command is null");
			}
			try
			{
				var actionStr = commandStr.Split(' ')[0];
				cmd = (REMOTE_COMMAND)Enum.Parse(typeof(REMOTE_COMMAND), actionStr);
			}
			catch (ArgumentException)
			{
				//throw new UknownCommand($"Uknown Command '{commandStr}'");
				cmd = REMOTE_COMMAND.UKNONW;
			}
			return cmd;
		}
	}

	public class RemoteMuxCommandBuilder : RemoteCommandBuilder
	{
		public RemoteMuxCommandBuilder(String Command, IList<TunerController> tunerController) : base(Command)
		{
			IRemoteCommand myCommand = null;
			if (String.IsNullOrEmpty(Command))
			{
				throw new ArgumentNullException("Command is null");
			}
			var parts = Command.Split(' ');
			if (parts.Length < 3)
			{
				throw new ArgumentException("Invalid number of arguments!");
			}
			var cmd = new MuxControlCommand();

			// Parse MUX id args...
			int muxId;
			if (!int.TryParse(parts[1], out muxId))
			{
				throw new ArgumentException($"Unexpected MuxId value '{parts[1]}'!");
			}
			cmd.MuxId = muxId;

			// Parse action arg...
			try
			{
				cmd.Action = (MuxControlCommand.Mux_action)Enum.Parse(typeof(MuxControlCommand.Mux_action), parts[2], true);
			}
			catch (Exception)
			{
				throw new ArgumentException($"Unexpected ACTION value '{parts[2]}'!");
			}

			cmd.TunnerController = tunerController;
			myCommand = cmd;
			
			this.Command = myCommand;
		}
	}
}
