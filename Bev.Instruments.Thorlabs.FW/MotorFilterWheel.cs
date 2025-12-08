using System;
using System.IO.Ports;
using System.Threading;

namespace Bev.Instruments.Thorlabs.FW
{
    public class MotorFilterWheel : IFilterWheel
    {
        private readonly SerialPort serialPort;
        private const int transmitDelay = 100;

        // The time it takes to move from #1 to #4 (#1 to #7 for 12 filter wheel)
        // the manual specifies 2500 ms, we measured 2900 ms.
        // in slow mode the respective time is 6800 ms.
        private const int typicalAccessTime = 2900;

        public MotorFilterWheel(string port)
        {
            serialPort = new SerialPort(port, 115200, Parity.None, 8, StopBits.One)
            {
                Handshake = Handshake.None,
                NewLine = "\r",
                ReadTimeout = typicalAccessTime,
                WriteTimeout = 500
            };
            serialPort.Open();
            UpdateInstrumentId();
        }

        public string DevicePort => serialPort.PortName;
        public string InstrumentManufacturer { get; private set; }
        public string InstrumentType { get; private set; }
        public string InstrumentFirmwareVersion { get; private set; }
        public string InstrumentID => $"{InstrumentManufacturer} {InstrumentType} {InstrumentFirmwareVersion}";
        public int FilterCount => GetFilterCount();
        public string Name => InstrumentID;

        public void GoToPosition(int position)
        {
            if (IsInvalidPosition(position)) throw new ArgumentOutOfRangeException($"pos={position}");
            if (position == GetPosition()) return; // already there
            _ = Query($"pos={position}");
        }

        public int GetPosition() => QueryNumber("pos?");

        public string Query(string command)
        {
            Write(command);
            _ = Read(); // this consumes the echo!
            string answer = Read();
            answer = RemoveNewLine(answer);
            CheckErrorStatus(answer, command);
            return answer;
        }

        public int QueryNumber(string command)
        {
            string answer = Query(command);
            return int.TryParse(answer, out int value) ? value : -1;
        }

        private int GetFilterCount() => QueryNumber("pcount?");

        private void UpdateInstrumentId()
        {
            string idLine = Query("*idn?");
            char[] delimiter = { ' ' };
            string[] tokens = idLine.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 6)
            {
                InstrumentManufacturer = tokens[0];
                InstrumentType = tokens[1];
                InstrumentFirmwareVersion = tokens[5];
            }
            else
            {
                Console.WriteLine($"wrong format for idn: {tokens.Length} tokens!");
            }
        }

        private bool IsInvalidPosition(int pos)
        {
            if (FilterCount < 0) return false; // cannot check if filter count is invalid
            if (pos < 1) return true;
            if (pos > FilterCount) return true;
            return false;
        }

        private void CheckErrorStatus(string answer, string command)
        {
            if (answer.Contains("CMD_NOT_DEFINED"))
            {
                throw new InvalidOperationException(command);
            }
            if (answer.Contains("CMD_ARG_INVALID"))
            {
                throw new ArgumentOutOfRangeException(command, answer);
            }
        }

        private void Write(string command)
        {
            serialPort.DiscardInBuffer();
            serialPort.DiscardOutBuffer();
            serialPort.WriteLine(command);
            Thread.Sleep(transmitDelay);
        }

        private string Read()
        {
            string answer = string.Empty;
            try
            {
                answer = serialPort.ReadLine();
            }
            catch (TimeoutException)
            {
                // return the empty string
            }
            return RemoveNewLine(answer);
        }

        private string RemoveNewLine(string line) => line.Replace("\r", string.Empty).Replace("\n", string.Empty);

    }
}
