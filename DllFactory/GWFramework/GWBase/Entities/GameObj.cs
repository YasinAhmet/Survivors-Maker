using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;

public class GameObj : MonoBehaviour
{    
    public ActivationChange onActivationChange;

    public HealthChangeEvent onHealthChange;
    [SerializeField] protected SpriteRenderer ownedSpriteRenderer;
    [SerializeField] protected Rigidbody2D ownedRigidbody;
    [SerializeField] protected CircleCollider2D ownedCollider;
    [SerializeField] protected ThingDef possessedThing;

    [SerializeField] protected List<IObjBehaviour> installedBehaviours = new();

    protected float rareUpdateTimeCounter = 0;
    [SerializeField] protected float rareUpdateTickTime = 0.5f;
    [SerializeField] protected bool stopIfObstacle = true;
    [SerializeField] protected float antiPushRadius = 0.4f;
    private BehaviourHandler<GameObj> behaviourHandler = null;
    public float movementSpeed = 0;
    public string faction = "null";

    public virtual void Spawned() {
        
    }

    public virtual void Update()
    {
        rareUpdateTimeCounter += Time.deltaTime;
        if (rareUpdateTimeCounter > rareUpdateTickTime)
        {
            rareUpdateTimeCounter = 0;
            RareTick();
        }
        foreach (var behaviour in installedBehaviours)
        {
            behaviour?.Tick(null, Time.deltaTime);
        }
    }

    public virtual void RareTick()
    {
        foreach (var behaviour in installedBehaviours)
        {
            behaviour?.RareTick(null, rareUpdateTickTime);
        }
    }

    public virtual void Possess<T>(ThingDef entity, string faction)
    {
        Debug.Log("An object possessed thing: " + entity.Name);
        this.faction = faction;
        gameObject.layer = LayerMask.NameToLayer(faction);
        possessedThing = entity;
        PossessTexture(entity);
        PossessBehaviours(entity.behaviours);
    }

    public virtual void PossessTexture(ThingDef entity)
    {
        ownedSpriteRenderer.sprite = AssetManager.assetLibrary.texturesDictionary.First(x => x.Key.Equals(possessedThing.TexturePath)).Value;
        gameObject.transform.localScale = new Vector3(possessedThing.TextureSize, possessedThing.TextureSize, possessedThing.TextureSize);
    }

    public virtual void PossessBehaviours(string[] behaviourNames)
    {
        if(installedBehaviours != null)installedBehaviours.Clear();
        if(behaviourNames == null || behaviourNames == Array.Empty<string>()) return;

        foreach (var behaviourName in behaviourNames)
        {
            var keyPair = AssetManager.assetLibrary.behaviourDictionary.FirstOrDefault(x => x.Key == behaviourName);
            ObjBehaviourRef foundBehaviour = keyPair.Value;
            var targetDll = AssetManager.assetLibrary.GetAssembly(foundBehaviour.DllName);
            Type targetType = targetDll.GetType(foundBehaviour.Name, true);
            IObjBehaviour newBehaviour = (IObjBehaviour)System.Activator.CreateInstance(targetType);

            installedBehaviours.Add(newBehaviour);

            behaviourHandler = new BehaviourHandler<GameObj>()
            {
                ownedThing = this
            };

            newBehaviour.Start(foundBehaviour.linkedXmlSource, behaviourHandler.GetObjectsByRequests(foundBehaviour.parameterRequests));
        }
    }


    public virtual void MoveObject(Vector2 axis, float delta)
    {

        var movementResult = axis * movementSpeed;

        RaycastHit2D collision = Physics2D.CircleCast(
                        (Vector2)transform.position + movementResult,
                        antiPushRadius,
                        movementResult);

        if(stopIfObstacle && collision.collider != null) {
            ownedRigidbody.velocity = ownedRigidbody.velocity / 4;
        } else {
            ownedRigidbody.AddForce(movementResult * delta);
        }
    }

    public ThingDef GetPossessed()
    {
        return possessedThing;
    }

    public AudioClip GetAudioClipOf(string[] audioNamesList) {
        int audioIndex = UnityEngine.Random.Range(0, audioNamesList.Length);
        string audioName = audioNamesList[audioIndex];
        AudioClip audioClip = AssetManager.assetLibrary.audioDictionary.FirstOrDefault(x => x.Key.Equals(audioName)).Value;
        return audioClip;
    }

}

public interface IDamageable {
    public bool TryDamage(float amount, out bool endedUpKilling);
    public bool TryDestroy();
    public float GetHealth();
    public bool IsHealthDepleted();
    public float GetXP();
}


[System.Serializable]
public class HealthChangeEvent : UnityEvent<HealthInfo>
{
}

public struct HealthInfo {
    public float currentHealth;
    public float damageTaken;
    public bool changeMax;
}


[System.Serializable]
public class ActivationChange : UnityEvent<bool>
{
}