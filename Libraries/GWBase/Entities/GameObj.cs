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
        public FacingDirectionChanged onFacingDirectionChange;
        public ActivationChange onActivationChange;
        public HealthChangeEvent onHealthChange;
        [SerializeField] protected SpriteRenderer ownedSpriteRenderer;
        [SerializeField] protected Rigidbody2D ownedRigidbody;
        [SerializeField] protected CircleCollider2D ownedCollider;
        [SerializeField] protected ThingDef possessedThing;

        [SerializeField] public List<IObjBehaviour> installedBehaviours = new();

        [SerializeField] protected bool stopIfObstacle = true;
        [SerializeField] protected float antiPushRadius = 0.4f;
        [SerializeField] protected float velocityDropSpeed = 0.5f;
        [SerializeField] public Vector2 lastMovementVector;
        private BehaviourHandler<GameObj> behaviourHandler = null;
        public string faction = "null";
        public bool doesMirrorPosOnFacing = false;
        public bool lookingAtRight = true;
        public bool dontPassMaxSpeed = true;
        public float cachedMovementSpeed = 0;
        public bool isActive = false;


        public virtual void Spawned()
        {

        }

        public virtual void FixedUpdate()
        {
        }


        public virtual void RareTick(float timePassed)
        {
        }

        public virtual void Possess<T>(ThingDef entity, string faction)
        {
            Debug.Log("An object possessed thing: " + entity.Name + " Faction is : " + faction);
            this.faction = faction;
            gameObject.layer = LayerMask.NameToLayer(faction);
            possessedThing = YKUtility.DeepClone<ThingDef>(entity);
            PossessTexture(entity);
            PossessBehaviours(entity.behaviours, true);
            cachedMovementSpeed = possessedThing.GetStatValueByName("MaxSpeed");
            isActive = true;
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
            /*Vector2 movementResult = Vector2.zero;
            bool doesPass = DoesPassMaxSpeed(out float maxSpeed, out float currentSpeed);
            
            if (doesPass && dontPassMaxSpeed) { return; } 
            else { movementResult = axis * cachedMovementSpeed; }
*/
            Vector2 movementResult = axis * cachedMovementSpeed;
            if (movementResult == Vector2.zero) return;
            if (ownedRigidbody.isKinematic) { ownedRigidbody.MovePosition(movementResult * delta); } else { ownedRigidbody.AddForce(movementResult * delta); }

            /*if (stopIfObstacle)
            {
                RaycastHit2D collision = Physics2D.CircleCast(
                                (Vector2)transform.position + movementResult,
                                antiPushRadius,
                                movementResult);


                if (stopIfObstacle && collision.collider != null)
                {
                    var newVelocity = (ownedRigidbody.velocity / 4) - new Vector2(velocityDropSpeed, velocityDropSpeed);
                    ownedRigidbody.velocity = new Vector2(Math.Max(newVelocity.x, 0), Math.Max(newVelocity.y, 0));
                }
                else {
                     if (ownedRigidbody.isKinematic) { ownedRigidbody.MovePosition(movementResult * delta); } else { ownedRigidbody.AddForce(movementResult * delta); } 
                }
            }
            else { if (ownedRigidbody.isKinematic) { ownedRigidbody.MovePosition(movementResult * delta); } else { ownedRigidbody.AddForce(movementResult * delta); } }
        */
        }

        public bool DoesPassMaxSpeed(out float maxSpeed, out float currentSpeed) {
            maxSpeed = possessedThing.GetStatValueByName("MaxSpeed");
            currentSpeed = ownedRigidbody.velocity.magnitude;
            return currentSpeed > maxSpeed;
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

        public void SetRigidbodyMode(string type)
        {
            if (type == "Kinematic")
            {
                ownedRigidbody.bodyType = RigidbodyType2D.Kinematic;
            }
            else if (type == "Dynamic")
            {
                ownedRigidbody.bodyType = RigidbodyType2D.Dynamic;
            }
        }

        public void FlipFace(bool flipRight)
        {
            bool wasLookingAtRight = lookingAtRight;
            if (flipRight)
            {
                lookingAtRight = false;
                ownedSpriteRenderer.flipX = true;
            }
            else
            {
                lookingAtRight = true;
                ownedSpriteRenderer.flipX = false;
            }

            onFacingDirectionChange?.Invoke(flipRight);
        }

        public void FlipLocalPos(bool flipRight)
        {
            bool wasLookingAtRight = lookingAtRight;
            if (flipRight)
            {
                lookingAtRight = false;
            }
            else
            {
                lookingAtRight = true;
            }

            if (lookingAtRight != wasLookingAtRight && doesMirrorPosOnFacing)
            {
                transform.localPosition = new Vector3(-transform.localPosition.x, transform.localPosition.y, transform.localPosition.z);
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


    [System.Serializable] public class ActivationChange : UnityEvent<bool> { }


    [System.Serializable] public class FacingDirectionChanged : UnityEvent<bool> { }
}