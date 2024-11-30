using System.IO.Ports;

namespace SerialPortCommunicator.Core.Models;
internal class SerialPortCommunicatorMessage
{
    required public TaskCompletionSource<string> Task { get; set; }
    required public string Content { get; set; }
    required public Func<SerialPort, string> Reader { get; set; }
    required public Action<SerialPort, string> Writer { get; set; }
    public int TimeoutMs { get; set; }
}
