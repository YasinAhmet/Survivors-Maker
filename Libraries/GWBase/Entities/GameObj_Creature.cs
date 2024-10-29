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

    public class GameObj_Creature : GameObj, IDamageable, IAnimatable
    {

        public List<GameObj_Shooter> possessedWeapons;
        public event GameObj_Shooter.OnProjectileHit onHit;
        public delegate void ActionHappened(string actionType, object typeObj);
        public event ActionHappened onActionHappen = delegate(string type, object obj) {  };
        [SerializeField] private float maxhealth = 100;
        public int killCount = 0;
        public CreatureState currentState;
        public ItemPickupManager pickupManager;
        public GroupMemberReference groupAttached = new GroupMemberReference();
        public GroupMemberReference GroupAttached {
            set {
                groupAttached.group = value.group;
                groupAttached.position = value.position;
                ((Component)this).transform.localPosition = value.position;
                if(groupAttached.group.groupLeader)((Component)this).transform.position = ((Component)groupAttached.group.groupLeader).transform.position + groupAttached.position;
            }
            get {
                return groupAttached;
            }
        }

        public GroupPositionData possessedPosition;

        bool isPlayingAnimation = false;
        string currentAnimationPlaying = "null";
        float animationPlayingSpeed = 1;
        bool animationLooping = false;
        public AnimationSheet movementAnimationSheet;
        private IEnumerator movementCoroutine;
        public Vector2 directionLookingAt;

        public override void Possess<GameObj_Creature>(ThingDef entity, string faction)
        {
            lastHealthInfo = new HealthInfo();
            lastDamagedBy = null;
            directionLookingAt = Vector2.zero;
            currentState = CreatureState.Idle;
            cachedMovementSpeed = 0;
            lastMovementVector = Vector2.zero;
            animationPlayingSpeed = 1;
            animationLooping = false;
            isPlayingAnimation = false;
            killCount = 0;
            
            if (possessedWeapons != null)
            {
                foreach (var weapon in possessedWeapons)
                {
                    GameObject.Destroy(weapon.gameObject);
                }
            }
            
            possessedWeapons = new List<GameObj_Shooter>();
            
            base.Possess<GameObj_Creature>(entity, faction);            
            gameObject.layer = LayerMask.NameToLayer(faction);
            PossessEquipments(entity, faction);
            killCount = 0;
            

            CallHealthEvent(new HealthInfo()
            {
                currentHealth = possessedThing.GetStatValueByName("Health"),
                damageTaken = 0,
                maxHealth = possessedThing.GetStatValueByName("MaxHealth"),
                infoOf = this
            });

            CallActivationChange(true);
            InitializeAnims();
        }

        public void InitializeAnims()
        {
            try
            {
                var movementAnimationInfo = possessedThing.animations.FirstOrDefault(x => x.typeName.Equals("Movement"));
                movementAnimationSheet = AssetManager.assetLibrary.animationSheetsDictionary.FirstOrDefault(x => x.Key == movementAnimationInfo.sheetName).Value;
            }
            catch (Exception ex)
            {
            }
        }


        public override void Spawned()
        {
            base.Spawned();
        }

        public void UpdateCharacterMovement(Vector2 axis)
        {
            if (axis != Vector2.zero) directionLookingAt = axis;
            lastMovementVector = axis;
        }

        public override void MoveObject(Vector2 axis, float delta, bool passMax)
        {
            lastMovementVector = axis;
            base.MoveObject(axis, delta, false);

            if (groupAttached.group?.groupLeader != null)
            {
                axis = groupAttached.group.groupLeader.lastMovementVector;
            }

            if (Math.Abs(axis.x) + Math.Abs(axis.y) > 0)
            {
                currentState = CreatureState.Moving;
                FlipFace(axis.x < 0);
            }
            else
            {
                currentState = CreatureState.Idle;
            }
        }

        public override void FixedUpdate()
        {
            soundEffectCooldown -= Time.fixedDeltaTime;
            base.FixedUpdate();
            if (currentState == CreatureState.Moving && movementAnimationSheet != null && movementAnimationSheet.info.frameCount > 0 && !isPlayingAnimation)
            {
                if(movementCoroutine != null) StopCurrentAnimation();
                movementCoroutine = PlayAnimation(movementAnimationSheet);
                StartCoroutine(movementCoroutine);
            }
            else if (isPlayingAnimation && currentAnimationPlaying.Equals("Movement") && currentState != CreatureState.Moving)
            {
                StopCurrentAnimation();
            }
        }


        public virtual void PossessEquipments(ThingDef entity, string faction)
        {
            if (entity == null || entity.equipmentNames == null || entity.equipmentNames.Count() == 0) return;
            foreach (var equipment in entity.equipmentNames)
            {
                if (equipment.name == null || equipment.name == string.Empty) continue;
                var thingDef = AssetManager.assetLibrary.thingDefsDictionary.FirstOrDefault(x => x.Key == equipment.name).Value;
                var prefab = PrefabManager.prefabManager.GetPrefabOf("equipment");
                var spawnedObj = Instantiate(prefab).GetComponent<GameObj>();
                ((Component)spawnedObj).transform.parent = ((Component)this).transform;
                ((Component)spawnedObj).transform.localPosition = new Vector3(equipment.offset.x, equipment.offset.y, equipment.offset.z);
                GameObj_Shooter shooter = (GameObj_Shooter)spawnedObj;
                shooter.Possess<GameObj_Shooter>(YKUtility.FromXElement<ThingDef>(thingDef), faction, this);
                shooter.onProjectileHit += OnHitToEnemy;
                shooter.stats = possessedThing.stats;
                possessedWeapons.Add(shooter);

                if(equipment.offset.flipOffset == "Yes") {
                    spawnedObj.doesMirrorPosOnFacing = true;
                    onFacingDirectionChange += (spawnedObj.FlipLocalPos);
                }
            }
        }
        

        public void OnHitToEnemy(HitResult result)
        {
            onActionHappen.Invoke("hitGiven", result);
            onHit?.Invoke(result);
        }

        public void Heal(float amount, bool bypassMax)
        {
            possessedThing.AddToStat("Health", amount);

            if (possessedThing.GetStatValueByName("Health") > maxhealth && !bypassMax)
            {
                possessedThing.ReplaceStat("Health", maxhealth);
            }


            CallHealthEvent(new HealthInfo()
            {
                currentHealth = possessedThing.GetStatValueByName("Health"),
                maxHealth = possessedThing.GetStatValueByName("MaxHealth"),
                damageTaken = -amount,
                infoOf = this
            });
        }

        public float soundEffectCooldown = 1;
        public GameObj_Creature lastDamagedBy;
        public override bool TryDamage(float amount, out HealthInfo healthInfo, GameObj_Creature causer)
        {
            lastDamagedBy = causer;
            onActionHappen.Invoke("hitTaken", amount);
            possessedThing.RemoveFromStat("Health", amount);
            
            if (possessedThing.soundConfig.onDamageTakenSounds != null)
            {
                if (soundEffectCooldown < 0)
                {
                    var audioClip = GetAudioClipOf(possessedThing.soundConfig.onDamageTakenSounds);
                    AudioSource.PlayClipAtPoint(audioClip, Vector3.zero);
                    soundEffectCooldown = 0.5f;
                }
            }
            lastHealthInfo = new HealthInfo()
            {
                currentHealth = possessedThing.GetStatValueByName("Health"),
                maxHealth = possessedThing.GetStatValueByName("MaxHealth"),
                damageTaken = amount,
                gotKilled =  IsHealthDepleted(),
                infoOf = this
            };
            
            healthInfo = lastHealthInfo;
            CallPreHealthEvent(lastHealthInfo);
            lastHealthInfo.gotKilled = IsHealthDepleted();
            healthInfo = lastHealthInfo;
            
            CallHealthEvent(lastHealthInfo);
            healthInfo = lastHealthInfo;
            return true;
        }


        public bool TryDestroy()
        {
            if (IsHealthDepleted())
            {
                if (possessedThing.soundConfig.onDeathSounds != null)
                {
                    var audioClip = GetAudioClipOf(possessedThing.soundConfig.onDeathSounds);
                    AudioSource.PlayClipAtPoint(audioClip, Vector3.zero);
                }

                selfObject.SetActive(false);
                isActive = false;
                ownedSpriteRenderer.color = Color.white;
                CallActivationChange(false);
                return true;
            }
            else
            {
                return false;
            }
        }

        public float GetHealth() { return possessedThing.GetStatValueByName("Health"); }

        public bool IsHealthDepleted() { return possessedThing.GetStatValueByName("Health") <= 0; }

        public float GetXP() { return xp; }
        public string GetTeam()
        {
            return faction;
        }

        public IEnumerator PlayAnimation(AnimationSheet animationDef)
        {
            {
                isPlayingAnimation = true;
                currentAnimationPlaying = animationDef.info.typeName;
                bool haventExecutedOnce = true;
                animationLooping = animationDef.info.doesLoop.Equals("Yes");
                float timeToWaitForNextFrame = 1 / (animationDef.info.framePerSecond * animationPlayingSpeed);

                while ((haventExecutedOnce || animationLooping) && isPlayingAnimation)
                {
                    for (int i = 0; i < animationDef.info.frameCount; i++)
                    {
                        if (!isPlayingAnimation) break;
                        var newSprite = animationDef.calculatedFrames[i];
                        ownedSpriteRenderer.sprite = newSprite;
                        haventExecutedOnce = false;
                        yield return new WaitForSeconds(timeToWaitForNextFrame);
                    }
                }


                isPlayingAnimation = false;
                yield return this;
            }
        }

        public void StopCurrentAnimation()
        {
            isPlayingAnimation = false;
            StopCoroutine(movementCoroutine);
            movementCoroutine = null;
        }

        public bool IsPlayingAnimation() { return isPlayingAnimation; }

        public void SetAnimationSpeed(float speed) { animationPlayingSpeed = speed; }

        public void SetAnimationLooping(bool isLooping) { animationLooping = isLooping; }

        public AnimationSheet GetAnimation(string animationName)
        {
            var foundThing = possessedThing.animations.FirstOrDefault(x => x.typeName == animationName);
            if (foundThing.typeName == animationName)
            {
                return AssetManager.assetLibrary.animationSheetsDictionary.FirstOrDefault(x => x.Key == foundThing.sheetName).Value;
            }
            return null;
        }

        public enum CreatureState { Idle, Moving, OnAction, Dead }

    }

}