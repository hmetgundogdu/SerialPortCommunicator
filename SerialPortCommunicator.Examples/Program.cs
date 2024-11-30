using System.IO.Ports;

using SerialPortCommunicator.Core.Configuration;
using SerialPortCommunicator.Core.Communication;

internal class Program
{
    private static readonly SerialPortCommunicatorClient _communicator = new(
            new SerialPort("COM11", 9600, Parity.None, 8, StopBits.One),
            new SerialPortCommunicatorOptions
            {
                Reader = (_) => _.ReadLine(),
                Writer = (_, text) => _.WriteLine(text),
            });

    readonly static List<string> commandList1 = new() {
        "Command-1",
        "Command-2",
    };

    readonly static List<string> commandList2 = new() {
        "Command-3",
        "Command-4",
    };

    public static void Main(string[] args)
    {
        var exit = "";

        do
        {
            var example1Task = SendCommandList1();
            var example2Task = SendCommandList2();

            Task.WaitAll([example1Task, example2Task]);

            exit = Console.ReadLine();

        } while (exit == "");

        _communicator.Dispose();
    }
    private static async Task SendCommandList1()
    {
        var resultList = new List<string>();

        foreach (var command in commandList1)
        {
            var result = await _communicator.RequestAsync(command, 6000, (_) => _.ReadLine());

            resultList.Add(result);
        }

        Console.WriteLine($"Command list 1 results;");

        foreach (var result in resultList)
        {
            Console.WriteLine(result);
        }
    }
    private async static Task SendCommandList2()
    {
        var resultList = new List<string>();

        foreach (var command in commandList2)
        {
            var result = await _communicator.RequestAsync(command, 6000, (_) => _.ReadLine());

            resultList.Add(result);
        }

        Console.WriteLine($"Command list 2 results;");

        foreach (var result in resultList)
        {
            Console.WriteLine(result);
        }
    }
}


