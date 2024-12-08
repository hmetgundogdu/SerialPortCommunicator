## Why It Was Created?
Why It Was Created?
Before this library, sending a command and handling the response with separate handlers often caused confusion. Managing multiple commands at the same time made things even more chaotic and hard to control.

This library was created to solve these problems. It makes serial communication simpler and easier for developers to manage.

## Example

```csharp
 var SerialPortCommunicatorClient _communicator = new(
            new SerialPort("COM1", 9600, Parity.None, 8, StopBits.One),
            new SerialPortCommunicatorOptions
            {
                Reader = (_) => _.ReadLine(),
                Writer = (_, text) => _.WriteLine(text),
                RequestTimeoutMs = 5000,
            });

var result = await _communicator.RequestAsync("test-command");

Console.WriteLine(result);

``` 
You can also check out [example project](https://github.com/hmetgundogdu/SerialPortCommunicator/blob/main/SerialPortCommunicator.Examples/Program.cs").

## Library Workflow Diagram

![Image](https://github.com/hmetgundogdu/SerialPortCommunicator/blob/main/Docs/SerialCommunicatorDiagram.svg)

## License
This project is licensed under the MIT License - see the [LICENSE](./LICENSE) file for details.
