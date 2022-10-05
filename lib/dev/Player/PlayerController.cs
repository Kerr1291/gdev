using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using System;

namespace nv.Game
{

    public class PlayerController : MonoBehaviour
    {
        public Action OnAttack;
        public Action<HitData> OnReceiveHit;
        public Action OnDie;

        [Header( "References" )]
        public Rigidbody body = default;
        public Animator anim;
        public Animator attackAnimator;
        public Collider hitCollider;
        public HitEffect hitEffect;
        public DeathEffect deathEffect;

        [Header( "Speed paramenters" )]
        public float walkingSpeed = 5;
        public float runningSpeed = 10;
        public float jumpForce = 10;

        [Header( "Health" )]
        public int baseMaxHP = 3;
        public List<GameObject> ignoreHitFrom;

        [Header( "Attacking" )]
        public float attackCooldownDuration = 0.4f;
        public AudioClipSet attackSFX;

        [Header( "Animation Parameters" )]
        public string AnimationParamName_MovementSpeed = "MovementSpeed";
        public string AnimationParamName_Attack = "Attack";

        //force variables
        Vector3 forceToAdd = default;
        bool stopMovement = false;
        float currentAttackCooldown;

        float RunAnimationRate {
            get => body.velocity.magnitude;
        }

        public virtual int MaxHP {
            get => baseMaxHP;
        }

        public virtual int CurrentHP {
            get; protected set;
        }

        public virtual bool IsDead {
            get => CurrentHP <= 0;
            protected set {
                if( value )
                {
                    CurrentHP = 0;
                    ProcessDie();
                }
            }
        }

        void Setup()
        {
            CurrentHP = MaxHP;
        }

        private void Awake()
        {
            SetupRewiredInput();
            Setup();
        }

        private void OnEnable()
        {
            body.sleepThreshold = 0f;
            EnableRewiredInputDelegates();
        }

        private void OnDisable()
        {
            DisableRewiredInputDelegates();
        }

        void Update()
        {
            UpdateCooldowns();
            ProcessDemoKeyboardInput();
            SetRunAnimationRate();
        }

        void FixedUpdate()
        {
            //TODO: For now, we will just modify the velocity directly, since
            //we want the character to not have acceleration. Once terrains types are
            //introduced, we'll probably have to modify this 
            if(stopMovement)
            {
                body.velocity = Vector3.zero;
                forceToAdd = Vector3.zero;
                stopMovement = false;
            }
            else if( forceToAdd.sqrMagnitude > 0 )
            {
                //update player orientation
                body.transform.forward = forceToAdd.normalized;

                //set velocity
                body.velocity = forceToAdd;

                forceToAdd = Vector3.zero;
            }

            
        }


        void ProcessDemoKeyboardInput()
        {
            if( UnityEngine.Input.GetKeyDown( KeyCode.Mouse0 ) )
                HandleButtonPressed_Attack( default );
        }



        void UpdateCooldowns()
        {
            if( currentAttackCooldown > 0f )
            {
                currentAttackCooldown -= Time.deltaTime;
            }
        }

        void SetRunAnimationRate()
        {
            anim?.SetFloat( AnimationParamName_MovementSpeed, RunAnimationRate );
        }

        public virtual bool ReceiveHit( HitData hitData )
        {
            if( !ValidateHit( hitData ) )
                return false;

            ProcessHit( hitData );

            OnReceiveHit?.Invoke( hitData );

            if( IsDead )
            {
                ProcessDie();
                OnDie?.Invoke();
            }
            else
            {
                hitEffect?.Play( hitCollider.bounds, hitData.hitZone );
            }
            return true;
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

        [ContextMenu( "Debug - Force Kill" )]
        public virtual void ForceKill()
        {
            IsDead = true;
            OnDie?.Invoke();
        }

        [ContextMenu( "Debug - Hit" )]
        public virtual void DebugHit()
        {
            HitData hit = new HitData()
            {
                damage = 1
                ,
                owner = null
                ,
                hitZone = hitCollider.bounds
            };
            ReceiveHit( hit );
        }

        public void Attack()
        {
            OnAttack?.Invoke();
            attackSFX?.PlayRandom();

            //animate the player to attack
            anim?.SetTrigger( AnimationParamName_Attack );

            //TODO: need to create an "AttackEffect" class like the hit and death effect to manage the attack
            //TODO: the attack effect will 'disable' the sword the player is holding and allow the attack effect to use it for the attack animation
            attackAnimator?.Play( "Attack" );

            currentAttackCooldown = attackCooldownDuration;

            //TODO: figure out how better to handle this
            int playerHitDamage = 1;
            HitData playerHit = new HitData()
            {
                damage = playerHitDamage,
                owner = gameObject
            };
        }

        //called when an enemy hits the player
        public void DoCollisionHit( EnemyMain enemyHit )
        {
            HitData hit = new HitData()
            {
                damage = 1
                ,
                owner = body.gameObject
                ,
                hitZone = hitCollider.bounds
            };
            enemyHit.ReceiveHit( hit );
        }

        //Handlers
        void HandleButtonPressed_Attack( InputActionEventData data )
        {
            Attack();
        }

        void HandleButtonPressed_Jump( InputActionEventData data )
        {
            forceToAdd = body.transform.up * jumpForce;
        }

        void HandleAxis_H( InputActionEventData data )
        {
            Move_H( data.GetAxis() );
        }

        void HandleButtonReleased( InputActionEventData data )
        {
            StopMovement();
        }

        void HandleAxis_V( InputActionEventData data )
        {
            Move_V( data.GetAxis() );
        }

        void Move_H( float normalizedAmount )
        {
            //use += here in case the player moves diagonal 
            forceToAdd += new Vector3( normalizedAmount, 0f, 0f ) * runningSpeed;
        }

        void Move_V( float normalizedAmount )
        {
            //use += here in case the player moves diagonal 
            forceToAdd += new Vector3( 0f, 0f, normalizedAmount ) * runningSpeed;
        }

        void StopMovement()
        {
            stopMovement = true;
            
        }

        //Input configuration
        [Header( "ReWired Input Settings" )]
        public int rewiredPlayerID = 0;
        Rewired.Player input;
        List<Action<InputActionEventData>> enabledDelegates;

        void SetupRewiredInput()
        {
            input = ReInput.players.GetPlayer( rewiredPlayerID );
        }


        void EnableRewiredInputDelegates()
        {
            DisableRewiredInputDelegates();
            enabledDelegates = new List<Action<InputActionEventData>>();

            AddDelegate( HandleButtonPressed_Attack, InputActionEventType.ButtonJustPressed, "Attack" );
            //AddDelegate(HandleButtonHeld_Jump, InputActionEventType.ButtonPressed, "Jump");

            AddDelegate( HandleAxis_H, InputActionEventType.ButtonPressed, "Move_H" );
            AddDelegate( HandleAxis_H, InputActionEventType.NegativeButtonPressed, "Move_H" );
            AddDelegate( HandleButtonReleased, InputActionEventType.ButtonJustReleased, "Move_H" );
            AddDelegate( HandleButtonReleased, InputActionEventType.NegativeButtonJustReleased, "Move_H" );

            AddDelegate( HandleAxis_V, InputActionEventType.ButtonPressed, "Move_V" );
            AddDelegate( HandleAxis_V, InputActionEventType.NegativeButtonPressed, "Move_V" );
            AddDelegate( HandleButtonReleased, InputActionEventType.ButtonJustReleased, "Move_V" );
            AddDelegate( HandleButtonReleased, InputActionEventType.NegativeButtonJustReleased, "Move_V" );

            //AddDelegate(HandleButtonPressed_Attack, InputActionEventType.ButtonJustPressed, "Attack");

            void AddDelegate( Action<InputActionEventData> indel, InputActionEventType eventType, string name )
            {
                input.AddInputEventDelegate( indel, UpdateLoopType.Update, eventType, name );
                enabledDelegates.Add( indel );
            }
        }

        void DisableRewiredInputDelegates()
        {
            if( enabledDelegates == null || enabledDelegates.Count <= 0 )
                return;

            foreach( var indel in enabledDelegates )
            {
                input.RemoveInputEventDelegate( indel );
            }
            enabledDelegates.Clear();
        }
    }
}