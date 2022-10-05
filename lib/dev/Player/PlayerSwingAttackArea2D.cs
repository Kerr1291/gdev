using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace nv.Game
{
    [RequireComponent( typeof( Collider2D ) )]
    public class PlayerSwingAttackArea2D : MonoBehaviour
    {
        public PlayerController2D owner;
        public float cooldown = 0f;
        public bool hitOnStay = true;

        float cooldownRemaining = 0f;

        public bool IsOnCooldown {
            get => cooldownRemaining > 0f;
        }

        private void OnCollisionEnter2D( Collision2D collision )
        {
            TryHit( collision.gameObject );
        }

        private void OnTriggerEnter2D( Collider2D collision )
        {
            TryHit( collision.gameObject );
        }

        private void OnTriggerStay2D( Collider2D collision )
        {
            if( hitOnStay )
                TryHit( collision.gameObject );
        }

        private void OnCollisionStay2D( Collision2D collision )
        {
            if( hitOnStay )
                TryHit( collision.gameObject );
        }

        void TryHit( GameObject other )
        {
            if( IsOnCooldown )
                return;

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
