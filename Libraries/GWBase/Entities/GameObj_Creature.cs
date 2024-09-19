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

        public delegate void ActionHappened(string actionType, object typeObj);
        public event ActionHappened onActionHappen = delegate(string type, object obj) {  };
        [SerializeField] private float maxhealth = 100;
        [SerializeField] private float hitColorSpeed = 0.1f;
        public int killCount = 0;
        public CreatureState currentState;
        public Transform ownedTransform;
        public ItemPickupManager pickupManager;
public GroupMemberReference groupAttached = new GroupMemberReference();
        public GroupMemberReference GroupAttached {
            set {
                groupAttached.group = value.group;
                groupAttached.position = value.position;
                if(groupAttached.group.groupLeader)transform.position = groupAttached.group.groupLeader.transform.position + groupAttached.position;
            }
            get {
                return groupAttached;
            }
        }
        public Vector2 lastAxis;

        bool isPlayingAnimation = false;
        string currentAnimationPlaying = "null";
        float animationPlayingSpeed = 1;
        bool animationLooping = false;
        public AnimationSheet movementAnimationSheet;

        public override void Possess<GameObj_Creature>(ThingDef entity, string faction)
        {
            base.Possess<GameObj_Creature>(entity, faction);
            PossessEquipments(entity, faction);
            killCount = 0;
            

            CallHealthEvent(new HealthInfo()
            {
                currentHealth = possessedThing.GetStatValueByName("Health"),
                damageTaken = 0,
                changeMax = true,
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
                Debug.LogWarning("[Error] initializing animation: " + ex.Message);
            }
        }


        public override void Spawned()
        {
            base.Spawned();
            ownedTransform = transform;
        }

        public void UpdateCharacterMovement(Vector2 axis) { lastMovementVector = axis; }

        public override void MoveObject(Vector2 axis, float delta)
        {
            lastAxis = axis;
            base.MoveObject(axis, delta);

            if (groupAttached.group?.groupLeader != null)
            {
                axis = groupAttached.group.groupLeader.lastAxis;
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
            base.FixedUpdate();
            if (currentState == CreatureState.Moving && movementAnimationSheet != null && movementAnimationSheet.info.frameCount > 0 && !isPlayingAnimation)
            {
                StartCoroutine(PlayAnimation(movementAnimationSheet));
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
                spawnedObj.transform.parent = transform;
                spawnedObj.transform.position = spawnedObj.transform.position + new Vector3(equipment.offset.x, equipment.offset.y, equipment.offset.z);
                spawnedObj.Possess<GameObj_Shooter>(YKUtility.FromXElement<ThingDef>(thingDef), faction);
                GameObj_Shooter shooter = (GameObj_Shooter)spawnedObj;
                shooter.onProjectileHit += OnHitToEnemy;
                shooter.stats = possessedThing.stats;

                if(equipment.offset.flipOffset == "Yes") {
                    spawnedObj.doesMirrorPosOnFacing = true;
                    onFacingDirectionChange += (spawnedObj.FlipLocalPos);

                }
            }
        }

        public void OnHitToEnemy(HitResult result)
        {
            onActionHappen.Invoke("hitGiven", result);
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
                damageTaken = -amount,
                infoOf = this
            });
        }

        public bool TryDamage(float amount, out bool endedUpKilling)
        {
            onActionHappen.Invoke("hitTaken", amount);
            possessedThing.RemoveFromStat("Health", amount);

            var audioClip = GetAudioClipOf(possessedThing.soundConfig.onDamageTakenSounds);
            AudioSource.PlayClipAtPoint(audioClip, Vector3.zero);

            if (!IsHealthDepleted()) StartCoroutine(HitColorChange());
            endedUpKilling = IsHealthDepleted();
            var currentHealthInfo = new HealthInfo()
            {
                currentHealth = possessedThing.GetStatValueByName("Health"),
                damageTaken = amount,
                gotKilled = endedUpKilling,
                infoOf = this
            };
            
            CallHealthEvent(currentHealthInfo);
            return true;
        }

        public IEnumerator HitColorChange()
        {
            ownedSpriteRenderer.color = Color.red;
            yield return new WaitForSeconds(hitColorSpeed);
            ownedSpriteRenderer.color = Color.black;
            yield return new WaitForSeconds(hitColorSpeed);
            ownedSpriteRenderer.color = Color.white;
        }

        public bool TryDestroy()
        {
            if (IsHealthDepleted())
            {
                var audioClip = GetAudioClipOf(possessedThing.soundConfig.onDeathSounds);
                AudioSource.PlayClipAtPoint(audioClip, Vector3.zero);

                gameObject.SetActive(false);
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

        public IEnumerator PlayAnimation(AnimationSheet animationDef)
        {
            {
                if (animationDef.info.frameCount == 0 || animationDef == null || isPlayingAnimation) yield return this;
                isPlayingAnimation = true;
                currentAnimationPlaying = animationDef.info.sheetName;
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