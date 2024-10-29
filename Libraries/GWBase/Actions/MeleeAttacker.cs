using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;

namespace GWBase
{
    public class MeleeAttacker : IObjBehaviour
    {
        public bool iniatingAttack;
        public float attackTime = 0f;
        public float closeness = 0.1f;
        public float attackCooldown;
        public float hitboxSize;
        private float cooldownCounter;
        private float attackTimeCounter;
        public GameObj_Creature ownedObject;
        private bool doesHitInstant = false;
        //private bool recalculatesHitBox = false;

        private float criticalStrikeChance;
        private float criticalStrikeMultiplier;

        public delegate void AttackPreparing(float timeRemaining, GameObj causer);
        public delegate void AttackHappened(bool didKill, float damage, GameObj causer, GameObj taker);

        public event AttackPreparing preparing;
        public event AttackHappened happened;
        
        public void Start(XElement possess, object[] parameters, CustomParameter[] customParameters)
        {
            attackTime = float.Parse(customParameters.FirstOrDefault(x => x.parameterName.Equals("AttackTime")).parameterValue, CultureInfo.InvariantCulture);
            attackCooldown = float.Parse(customParameters.FirstOrDefault(x => x.parameterName.Equals("AttackCooldown")).parameterValue, CultureInfo.InvariantCulture);
            hitboxSize = float.Parse(customParameters.FirstOrDefault(x => x.parameterName.Equals("HitboxSize")).parameterValue, CultureInfo.InvariantCulture);
            closeness = float.Parse(customParameters.FirstOrDefault(x => x.parameterName.Equals("HitboxForwardOffset")).parameterValue, CultureInfo.InvariantCulture);
            criticalStrikeMultiplier = float.Parse(customParameters.FirstOrDefault(x => x.parameterName.Equals("CriticalStrikeMultiplier")).parameterValue, CultureInfo.InvariantCulture);
            criticalStrikeChance = float.Parse(customParameters.FirstOrDefault(x => x.parameterName.Equals("CriticalStrikeChance")).parameterValue, CultureInfo.InvariantCulture);
            ownedObject = (GameObj_Creature)parameters[0];
            ownedObject.actionRequested += OwnedObjectOnactionRequested;

            try
            {
                doesHitInstant =
                    customParameters.FirstOrDefault(x => x.parameterName.Equals("HitsInstantly")).parameterValue ==
                    "Yes";
            }
            catch
            {
                doesHitInstant = false;
            }
        }

        private void OwnedObjectOnactionRequested(string actionname)
        {
            if(actionname == "MeleeAttack") PrepareAttack();
        }

        public void PrepareAttack()
        {
            if (cooldownCounter < attackCooldown || iniatingAttack) return;
            ownedObject.UpdateCharacterMovement(Vector2.zero);
            ownedObject.currentState = GameObj_Creature.CreatureState.OnAction;
            iniatingAttack = true;
            if (doesHitInstant)
            {
                Attack();
            }
            else
            {
                preparing?.Invoke(attackTime, ownedObject);
            }
        }

        public void Attack()
        {
            foreach (var collider in GetHits)
            {
                if(collider.TryGetComponent<GameObj>(out GameObj obj))
                {
                    if(obj.faction == ownedObject.faction) continue;
                    float dmg = ownedObject.GetPossessed().GetStatValueByName("Damage");
                    if (Random.Range(0f, 1.0f) < criticalStrikeChance) dmg *= criticalStrikeMultiplier;

                    obj.TryDamage(dmg, out HealthInfo healthInfo, ownedObject);
                    
                    happened?.Invoke(healthInfo.gotKilled, dmg, ownedObject, obj);
                    obj.StartColorChange(Color.green);
                }
            }
            
            cooldownCounter = 0;
            iniatingAttack = false;
            ownedObject.currentState = GameObj_Creature.CreatureState.Idle;
        }

        public Collider2D[]  GetHits
        {
            get
            {
                Vector3 forward = ownedObject.directionLookingAt*closeness;
                Vector3 targetPos = ownedObject.transform.position + forward;
                float angle = YKUtility.GetRotationToTargetPoint(ownedObject.ownedTransform.position, targetPos);
                return Physics2D.OverlapBoxAll(targetPos, new Vector2(hitboxSize, hitboxSize), angle);
            }
        }

        public GameObject collisionChecker;
        public void Tick(object[] parameters, float deltaTime)
        {
            return;
        }

        public void RareTick(object[] parameters, float deltaTime)
        {
            cooldownCounter += deltaTime;
            if (iniatingAttack)
            {
                attackTimeCounter += deltaTime;
                if (attackTimeCounter >= attackTime)
                {
                    Attack();
                    attackTimeCounter = 0;
                }
            }
        }

        public void Suspend(object[] parameters)
        {
            return;
        }

        public string GetName()
        {
            return "MeleeAttacker";
        }

        public ParameterRequest[] GetParameters()
        {
            return null;
        }
    }
}