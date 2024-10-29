using System;
using System.Collections;

namespace GWBase
{
    public class PlayerManager : Manager
    {
        public PlayerController playerController;
        public delegate void NewSession(PlayerManager playerInstance);
        public static event NewSession newSessionStarted;
        public override IEnumerator Kickstart()
        {
            playerController = new();
            yield return StartCoroutine(playerController.Start());
            newSessionStarted.Invoke(this);
            yield return this;
        }
    }
}