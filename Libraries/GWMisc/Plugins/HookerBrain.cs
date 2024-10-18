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

        public override void Start(XElement possess, object[] parameters)
        {
            ownedObject = (GameObj_Creature)parameters[0];
            objectToFollow = ((PlayerController)parameters[1]).ownedCreature;
            InitializeVariables(ownedObject);
            HookSprite = AssetManager.assetLibrary.texturesDictionary.FirstOrDefault(x => x.Key == "Hook").Value;

            ownedObject.onHealthChange += IfDeadRemoveHooks;
        }

        public void IfDeadRemoveHooks(HealthInfo info)
        {
            if (info.gotKilled)
            {
                HookEvent.ObjReference.SetActive(false);
            }
        }

        Transform ownedtransform;
        public override Task Start(XElement possess, object[] parameters, CustomParameter[] customParameters)
        {
            Start(possess, parameters);
            hookSpeed = float.Parse(customParameters.FirstOrDefault(x => x.parameterName.Equals("HookSpeed")).parameterValue, CultureInfo.InvariantCulture);
            hookPower = float.Parse(customParameters.FirstOrDefault(x => x.parameterName.Equals("HookPower")).parameterValue, CultureInfo.InvariantCulture);
            ownedtransform = ownedObject.transform;
            return Task.CompletedTask;
        }

        public override void RareTick(object[] parameters, float deltaTime)
        {
            HookEvent.RemainingLifeTime -= deltaTime;
            if (HookEvent.RemainingLifeTime < 0 && HookEvent.Ongoing)
            {
                //Debug.Log("[HOOKER] Hook Gone");
                HookEvent.ObjReference.transform.localScale = new Vector3(1, 1, 1);
                HookEvent.ObjReference.SetActive(false);
                HookEvent.Ongoing = false;
                ownedObject.currentState = GameObj_Creature.CreatureState.Idle;
            }
            if (HookEvent.Ongoing && !HookEvent.Entangled)
            {
                //Debug.Log("[HOOKER] GOING," + HookEvent.ObjReference.name);
                var objTransform = this.HookEvent.ObjReference.transform;
                objTransform.position += (objTransform.right*hookSpeed)*deltaTime;

                var objectsOverlapped = Physics2D.OverlapCircleAll(objTransform.position, 1);
                foreach (var overlapped in objectsOverlapped)
                {
                    if (overlapped.TryGetComponent<IDamageable>(out IDamageable damageable) && damageable.GetTeam() == objectToFollow.faction)
                    {
                        HookEvent.Entangled = true;
                        HookEvent.EntangledThing = overlapped.GetComponent<GameObj>();
                        HookEvent.ObjReference.transform.position = HookEvent.EntangledThing.transform.position;
                    }
                }
            }
            else if (HookEvent.Ongoing && HookEvent.Entangled)
            {
                Vector3 entangledTransformPos = HookEvent.EntangledThing.transform.position;
                //Debug.Log("[HOOKER] PULLING");
                var axisTowardsHooker = YKUtility.GetDirection(entangledTransformPos,
                    ownedtransform.position);
                HookEvent.EntangledThing.MoveObject(axisTowardsHooker, deltaTime*hookPower, false);
                HookEvent.ObjReference.transform.position = entangledTransformPos;
            }
            else if (!HookEvent.Ongoing)
            {
                Vector3 objectToFollowPos = objectToFollow.transform.position;
                ownedObject.directionLookingAt = YKUtility.GetDirection(ownedObject.transform.position,
                    objectToFollowPos);
                TargetInRange(ownedtransform.position, objectToFollowPos,
                    out Vector3 direction, out float distance);

                if (distance < reachDistance) ThrowHook();
                else TryMove(direction);
            }
        }

        public void ThrowHook()
        {
            // Debug.Log("[HOOKER] Throwing Hook");
            var pool = PoolManager.poolManager.GetLightObjectPool("Effects");
            var ownedTransform = ownedObject.ownedTransform;
            var position = ownedTransform.position;
            var rotation = YKUtility.GetRotationToTargetPoint(position, position + (Vector3)objectToFollow.ownedRigidbody.velocity);
            var obj = pool.ObtainSlotForType(position, rotation);
            obj.name = "Hook";
            obj.GetComponent<SpriteRenderer>().sprite = HookSprite;
            HookEvent = new HookEvent()
            {
                ObjReference = obj,
                Ongoing = true,

                rb = obj.AddComponent<Rigidbody2D>(),
                RemainingLifeTime = 5f
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