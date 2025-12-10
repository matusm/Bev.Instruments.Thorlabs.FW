namespace Bev.Instruments.Thorlabs.FW
{
    public class NullFilterWheel : IFilterWheel
    {
        public string Name => "Null Filter Wheel";

        public int FilterCount => 0;

        public int GetPosition()
        {
            return 0;
        }

        public void GoToPosition(int position)
        {
            // Do nothing
        }
    }
}
