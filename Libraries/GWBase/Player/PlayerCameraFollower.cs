using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
namespace GWBase {

public class PlayerCameraFollower : MonoBehaviour, IBootable
{
    Transform targetTransform;
    public float ViewWidth = -10;
    public bool instant = false;

    public IEnumerator Boot()
    {
        while (PlayerController.playerController.ownedCreature == null) yield return new WaitForSeconds(0.25f);
       
        targetTransform = ((Component)PlayerController.playerController.ownedCreature).transform;
        yield return this;
    }
    void FixedUpdate()
    {
        try {
            if (instant)
            {
                transform.position = new Vector3(targetTransform.position.x, targetTransform.position.y, ViewWidth);
                return;
            }
            transform.position = Vector3.Lerp(transform.position, new Vector3(targetTransform.position.x, targetTransform.position.y, ViewWidth), Time.fixedDeltaTime);
        } catch {
            if(PlayerController.playerController?.ownedCreature != null)targetTransform = ((Component)PlayerController.playerController.ownedCreature).transform;
        }
    }

    public void BootSync()
    {
        throw new System.NotImplementedException();
    }
}

public interface IBootable {
    public IEnumerator Boot();
    public void BootSync();
}
}