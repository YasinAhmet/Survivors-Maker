using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using GWBase;
using UnityEngine;

namespace GWMisc
{
    public class HookerBrain : ChasePlayer
    {
        public HookEvent HookEvent = new HookEvent();
        public Sprite HookSprite;
        public float hookSpeed;
        public float hookPower;
        public float hookLifetime;
        public Collider2D targettedCollider;

        public void IfDeadRemoveHooks(HealthInfo info)
        {
            if (info.gotKilled)
            {
                HookEvent.ObjReference.SetActive(false);
            }
        }

        public void Suspend()
        {
            ownedObject.onHealthChange -= IfDeadRemoveHooks;
        }

        Transform ownedtransform;
        public override void Start(XElement possess, object[] parameters, CustomParameter[] customParameters)
        {
            
            ownedObject = (GameObj_Creature)parameters[0];
            objectToFollow = ((PlayerController)parameters[1]).ownedCreature;
            InitializeVariables(ownedObject);
            HookSprite = AssetManager.assetLibrary.texturesDictionary.FirstOrDefault(x => x.Key == "Hook").Value;
            targettedCollider = objectToFollow.GetComponent<Collider2D>();

            ownedObject.onHealthChange += IfDeadRemoveHooks;
            hookSpeed = float.Parse(customParameters.FirstOrDefault(x => x.parameterName.Equals("HookSpeed")).parameterValue, CultureInfo.InvariantCulture);
            hookPower = float.Parse(customParameters.FirstOrDefault(x => x.parameterName.Equals("HookPower")).parameterValue, CultureInfo.InvariantCulture);
            hookLifetime = float.Parse(customParameters.FirstOrDefault(x => x.parameterName.Equals("HookLifetime")).parameterValue, CultureInfo.InvariantCulture);
            ownedtransform = ownedObject.transform;
            
        }

        public override void RareTick(object[] parameters, float deltaTime)
        {
            TickHook(deltaTime);
            
            if (!HookEvent.Ongoing)
            {
                InIdleCase();
            }
            
            if (HookEvent.Ongoing && !HookEvent.Entangled)
            {
                InThrowingCase(deltaTime);
            }
            
            if (HookEvent.Ongoing && HookEvent.Entangled)
            {
                InPullingCase(deltaTime);
            }
            
        }

        public void TickHook(float deltaTime)
        {
            HookEvent.RemainingLifeTime -= deltaTime;
            if (HookEvent.RemainingLifeTime < 0 && HookEvent.Ongoing)
            {
                HookEvent.ObjReference.transform.localScale = new Vector3(1, 1, 1);
                HookEvent.ObjReference.SetActive(false);
                HookEvent.Ongoing = false;
                ownedObject.currentState = GameObj_Creature.CreatureState.Idle;
            }
        }

        public void InThrowingCase(float deltaTime)
        {
            var objTransform = this.HookEvent.ObjReference.transform;
            var right = objTransform.right;
            var position = objTransform.position;
            var change = (right * hookSpeed) * deltaTime;
            var raycast = Physics2D.Raycast(position, change, change.sqrMagnitude, LayerMask.NameToLayer(objectToFollow.faction));
            objTransform.position += change;
            
            if (raycast.collider)
            {
                Collider2D result = raycast.collider;
                HookEvent.Entangled = true;
                HookEvent.EntangledThing = objectToFollow;
                HookEvent.ObjReference.transform.position = HookEvent.EntangledThing.transform.position;
                
            }
        }

        public void InPullingCase(float deltaTime)
        {
            Vector3 entangledTransformPos = HookEvent.EntangledThing.transform.position;
            var axisTowardsHooker = YKUtility.GetDirection(entangledTransformPos,
                ownedtransform.position);
            HookEvent.EntangledThing.MoveObject(axisTowardsHooker, deltaTime*hookPower, false);
            HookEvent.ObjReference.transform.position = entangledTransformPos;
        }
        public void InIdleCase()
        {
            Vector3 objectToFollowPos = objectToFollow.ownedTransform.position;
            ownedObject.directionLookingAt = YKUtility.GetDirection(ownedObject.transform.position,
                objectToFollowPos + (Vector3)objectToFollow.ownedRigidbody.velocity);
            TargetInRange(ownedtransform.position, objectToFollowPos,
                out Vector3 direction, out float distance);

            if (distance < reachDistance) ThrowHook();
            else TryMove(direction);
        }

        public void ThrowHook()
        {
            var pool = PoolManager.poolManager.GetLightObjectPool("Effects");
            var ownedTransform = ownedObject.ownedTransform;
            var position = ownedTransform.position;
            var rotation = YKUtility.GetRotationToTargetPoint(position, objectToFollow.ownedTransform.position);
            var obj = pool.ObtainSlotForType(position, rotation);
            obj.name = "Hook";
            obj.GetComponent<SpriteRenderer>().sprite = HookSprite;
            HookEvent = new HookEvent()
            {
                ObjReference = obj,
                Ongoing = true,
                RemainingLifeTime = hookLifetime
            };

            HookEvent.ObjReference.transform.localScale = new Vector3(8, 8, 1);
            ownedObject.currentState = GameObj_Creature.CreatureState.OnAction;
            ownedObject.ownedRigidbody.velocity = Vector2.zero;
            ownedObject.UpdateCharacterMovement(Vector2.zero);
        }
    }

    public struct HookEvent
    {
        public GameObject ObjReference;
        public Rigidbody2D rb;
        public bool Ongoing;
        public bool Entangled;
        public GameObj EntangledThing;
        public float RemainingLifeTime;
    }
}