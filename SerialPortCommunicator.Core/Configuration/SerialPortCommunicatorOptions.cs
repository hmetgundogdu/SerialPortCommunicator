using System.IO.Ports;

namespace SerialPortCommunicator.Core.Configuration;
public class SerialPortCommunicatorOptions
{
    public int RequestTimeoutMs { get; set; } = 5000;

    public Action<string>? MessageHandler { get; set; }

    required public Func<SerialPort, string> Reader { get; set; }
    required public Action<SerialPort, string> Writer { get; set; }
}
