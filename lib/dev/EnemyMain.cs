using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace nv.Game
{
    public class EnemyMain : MonoBehaviour
    {
        public Action<GameObject, HitData> OnDie;
        public Action<GameObject, HitData> OnHit;

        [Header( "Required" )]
        public Rigidbody body;
        public Collider hitCollider;
        public HitEffect hitEffect;
        public DeathEffect deathEffect;

        [Header( "Settings" )]
        public int maxHP = 1;
        public bool canDie = true;

        public List<GameObject> ignoreHitFrom;

        public virtual int CurrentHP {
            get; protected set;
        }

        public virtual bool IsDead {
            get => CurrentHP <= 0 && canDie;
            protected set {
                if( value )
                {
                    CurrentHP = 0;
                    canDie = true;
                    ProcessDie();
                }
            }
        }

        public virtual bool ReceiveHit( HitData hitData )
        {
            if( !ValidateHit( hitData ) )
                return false;

            ProcessHit( hitData );

            OnHit?.Invoke( gameObject, hitData );

            if( IsDead )
            {
                ProcessDie();
                OnDie?.Invoke( gameObject, hitData );
            }
            else
            {
                hitEffect?.Play( hitCollider.bounds, hitData.hitZone );
            }
            return true;
        }

        [ContextMenu("Debug - Force Kill")]
        public virtual void ForceKill()
        {
            IsDead = true;
            OnDie?.Invoke( gameObject, null );
        }

        [ContextMenu( "Debug - Hit" )]
        public virtual void DebugHit()
        {
            HitData hit = new HitData()
            {
                damage = 1
                , owner = null
                , hitZone = hitCollider.bounds
            };
            ReceiveHit(hit);
        }

        protected virtual void Reset()
        {
            EditorSetup();
        }

        protected virtual void OnValidate()
        {
            EditorSetup();
        }

        protected virtual void Awake()
        {
            Setup();
        }

        protected virtual void EditorSetup()
        {
            if( body == null )
                body = GetComponentInParent<Rigidbody>();
        }

        protected virtual void Setup()
        {
            CurrentHP = maxHP;
        }

        protected virtual bool ValidateHit( HitData hitData )
        {
            return !IsDead && !ignoreHitFrom.Contains( hitData.owner );
        }

        protected virtual void ProcessHit( HitData hitData )
        {
            CurrentHP -= hitData.damage;
        }

        protected virtual void ProcessDie()
        {
            deathEffect?.Play();
            
            //TODO: replace with a better method of disabling dead enemies
            body.gameObject.SetActive( false );
        }

        public void DoCollisionHit(PlayerController player)
        {
            HitData hit = new HitData()
            {
                damage = 1
                ,
                owner = body.gameObject
                ,
                hitZone = hitCollider.bounds
            };
            player.ReceiveHit( hit );
        }
    }
}