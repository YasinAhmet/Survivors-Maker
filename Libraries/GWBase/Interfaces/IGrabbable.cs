namespace GWBase
{
    public interface IGrabbable
    {
        public void GrabRequest(GameObj by);
        public void DropRequest(bool force);
    }
}