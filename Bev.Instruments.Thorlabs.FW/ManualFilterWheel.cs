using System;

namespace Bev.Instruments.Thorlabs.FW
{
    public class ManualFilterWheel : IFilterWheel
    {
        public string Name { get; }

        public int FilterCount { get; }

        public ManualFilterWheel(string name, int filterCount)
        {
            Name = name;
            FilterCount = filterCount;
        }

        public ManualFilterWheel() : this("Thorlabs CFW6/M", 6) {}

        public void GoToPosition(int position)
        {
            if (IsInvalidPosition(position)) throw new ArgumentOutOfRangeException($"position={position}");
            if (position == _position) return;
            WriteMessageAndWait($"Set the filter wheel '{Name}' to position {position} and press any key to continue ...");
            _position = position;
        }

        public int GetPosition() => _position;

        private int _position = -1; // default starting position, unknown

        private void WriteMessageAndWait(string message)
        {
            while (Console.KeyAvailable) // Check if any key is pressed
            {
                Console.ReadKey(true); // Read and ignore the key
            }
            Console.WriteLine(message);
            Console.ReadKey(true); // true = do not display the key pressed
        }

        private bool IsInvalidPosition(int pos)
        {
            if (FilterCount < 0) return false; // cannot check if filter count is invalid
            if (pos < 1) return true;
            if (pos > FilterCount) return true;
            return false;
        }
    }
}
