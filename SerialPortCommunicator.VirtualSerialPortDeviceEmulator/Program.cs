using System.IO.Ports;

var serialPort = new SerialPort("COM10", 9600, Parity.None, 8, StopBits.One);

serialPort.Open();

var random = new Random();

serialPort.DataReceived += delegate (object _, SerialDataReceivedEventArgs e)
{
    var command = serialPort.ReadLine();
    var randomNumber = random.Next();

    var result = (string?)null;

    switch (command)
    {
        case "Command-1": result = $"Command-1 Result: {randomNumber}"; break;
        case "Command-2": result = $"Command-2 Result: {randomNumber}"; break;
        default: result = $"There is no command like that: {command}"; break;
    }

    if (result is not null)
    {
        serialPort.WriteLine(result);
    }

    Console.WriteLine($"{command} recived then responsed that {result}");

};

Console.WriteLine("Listening as a virtual device...");

Console.ReadLine();