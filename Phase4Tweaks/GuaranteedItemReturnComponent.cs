using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Phase4Tweaks
{
    public class GuaranteedItemReturnComponent : MonoBehaviour
    {
        public static float cooldownDuration = 30f;
        public static float closeRangeDistanceSqr = 30f * 30f;
        public static float closeRangeCooldownMult = 3f;
        private List<AttackerCooldown> cooldowns = new List<AttackerCooldown>();

        private void FixedUpdate()
        {
            if (!NetworkServer.active) return;
            float closeRangeTime = Time.fixedDeltaTime * closeRangeCooldownMult;
            foreach(AttackerCooldown ac in cooldowns)
            {
                bool close = false;
                if (ac.attacker)
                {
                    float distSqr = (ac.attacker.transform.position - base.transform.position).sqrMagnitude;
                    if (distSqr <= GuaranteedItemReturnComponent.closeRangeDistanceSqr) close = true;
                }
                ac.ReduceCooldown(close ? closeRangeTime : Time.fixedDeltaTime);
            }

            //Remove dead attackers
            cooldowns = cooldowns.Where(ac => ac.attacker != null).ToList();
        }

        public void SetStealItemCooldown(GameObject attacker)
        {
            if (!attacker) return;
            AttackerCooldown ac = cooldowns.Find(c => c.attacker == attacker);
            if (ac != null)
            {
                ac.SetCooldown(GuaranteedItemReturnComponent.cooldownDuration);
            }
            else
            {
                ac = new AttackerCooldown(attacker, GuaranteedItemReturnComponent.cooldownDuration);
                cooldowns.Add(ac);
            }
        }

        public bool ShouldGuaranteedSteal(GameObject attacker)
        {
            if (!attacker) return false;
            AttackerCooldown ac = cooldowns.Find(c => c.attacker == attacker);
            if (ac == null)
            {
                ac = new AttackerCooldown(attacker, 0f);
                cooldowns.Add(ac);
            }

            return ac.CanSteal();
        }

        private class AttackerCooldown
        {
            public GameObject attacker;
            float cooldown;

            public AttackerCooldown(GameObject attacker, float cooldown)
            {
                this.attacker = attacker;
                this.cooldown = cooldown;
            }

            public void SetCooldown(float duration)
            {
                cooldown = duration;
            }
            
            public void ReduceCooldown(float duration)
            {
                if (cooldown > 0f)
                {
                    cooldown -= duration;
                }
            }

            public bool CanSteal()
            {
                return cooldown <= 0f;
            }
        }
    }
}
