using System.Threading.Tasks;
using UnityEngine;

public class InteractableEvent : MonoBehaviour, IPopup
{
    public bool eventOver;
    public Task StartPopup()
    {
        return Task.CompletedTask;
    }

    public async Task WaitForDone()
    {
        while(!eventOver) {
            await Task.Delay(250);
        }
    }

    public void SetEvent(bool eventVal) {
        eventOver = eventVal;
    }
}