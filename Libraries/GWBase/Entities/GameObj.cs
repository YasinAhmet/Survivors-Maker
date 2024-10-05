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
        
        public delegate void ActionRequest(string actionName);
        public event ActionRequest actionRequested;
        
        public delegate void XpGained(float xp);
        public delegate void HealthChangeEvent(HealthInfo healthInfo);

        public delegate void ActivationChange(bool change);
        
        public delegate void FacingDirectionChanged(bool direction);
        public event XpGained onXpGain = delegate(float f) {  };

        public delegate void ActivityChange(GameObj obj, bool active);
        public event ActivityChange activityChange; 
        
        public event FacingDirectionChanged onFacingDirectionChange = delegate(bool direction) {  };
        public event HealthChangeEvent onHealthChange = delegate(HealthInfo info) {  };
        public event HealthChangeEvent pre_onHealthChange = delegate(HealthInfo info) {  };
        [SerializeField] protected SpriteRenderer ownedSpriteRenderer;
        [SerializeField] public Rigidbody2D ownedRigidbody;
        [SerializeField] protected CircleCollider2D ownedCollider;
        [SerializeField] protected ThingDef possessedThing;

        [SerializeField] public List<IObjBehaviour> installedBehaviours = new();

        [SerializeField] protected bool stopIfObstacle = true;
        [SerializeField] protected float antiPushRadius = 0.4f;
        [SerializeField] protected float velocityDropSpeed = 0.5f;
        [SerializeField] public Vector2 lastMovementVector;
        private BehaviourHandler<GameObj> behaviourHandler = null;
        public string faction = "null";
        [SerializeField] protected float xp = 40;
        public bool doesMirrorPosOnFacing = false;
        public bool lookingAtRight = true;
        public bool dontPassMaxSpeed = true;
        public float cachedMovementSpeed = 0;
        public bool isActive = false;
        public GameObject selfObject;
        public bool isKinematicObj = false;
        public Transform ownedTransform;
             
        public delegate void Possessed();

        public event Possessed OnPosesssed;

        public void CallHealthEvent(HealthInfo healthInfo)
        {
            onHealthChange?.Invoke(healthInfo);
        }
        
        public void CallPreHealthEvent(HealthInfo healthInfo)
        {
            pre_onHealthChange?.Invoke(healthInfo);
        }
        public void CallFacingDirectionChanged(bool direction)
        {
            onFacingDirectionChange.Invoke(direction);
        }
        public void CallActivationChange(bool activation)
        {
            activityChange.Invoke(this, activation);
            isActive = activation;
            gameObject.SetActive(activation);
        }

        public void RequestAction(string actionName)
        {
            actionRequested?.Invoke(actionName);
        }


        public void GainXP(float amount)
        {
            Debug.Log("XP Gained!!");
            xp += amount;
            onXpGain.Invoke(amount);
        }

        public virtual void Spawned()
        {
            selfObject = gameObject;
            ownedTransform = transform;
        }

        public virtual void FixedUpdate()
        {
        }

        public virtual void Collided(Collider2D collider)
        {
            
        }


        public virtual void RareTick(float timePassed)
        {
        }
        
        private void UpdateColliderSize() {
            Vector3 spriteHalfSize = ownedSpriteRenderer.sprite.bounds.extents;
            ownedCollider.radius = spriteHalfSize.x > spriteHalfSize.y ? spriteHalfSize.x : spriteHalfSize.y;
        }

        public virtual void Possess<T>(ThingDef entity, string faction)
        {
            ownedTransform = transform;
            onXpGain = null;
            //Debug.Log("An object possessed thing: " + entity.Name + " Faction is : " + faction);
            this.faction = faction;
            possessedThing = new ThingDef(entity);
            cachedMovementSpeed = possessedThing.GetStatValueByName("MaxSpeed");
            PossessTexture(entity);
            PossessBehaviours(entity.behaviours, true);
            isActive = true;

            if (ownedRigidbody != null)
            {
                isKinematicObj = ownedRigidbody.isKinematic;
                ownedRigidbody.mass = possessedThing.mass;
            }

            OnPosesssed?.Invoke();
        }

        public virtual void PossessTexture(ThingDef entity)
        {
            ownedSpriteRenderer.sprite = AssetManager.assetLibrary.texturesDictionary.First(x => x.Key.Equals(possessedThing.TexturePath)).Value;
            ownedSpriteRenderer.transform.localScale = new Vector3(possessedThing.TextureSize, possessedThing.TextureSize, possessedThing.TextureSize);
            UpdateColliderSize();
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
                ObjBehaviourRef foundBehaviour = AssetManager.assetLibrary.GetBehaviour(behaviour.behaviourName);
                var targetDll = AssetManager.assetLibrary.GetAssembly(foundBehaviour.DllName);
                Type targetType = targetDll.GetType(foundBehaviour.Namespace + "." + foundBehaviour.Name, true);
                IObjBehaviour newBehaviour = (IObjBehaviour)System.Activator.CreateInstance(targetType);

                behaviourHandler = new BehaviourHandler<GameObj>()
                {
                    ownedThing = this
                };
                

                newBehaviour.Start(foundBehaviour.linkedXmlSource, behaviourHandler.GetObjectsByRequests(foundBehaviour.parameterRequests), behaviour.customParameters?.ToArray());
                if (foundBehaviour.isOneTime == "No") installedBehaviours.Add(newBehaviour);
            }
        }

        public virtual void MoveObject(Vector2 axis, float delta, bool passMax)
        {
            float speed = (cachedMovementSpeed * delta);
            if (!ownedRigidbody || isKinematicObj)
            {
                ownedTransform.position += new Vector3(axis.x*speed, axis.y*speed, 0);
                return;
            }
            if (DoesPassMaxSpeed(out float max, out float current) && !passMax)
            {
                return;
            }
            ownedRigidbody.AddForce(new Vector3(axis.x*speed, axis.y*speed, 0)); 
        }

        public bool DoesPassMaxSpeed(out float maxSpeed, out float currentSpeed) {
            maxSpeed = cachedMovementSpeed;
            currentSpeed = ownedRigidbody.velocity.magnitude;
            return currentSpeed > maxSpeed;
        }
 
        public ThingDef GetPossessed()
        {
            return possessedThing;
        }

        
        public virtual bool TryDamage(float amount, out HealthInfo healthInfo)
        {
            healthInfo = new HealthInfo();
            healthInfo.gotKilled = false;
            return false;
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
                ((Component)this).transform.localPosition = new Vector3(-((Component)this).transform.localPosition.x, ((Component)this).transform.localPosition.y, ((Component)this).transform.localPosition.z);
            }
        }
    }

    public interface IDamageable
    {
        public bool TryDamage(float amount, out HealthInfo healthInfo);
        public bool TryDestroy();
        public float GetHealth();
        public bool IsHealthDepleted();
        public float GetXP();
        public string GetTeam();
    }


    

    public struct HealthInfo
    {
        public float currentHealth;
        public float damageTaken;
        public float maxHealth;
        public bool gotKilled;
        public GameObj infoOf;
    }
}