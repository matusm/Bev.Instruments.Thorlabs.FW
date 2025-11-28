using System;
using Bev.Instruments.Thorlabs.FW;

namespace TestFW
{
    class Program
    {
        static void Main(string[] args)
        {
            //var fw = new FilterWheel(@"/dev/tty.usbserial-FTY594BQ");
            var fw = new FilterWheel(@"COM1");

            Console.WriteLine("Port:      " + fw.DevicePort);
            Console.WriteLine("ID:        " + fw.InstrumentID);
            Console.WriteLine("# filters: " + fw.FilterCount);
            Console.WriteLine("Position:  " + fw.GetPosition());


            for (int i = 0; i < 4; i++)
            {
                Move(1);
                Move(7);
                Move(3);
                Move(4);
                Move(5);
                Move(6);
                Move(1);
                Move(6);
                Move(5);
                Move(4);
                Move(3);
                Move(2);
                Move(1);
            }

            void Move(int pos)
            {
                Console.WriteLine($"Goto filter {pos} ...");
                fw.GoToPosition(pos);
                Console.Write("done. ");
                DisplayPosition();
                Console.WriteLine();
            }

            void DisplayPosition()
            {
                Console.WriteLine($"Position: {fw.GetPosition()}");
            }

        }

    }
}
