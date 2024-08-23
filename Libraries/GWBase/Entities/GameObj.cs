using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace GWBase
{

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
        public string faction = "null";

        public virtual void Spawned()
        {

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
            Debug.Log("An object possessed thing: " + entity.Name + " Faction is : " + faction);
            this.faction = faction;
            gameObject.layer = LayerMask.NameToLayer(faction);
            possessedThing = YKUtility.DeepClone<ThingDef>(entity);
            PossessTexture(entity);
            PossessBehaviours(entity.behaviours, true);
        }

        public virtual void PossessTexture(ThingDef entity)
        {
            ownedSpriteRenderer.sprite = AssetManager.assetLibrary.texturesDictionary.First(x => x.Key.Equals(possessedThing.TexturePath)).Value;
            gameObject.transform.localScale = new Vector3(possessedThing.TextureSize, possessedThing.TextureSize, possessedThing.TextureSize);
        }

        public virtual void PossessUpgrades(UpgradeDef[] upgrades)
        {
            foreach (var upgrade in upgrades)
            {
                PossessBehaviours(upgrade.behaviours.ToArray(), false);
            }
        }

        public virtual void PossessBehaviours(BehaviourInfo[] behaviourInfo, bool clean)
        {
            if (installedBehaviours != null && clean) installedBehaviours.Clear();
            if (behaviourInfo == null || behaviourInfo == Array.Empty<BehaviourInfo>()) return;

            foreach (var behaviour in behaviourInfo)
            {
                var keyPair = AssetManager.assetLibrary.behaviourDictionary.FirstOrDefault(x => x.Key == behaviour.behaviourName);
                ObjBehaviourRef foundBehaviour = keyPair.Value;
                var targetDll = AssetManager.assetLibrary.GetAssembly(foundBehaviour.DllName);
                Type targetType = targetDll.GetType(foundBehaviour.Namespace + "." + foundBehaviour.Name, true);
                IObjBehaviour newBehaviour = (IObjBehaviour)System.Activator.CreateInstance(targetType);

                behaviourHandler = new BehaviourHandler<GameObj>()
                {
                    ownedThing = this
                };

                newBehaviour.Start(foundBehaviour.linkedXmlSource, behaviourHandler.GetObjectsByRequests(foundBehaviour.parameterRequests), behaviour.customParameters.ToArray());
                if (foundBehaviour.isOneTime == "No") installedBehaviours.Add(newBehaviour);
            }
        }


        public virtual void MoveObject(Vector2 axis, float delta)
        {

            var movementResult = axis * possessedThing.GetStatValueByName("MovementSpeed");

            RaycastHit2D collision = Physics2D.CircleCast(
                            (Vector2)transform.position + movementResult,
                            antiPushRadius,
                            movementResult);

            if (stopIfObstacle && collision.collider != null)
            {
                ownedRigidbody.velocity = ownedRigidbody.velocity / 4;
            }
            else
            {
                if (ownedRigidbody.isKinematic)
                {
                    ownedRigidbody.MovePosition(movementResult * delta);
                }
                else
                {
                    ownedRigidbody.AddForce(movementResult * delta);
                }
            }
        }

        public ThingDef GetPossessed()
        {
            return possessedThing;
        }

        public AudioClip GetAudioClipOf(string[] audioNamesList)
        {
            int audioIndex = UnityEngine.Random.Range(0, audioNamesList.Length);
            string audioName = audioNamesList[audioIndex];
            AudioClip audioClip = AssetManager.assetLibrary.audioDictionary.FirstOrDefault(x => x.Key.Equals(audioName)).Value;
            return audioClip;
        }

        public void SetRigidbodyMode(string type) {
            if (type == "Kinematic") {
                ownedRigidbody.bodyType = RigidbodyType2D.Kinematic;
            } else if (type == "Dynamic") {
                ownedRigidbody.bodyType = RigidbodyType2D.Dynamic;
            }
        }

    }

    public interface IDamageable
    {
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

    public struct HealthInfo
    {
        public float currentHealth;
        public float damageTaken;
        public bool changeMax;
    }


    [System.Serializable]
    public class ActivationChange : UnityEvent<bool>
    {
    }
}