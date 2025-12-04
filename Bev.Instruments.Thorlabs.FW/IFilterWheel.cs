namespace Bev.Instruments.Thorlabs.FW
{
    public interface IFilterWheel
    {
        string Name { get; }
        int FilterCount { get; }
        void GoToPosition(int position);
        int GetPosition();
    }
}
