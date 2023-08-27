using System;
using System.IO.Ports;
using System.Threading;

namespace Bev.Instruments.Thorlabs.FW
{
    public class FilterWheel
    {
        private readonly SerialPort serialPort;
        private int filterCount;
        private const int typicalAccessTime = 2500; // in ms, specifications manual p 12
        private const int delay = 100;
        private const int maxLoops = 100000;

        public FilterWheel(string port)
        {
            DevicePort = port.Trim();
            serialPort = new SerialPort(DevicePort, 115200, Parity.None, 8, StopBits.One)
            {
                Handshake = Handshake.None,
                NewLine = "\r",
                ReadTimeout = typicalAccessTime,
                WriteTimeout = typicalAccessTime
            };
            serialPort.Open();
            Initialize();
        }

        public string DevicePort { get; }
        public string InstrumentManufacturer { get; private set; }
        public string InstrumentType { get; private set; }
        public string InstrumentFirmewareVersion { get; private set; }
        public string InstrumentID => $"{InstrumentManufacturer} {InstrumentType} {InstrumentFirmewareVersion}";
        public int FilterCount => filterCount;

        public void GoToPosition(int position)
        {
            // TODO check valid position number
            Query($"pos={position}");
        }

        public void GoToPositionWait(int position)
        {
            GoToPosition(position);
            Thread.Sleep(typicalAccessTime);
        }

        public int GetPosition() => QueryNumber("pos?");

        public string Query(string command)
        {
            Write(command);
            Thread.Sleep(delay);
            _ = Read();
            string answer = Read();
            Thread.Sleep(delay);
            answer = SkipNewLine(answer);
            _Log(command, answer);
            CheckErrorStatus(answer, command);
            return answer;
        }

        public int QueryNumber(string command)
        {
            string answer = Query(command);
            int n = int.TryParse(answer, out int value) ? value : -1;
            _Log(n);
            return n;
        }

        private void Initialize()
        {
            //serialPort.DiscardInBuffer();
            Write("");
            string answ = Read();
            if (!IsPrompt(answ))
                Write("");
            UpdateInstrumentId();
            filterCount = GetFilterCount();
        }

        private int GetFilterCount() => QueryNumber("pcount?");

        private void UpdateInstrumentId()
        {
            string idLine = Query("*idn?");
            char[] del = { ' ' };
            string[] tokens = idLine.Split(del, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 6)
            {
                InstrumentManufacturer = tokens[0];
                InstrumentType = tokens[1];
                InstrumentFirmewareVersion = tokens[5];
            }
        }

        private void CheckErrorStatus(string answer, string command)
        {
            _Log("CheckErrorStatus", answer);
            if (answer.Contains("CMD_NOT_DEFINED"))
            {
                throw new InvalidOperationException(command);
            }
            if (answer.Contains("CMD_ARG_INVALID"))
            {
                throw new ArgumentOutOfRangeException(command, answer);
            }
        }

        private void Write(string command) => serialPort.WriteLine(command);

        private string Read()
        {
            string answer = string.Empty;
            try
            {
                //int charCount = serialPort.BytesToRead;
                //char[] chars = new char[charCount];
                //serialPort.Read(chars, 0, charCount);
                //answer = new string(chars);
                answer = serialPort.ReadExisting();
            }
            catch (TimeoutException) // cannot happen with ReadExisting()
            {
                // return the empty string
            }
            return answer;
        }

        private bool IsPrompt(string line)
        {
            _Log("IsPrompt", line);
            if(line.Contains(">"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void WaitForPrompt()
        {
            for (int i = 0; i < maxLoops; i++)
            {
                string answer = Read();
                if (IsPrompt(answer)) 
                    return;
                Thread.Sleep(delay);
            }
        }


        private string SkipNewLine(string line) => line.TrimEnd('\r', '\n');


        private void _Log(string message) => Console.WriteLine($"*****DEBUG***** '{message}'");
        private void _Log(int n) => _Log($"n = {n}");
        private void _Log(string key, string value) => _Log($"{key}:{value}");

    }
}
