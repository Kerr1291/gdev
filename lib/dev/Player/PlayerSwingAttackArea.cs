using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace nv.Game
{
    [RequireComponent( typeof( Collider ) )]
    public class PlayerSwingAttackArea : MonoBehaviour
    {
        public PlayerController owner;
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

        void TryHit( GameObject other )
        {
            if( IsOnCooldown )
                return;

            //TODO: change this to use "attachedRigidbody	The rigidbody the collider is attached to." on the unity colliders and update our conventions to keep the enemy/player controllers on the same game object as the rigid body
            EnemyMain enemy = other?.GetComponentInChildren<EnemyMain>();
            if( enemy == null )
            {
                enemy = other.transform.parent?.GetComponentInChildren<EnemyMain>();
            }

            if( enemy != null )
            {
                cooldownRemaining = cooldown;
                owner?.DoCollisionHit( enemy );
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
