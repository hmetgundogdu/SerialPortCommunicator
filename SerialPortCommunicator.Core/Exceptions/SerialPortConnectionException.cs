namespace SerialPortCommunicator.Core.Exceptions;

public class SerialPortConnectionException : Exception
{
    public SerialPortConnectionException() { }
    public SerialPortConnectionException(string text) : base(text) { }
}
