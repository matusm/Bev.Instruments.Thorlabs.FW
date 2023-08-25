using System;
using System.IO.Ports;
using System.Threading;

namespace Bev.Instruments.Thorlabs.FW
{
    public class FilterWheel
    {
        private readonly SerialPort serialPort;
        private int filterCount;
        private const int typicalAccessTime = 2500; // in ms, specifications, manual p 12

        public FilterWheel(string port)
        {
            DevicePort = port.Trim();
            serialPort = new SerialPort(DevicePort, 115200, Parity.None, 8, StopBits.One)
            {
                Handshake = Handshake.None,
                NewLine = "\r"
            };
            serialPort.Open();
            Initialize();
        }

        public string DevicePort { get; }
        public string InstrumentManufacturer { get; private set; }
        public string InstrumentType { get; private set; }
        public string InstrumentSerialNumber => "---"; // no documented way to obtain the serial number
        public string InstrumentFirmewareVersion { get; private set; }
        public int FilterCount => filterCount;

        public void SetPosition(int position)
        {
            // TODO check valid position number
            Query($"pos={position}");
        }

        public void SetPositionWait(int position)
        {
            SetPosition(position);
            Thread.Sleep(typicalAccessTime);
        }

        public int GetPosition() => QueryNumber("pos?");

        public string Query(string command)
        {
            Write(command);
            string answer = Read();
            answer = SkipNewLine(answer);
            CheckErrorStatus(answer, command);
            return SkipPrompt(answer);
        }

        public int QueryNumber(string command)
        {
            string answer = Query(command);
            return int.TryParse(answer, out int value) ? value : -1;
        }

        private void Initialize()
        {
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

        private string Read() => serialPort.ReadLine();

        private string SkipNewLine(string line) => line.TrimEnd('\r', '\n');

        private string SkipPrompt(string line) => line.TrimStart('>', ' ');

    }
}
