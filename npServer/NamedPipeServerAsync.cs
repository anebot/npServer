using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Pipes;

namespace NamedPipeExample.npServer
{
	public class NamedPipeServerAsync
	{
		// Number of max request at same time!
		const short MAX_REQUESTS = 10;
		public string EntryPoint { get; private set; }

		IDictionary<String, RequestDigest> Requesters;
		private Object oRequestersLock = new object();

		public event EventHandler<RequestDigestEventArgs> ConnectionEstablished;
		
		public NamedPipeServerAsync(String NamedPipeName)
		{
			this.EntryPoint = NamedPipeName;
			this.Requesters = new Dictionary<String, RequestDigest>(MAX_REQUESTS);
		}

		private void OnCilentConnectionFinishes(object sender, EventArgs e)
		{
			var req = (sender as RequestDigest);
			lock (oRequestersLock)
			{
				this.Requesters.Remove(req.Id);
			}
		}

		private void OnClientConnected(object sender, EventArgs e)
		{
			if (this.Requesters.Count == MAX_REQUESTS) return; // ... ?

			// Create a request handler for the next connection...
			var requestHandler = CreateRequestHandler();
			lock (oRequestersLock)
			{
				this.Requesters.Add(requestHandler.Id, requestHandler);
			}
		}

		protected virtual void OnClientConnectionEstablished(object sender, RequestDigestEventArgs e)
		{
			this.ConnectionEstablished?.Invoke(sender, e);
		}

		public void Run()
		{
			var requestHandler = CreateRequestHandler();
		}

		public bool Stop()
		{
			lock (oRequestersLock)
			{
				foreach (var req in this.Requesters.Values)
				{
					req.Stop();
				}
				this.Requesters.Clear();
			}

			return true;
		}

		private RequestDigest CreateRequestHandler()
		{
			RequestDigest digest = new RequestDigest(this.EntryPoint, MAX_REQUESTS);
			digest.ClientConnected += OnClientConnected;
			digest.ConnectionStablished += OnClientConnectionEstablished;
			digest.ClientDisconnected += OnCilentConnectionFinishes;
			return digest;
		}

		public class RequestDigestEventArgs : EventArgs
		{
			public IStreamString StreamString { get; set; }
		}

		internal class RequestDigest
		{
			NamedPipeServerStream pipeServer;
			private bool isStopping = false;

			internal event EventHandler ClientConnected;
			internal event EventHandler<RequestDigestEventArgs> ConnectionStablished;
			internal event EventHandler ClientDisconnected;

			public String Id { get; private set; }
			public RequestDigest(String EntryPoint, short MaxAllowRequest)
			{
				this.pipeServer = new NamedPipeServerStream(EntryPoint, PipeDirection.InOut, MaxAllowRequest, PipeTransmissionMode.Byte,
								PipeOptions.Asynchronous);
				this.pipeServer.BeginWaitForConnection(this.WaitForConnectionCallBack, null);

				this.Id = Guid.NewGuid().ToString();
			}
			private void WaitForConnectionCallBack(IAsyncResult ar)
			{
				if (!this.isStopping)
				{
					this.pipeServer.EndWaitForConnection(ar);
					OnClientConnect();
					IStreamString ss = new StreamString(this.pipeServer);

					var conArgs = new RequestDigestEventArgs() { StreamString = ss };
					ConnectionStablished(this, conArgs);

					Stop();
					OnClientDisconnect();
				}
			}
			private void OnClientConnect()
			{
				ClientConnected(this, new EventArgs());
			}
			private void OnClientDisconnect()
			{
				ClientDisconnected(this, new EventArgs());
			}
			public void Stop()
			{
				this.isStopping = true;

				try
				{
					if (this.pipeServer.IsConnected)
					{
						this.pipeServer.Disconnect();
					}
				}
				catch (Exception ex)
				{
					//Logger.Error(ex);
					throw;
				}
				finally
				{
					this.pipeServer.Close();
					this.pipeServer.Dispose();
				}
			}
		}

		public interface IStreamString {
			string ReadString();
			int WriteString(string outString);
		}

		/// <summary>
		/// Exposes the methods definied on IStreamString which allows comunicate through the pipe
		/// </summary>
		public class StreamString : IStreamString
		{
			private Stream ioStream;
			const short BUFF_SIZE = 128; // messages longer that 64 chars will not be accepted!
			Encoding myEncode;
			
			public StreamString(Stream stream, Encoding encode) {
				this.ioStream = stream;
				this.myEncode = encode;
			}

			public StreamString(Stream stream) : this(stream, new ASCIIEncoding())
			{
			}

			public string ReadString()
			{
				byte[] buff = new byte[BUFF_SIZE];
				int read;

				read = ioStream.Read(buff, 0, buff.Length);

				return this.myEncode.GetString(buff,0,read);
			}
			public int WriteString(string outString)
			{
				byte[] data = this.myEncode.GetBytes(outString);
				if (data.Length > BUFF_SIZE)
				{
					throw new ArgumentOutOfRangeException("Message is too long!");
				}
				this.ioStream.Write(data, 0, data.Length);
				this.ioStream.Flush();
				return data.Length;
			}
		}
	}
}
