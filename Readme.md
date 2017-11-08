
# npServer
Library for simplify the creation of 'Named Pipe' servers.
It provides a ready and easy to use full implementation of named pipe message server

## Running the example.
The solution is composed by three projects.
+ **npServer**
The server library. Contains the `NamedPipeServerAsync` class with the core.
+ **npServerExample**
	Contains PingPongServer wich is a basic example of npServer library usage.
+ **npClientExample**
	Contains a basic client wich connect with PingPongServer.

## Using the code.
You only need to extend `NamedPipeServerAsync` class and overwrite `OnClientConnectionEstablished` method with your desired behaviour.
A full example is provided on `PingPongServer` class inside npServerExample project.

## License
The code is made freely available under the MIT open source license (see accompanying LICENSE file for details).
Is provide as-is with no guarantee about its reliability, correctness, or suitability for any purpose.
