using System.Threading.Tasks;

public interface IPopup
{
    public Task WaitForDone();
    public Task StartPopup();
}