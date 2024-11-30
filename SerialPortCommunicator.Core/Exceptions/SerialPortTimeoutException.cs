namespace SerialPortCommunicator.Core.Exceptions;

public class SerialPortTimeoutException(string message) : Exception(message)
{
}