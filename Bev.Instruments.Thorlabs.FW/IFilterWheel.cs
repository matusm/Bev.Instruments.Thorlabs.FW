namespace Bev.Instruments.Thorlabs.FW
{
    interface IFilterWheel
    {
        int FilterCount { get; }
        void GoToPosition(int position);
        int GetPosition();
    }
}
