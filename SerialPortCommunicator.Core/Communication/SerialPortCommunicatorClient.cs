using System.IO.Ports;
using System.Collections.Concurrent;

using SerialPortCommunicator.Core.Models;
using SerialPortCommunicator.Core.Exceptions;
using SerialPortCommunicator.Core.Configuration;

namespace SerialPortCommunicator.Core.Communication;
public class SerialPortCommunicatorClient : IDisposable
{
    private readonly SerialPort _client;
    private readonly SerialPortCommunicatorOptions _option;

    private readonly SemaphoreSlim _semaphore = new(1, 1);

    private bool _isProcessing = false;

    private Func<SerialPort, string>? _responseReader = null;
    private TaskCompletionSource<string>? _responseCompletion = null;

    private readonly ConcurrentQueue<SerialPortCommunicatorMessage> _messages = new();

    public SerialPortCommunicatorClient(
        SerialPort client,
        SerialPortCommunicatorOptions option)
    {
        _option = option ?? throw new ArgumentNullException(nameof(option));
        _client = client ?? throw new ArgumentNullException(nameof(client));

        _client.Open();
        _client.DataReceived += _handleSerialPortMessage;
    }

    /// <summary>
    /// Creates a request via text and custom serial port reader function then returns its task
    /// </summary>
    /// <param name="text"></param>
    /// <param name="reader"></param>
    /// <returns></returns>
    public Task<string> RequestAsync(
        string text,
        Func<SerialPort, string>? reader = null)
    {
        return RequestAsync(text, _option.RequestTimeoutMs, reader);
    }

    public Task<string> RequestAsync(
        string text,
        int timeout)
    {
        return RequestAsync(text, _option.RequestTimeoutMs, _option.Reader);
    }

    public Task<string> RequestAsync(
        string text,
        int timeout,
        Func<SerialPort, string>? reader = null)
    {
        return RequestAsync(text, timeout, reader, _option.Writer);
    }

    /// <summary>
    /// Creates a request via text, timeout and custom serial port reader function then returns its task
    /// </summary>
    /// <param name="text"></param>
    /// <param name="timeout"></param>
    /// <param name="reader"></param>
    /// <returns></returns>
    public async Task<string> RequestAsync(
        string text,
        int timeout,
        Func<SerialPort, string>? reader = null,
        Action<SerialPort, string>? writer = null)
    {
        var messageTaskCan = new CancellationTokenSource();
        var tcs = new TaskCompletionSource<string>(messageTaskCan);
        var message = new SerialPortCommunicatorMessage
        {
            Task = tcs,
            Content = text,
            Reader = reader ?? _option.Reader,
            Writer = writer ?? _option.Writer,
            TimeoutMs = timeout,
        };

        var timeoutTask = Task.Delay(timeout).ContinueWith(_ => "Timeout");
        var processResults = new Task<string>[] { tcs.Task, timeoutTask };

        _messages.Enqueue(message);

        ProcessMessagesAsync();

        var processResult = await Task.WhenAny(processResults);

        if (processResult == timeoutTask)
        {
            throw new SerialPortTimeoutException($"Got a timeout for request Body: {text}");
        }

        return processResult.Result;
    }

    /// <summary>
    /// Calls after the request and process message as one by one
    /// </summary>
    private async void ProcessMessagesAsync()
    {
        if (_isProcessing)
            return;

        _isProcessing = true;

        await _semaphore.WaitAsync();

        try
        {
            while (_messages.TryDequeue(out var message))
            {
                var tcs = message.Task;
                var writer = message.Writer ?? _option.Writer;

                if (_client.IsOpen == false && tcs.Task.IsCompleted is false)
                {
                    tcs.SetException(new SerialPortConnectionException());
                }

                _responseReader = message.Reader ?? _option.Reader;
                _responseCompletion = new TaskCompletionSource<string>();

                var write = writer ?? _option.Writer;
                var content = message.Content;

                write(_client, content);

                var responseTask = _responseCompletion.Task;

                var timeout = message.TimeoutMs;
                var timeoutTask = Task.Delay(timeout).ContinueWith(_ => "Timeout");

                var processTaskResult = await Task.WhenAny([responseTask, timeoutTask]);

                if (processTaskResult == timeoutTask)
                {
                    _responseCompletion.SetCanceled();

                    continue;
                }

                var canMessageResponsable = message.Task.Task.IsCompleted is false;
                var response = await responseTask;

                if (canMessageResponsable)
                {
                    tcs.SetResult(response);
                }
            }
        }
        finally
        {
            _isProcessing = false;
            _responseReader = null;
            _responseCompletion = null;

            _semaphore.Release();
        }

    }

    /// <summary>
    /// Handles serial port messages try to complete current message response task
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void _handleSerialPortMessage(object sender, SerialDataReceivedEventArgs e)
    {
        var serialPort = (SerialPort)sender;
        var readSerialPort = _responseReader ?? _option.Reader;

        try
        {
            if (readSerialPort is null)
            {
                return;
            }

            var recivedMessage = readSerialPort(serialPort);
            // TODO: Write message handler and response compilation in same time
            if (_option.MessageHandler is not null)
            {
                _option.MessageHandler(recivedMessage);
            }

            if (_responseCompletion is not null && _responseCompletion.Task.IsCompleted is false)
            {
                _responseCompletion.SetResult(recivedMessage);
            }
        }
        catch (Exception) { }
    }

    /// <summary>
    /// Dispose all disposables using in side the comminicator
    /// </summary>
    public void Dispose()
    {
        if (_client.IsOpen)
        {
            _client.DataReceived -= _handleSerialPortMessage;
            _client.Close();
        }
        _client.Dispose();
    }
}
