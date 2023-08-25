using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bev.Instruments.Thorlabs.FW;

namespace TestFW
{
    class Program
    {
        static void Main(string[] args)
        {
            var fw = new FilterWheel("COM4");

            fw.SetPositionWait(1);
            Console.WriteLine(fw.GetPosition());
            Console.WriteLine("# filters: " + fw.FilterCount);
            Console.WriteLine("ID: " + fw.InstrumentID);
            fw.SetPositionWait(4);
            fw.SetPositionWait(7);
            fw.SetPositionWait(5);



        }
    }
}
