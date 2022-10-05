using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace nv.Game
{
    [RequireComponent(typeof(Collider))]
    public class DamageOnCollision : MonoBehaviour
    {
        public EnemyMain owner;
        public float cooldown = 0f;
        public bool hitOnStay = true;

        float cooldownRemaining = 0f;

        public bool IsOnCooldown {
            get => cooldownRemaining > 0f;
        }

        private void OnCollisionEnter( Collision collision )
        {
            TryHit( collision.gameObject );
        }

        private void OnTriggerEnter( Collider collision )
        {
            TryHit( collision.gameObject );
        }

        private void OnTriggerStay( Collider collision )
        {
            if( hitOnStay )
                TryHit( collision.gameObject );
        }

        private void OnCollisionStay( Collision collision )
        {
            if( hitOnStay )
                TryHit( collision.gameObject );
        }

        void TryHit(GameObject other)
        {
            if( IsOnCooldown )
                return;

            PlayerController player = other?.GetComponentInChildren<PlayerController>();
            if(player != null)
            {
                cooldownRemaining = cooldown;
                owner?.DoCollisionHit( player );
            }
        }

        private void Update()
        {
            if( IsOnCooldown )
            {
                cooldownRemaining -= Time.deltaTime;
            }
        }
    }
}
