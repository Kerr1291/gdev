using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using System;
using System.Linq;


//(rewired) input related TODOs: 
// Always make a mapping for the Unknown Controller.If a player uses a controller that isn't recognized, they can at least have some kind of input without manual mapping. Please see this for more information and recommended mapping guidelines.
//
// Always provide some means for your users to remap their controls. Many controllers cannot ever be recognized by Rewired due to bad practices by manufacturers. To support these controllers properly and give your users the best experience, you must provide them a way to remap their controls. You can either include Control Mapper directly in your game or create your own control remapping system.
////      
//If your game is 1-Player:
//    Set Max Joysticks Per Player to 100 so all controllers attached to the system may be used by the player.
// 
// Always initialize Rewired before intializing Steam to avoid potential exceptions being thrown( at least on Windows.) See this for more information on this issue.
// For the best controller support, it is recommended that you disable Steam's PS4, Xbox, and Generic Gamepad Configuration Support in the Steam developer back-end as well as informing your users to disable these settings in their Steam Big Picture controller configuration settings. See this for more information on this issue. Note that doing this has some tradeoffs which are addressed in the previous link.
//////////////////
///



namespace nv.Game
{
    public class PlayerController2D : MonoBehaviour
    {
        public CommunicationNode comms;

        public Action<int> OnHPChanged;
        public Action      OnEnterIdle;
        public Action      OnDestroyed;
        public Action      OnJump;
        public Action      OnAttack;
        public Action<HitData> OnReceiveHit;
        public Action      OnDie;

        [Header( "References" )]
        public Rigidbody2D body;
        public RaySensor groundSensor;
        public Collider2D hitCollider;
        public HitEffect hitEffect;
        public DeathEffect deathEffect;
        public Animator bodyAnimator;
        public Animator attackAnimator;
        public Transform animationRoot;

        public List<GameObject> ignoreHitFrom;

        [Header( "Health" )]
        public int baseMaxHP = 3;

        [Header( "Movement" )]
        public float baseXMovementPower = 100f;
        public float minxMovement = 0f;
        public float xDamping = .9f;
        public float idleMinVeloicty = .05f;

        [Header( "Jump" )]
        public float minJumpTime = .2f;
        public float baseJumpPower = 800f;
        public float triedToJumpResetTime = .2f;
        public float maxJumpBoostTime = .5f;
        public float baseJumpBoostPower = 800f;
        public AudioClipSet jumpSFX;

        [Header( "Falling" )]
        public float fallingVelocityThreshold = -.5f;

        [Header( "Attacking" )]
        public float attackCooldownDuration = 0.4f;
        public float attackComboCountdownDuration = 0.5f;
        public AudioClipSet attackSFX;

        [Header( "Attack Prefabs" )]
        public GameObject standingAttackHitFX;
        public GameObject standingAttackHitFX_2;

        Vector2 inputAxis;

        Vector2 jumpVector = Vector2.up;
        Vector2 walkVector = Vector2.right;
        Vector2 fallVector = Vector2.down;

        bool triedToJump;
        float triedToJumpCooldown;
        float jumpTime = 0f;
        bool jumping;

        float currentAttackComboCountdown;
        float currentAttackCooldown;
        int attackComboIndex;

        List<GameObject> currentAttacks = new List<GameObject>();

        float JumpBoostFactor {
            get => Mathf.Clamp01( 1f - ( jumpTime / maxJumpBoostTime ) );
        }

        public bool HasXInput {
            get => Mathf.Abs( inputAxis.x ) > 0f;
        }

        public bool HasYInput {
            get => Mathf.Abs( inputAxis.y ) > 0f;
        }

        public bool IsOnGround {
            get => groundSensor.HasHit;
        }

        bool Jumping {
            get => jumping;
            set {
                if( value && !jumping )
                    jumpTime = 0f;
                jumping = value;
            }
        }

        public bool CanJump {
            get => IsOnGround;
        }

        public bool CanWalk {
            get {
                return true;
            }
        }

        public bool IsFalling {
            get => Vector2.Dot( body.velocity, fallVector ) < fallingVelocityThreshold;
        }

        public bool CanAttack {
            get => currentAttackCooldown <= 0f;
        }

        public virtual int CurrentHP {
            get; protected set;
        }

        public virtual int MaxHP {
            get {
                return baseMaxHP;
            }
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

        // Misc

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
            //body.isKinematic = true;

            //if( itemDropper != null )
            //{
            //    itemDropper.DropItem( killingHitData );
            //}

            deathEffect?.Play();

            //TODO: replace with a better method of disabling dead enemies
            body.gameObject.SetActive( false );

            //Manager<PoolManager>.Instance.PoolObjectInstance( base.gameObject );
        }


        //Setup methods

        void Setup()
        {
            CurrentHP = MaxHP;
            comms.EnableNode( this );
        }

        private void OnValidate()
        {

        }

        private void Awake()
        {
            SetupRewiredInput();
            Setup();
        }

        private void Start()
        {

        }

        private void OnEnable()
        {
            EnableRewiredInputDelegates();
            bodyAnimator?.Play( "Idle" );
            attackAnimator?.Play( "Idle" );
        }

        private void OnDisable()
        {
            DisableRewiredInputDelegates();
        }

        private void OnApplicationQuit()
        {

        }


        //Update methods


        private void Update()
        {
            UpdateCooldowns();

            if( currentAttackComboCountdown > 0f )
            {
                currentAttackComboCountdown -= Time.deltaTime;
                if( currentAttackComboCountdown <= 0f )
                {
                    attackComboIndex = 0;
                }
            }
        }

        private void FixedUpdate()
        {
            if( triedToJump )
            {
                triedToJumpCooldown += Time.fixedDeltaTime;
                if( triedToJumpCooldown > triedToJumpResetTime )
                {
                    triedToJumpCooldown = 0f;
                    triedToJump = false;
                }
            }

            if( Jumping )
            {
                jumpTime += Time.fixedDeltaTime;

                if( jumpTime > minJumpTime )
                {
                    if( IsOnGround )
                    {
                        Jumping = false;

                        //the player recently tried to jump, so jump now!
                        if( triedToJump )
                            Jump();
                    }
                }
            }

            if( !HasXInput )
            {
                Vector2 walkPerp = Vector2.Perpendicular( walkVector.normalized );

                float horizontalMovement = Vector2.Dot( walkVector, body.velocity );

                if( Mathf.Abs(horizontalMovement) > minxMovement )
                {
                    body.velocity = Vector2.Dot( body.velocity, walkPerp ) * walkPerp + walkVector * horizontalMovement * xDamping;
                }
            }

            if( IsFalling )
            {
                bodyAnimator?.Play( "Falling" );
            }

            if( !HasXInput && body.velocity.sqrMagnitude <= idleMinVeloicty && CanAttack )
            {
                bodyAnimator?.Play( "Idle" );
            }
        }

        private void LateUpdate()
        {
            inputAxis = Vector2.zero;
        }

        void UpdateCooldowns()
        {
            if( currentAttackCooldown > 0f )
            {
                currentAttackCooldown -= Time.deltaTime;
            }
        }


        //Input handlers

        void HandleAxis_H( InputActionEventData data )
        {
            inputAxis.x = data.GetAxis();

            if( CanWalk )
            {
                Walk( inputAxis.x );
            }
        }

        void HandleAxis_V( InputActionEventData data )
        {
            inputAxis.y = data.GetAxis();
        }

        void HandleButtonPressed_Jump( InputActionEventData data )
        {
            if( CanJump )
            {
                Jump();
            }
            else
            {
                triedToJump = true;
                triedToJumpCooldown = 0f;
            }
        }

        void HandleButtonHeld_Jump( InputActionEventData data )
        {
            if( Jumping )
            {
                BoostJump();
            }
        }

        void HandleButtonPressed_Attack( InputActionEventData data )
        {
            if( CanAttack )
            {
                Attack();
            }
        }



        // Abilities


        void FlipCharacter( bool flip )
        {
            animationRoot.transform.localScale = animationRoot.transform.localScale.SetX( flip ? -1f: 1f );
        }

        void Walk(float walkAmount)
        {
            if( !Jumping )
                bodyAnimator?.Play( "Walk" );

            Vector2 walkPerp = Vector2.Perpendicular( walkVector.normalized );
            body.velocity = Vector2.Dot( body.velocity, walkPerp ) * walkPerp + walkVector * walkAmount * baseXMovementPower;

            if( CanAttack )
            {
                FlipCharacter( walkAmount < 0f );
            }
        }

        void Jump()
        {
            triedToJump = false;
            triedToJumpCooldown = 0f;

            bodyAnimator?.Play( "Jump" );

            Jumping = true;
            Vector2 jumpPerp = Vector2.Perpendicular( jumpVector.normalized );
            body.velocity = Vector2.Dot( body.velocity, jumpPerp ) * jumpPerp + jumpVector * baseJumpPower;
        }

        void BoostJump()
        {
            Vector2 jumpPerp = Vector2.Perpendicular( jumpVector.normalized );
            body.velocity += jumpVector * ( baseJumpBoostPower * JumpBoostFactor );
        }

        public void Attack()
        {
            OnAttack?.Invoke();
            attackSFX?.PlayRandom();

            bodyAnimator?.Play( "Attack" );
            attackAnimator?.Play( "Attack" );

            currentAttackCooldown = attackCooldownDuration;

            //TODO: figure out how better to handle this
            int playerHitDamage = 1;
            HitData playerHit = new HitData()
            {
                damage = playerHitDamage,
                owner = gameObject
            };            

            //GameObject attackFXPrefab;
            //////if( !this.Controller.collisionInfo.below )
            //////{
            //////    if( this.isOnWall )
            //////    {
            //////        attackFXPrefab = ( ( !flag ) ? this.wallAttackHitFX : this.wallChargedAttackHitFX );
            //////    }
            //////    else
            //////    {
            //////        attackFXPrefab = ( ( !flag ) ? this.airAttackHitFX : this.airChargedAttackHitFX );
            //////    }
            //////}
            //////else if( this.isDucking )
            //////{
            //////    attackFXPrefab = ( ( !flag ) ? this.crouchedAttackHitFX : this.crouchedChargedAttackHitFX );
            //////}
            //////else if( this.isRunning )
            //////{
            //////    attackFXPrefab = ( ( !flag ) ? this.runAttackHitFX : this.runChargedAttackHitFX );
            //////}
            //////else if( flag )
            //////{
            //////    attackFXPrefab = this.standingChargedAttackHitFX;
            //////}
            //////else
            //{
            //    attackFXPrefab = ( ( this.attackComboIndex != 0 ) ? this.standingAttackHitFX_2 : this.standingAttackHitFX );
            //}
            //////if( !this.isRunning && !this.isOnWall )
            //{
            //    this.attackComboIndex++;
            //    this.attackComboIndex = ( ( this.attackComboIndex <= 1 ) ? this.attackComboIndex : 0 );
            //    this.currentAttackComboCountdown = this.attackComboCountdownDuration;
            //}
            //this.SpawnAttackHitZone( attackFXPrefab, playerHit );
        }

        //private void SpawnAttackHitZone( GameObject attackFXPrefab, HitData hitData )
        //{
        //    GameObject attackObject = GameObject.Instantiate( attackFXPrefab );

        //    Collider2D hitCollider = attackObject.GetComponent<Collider2D>();

        //    if( hitCollider != null )
        //        hitData.hitZone = hitCollider.bounds;

        //    //PlayerAttackHitZone component = gameObject.GetComponent<PlayerAttackHitZone>();
        //    //component.Init( hitData, new Action<PlayerHitData, Hittable>( this.OnSuccessfullAttack ), new Action<PlayerAttackHitZone>( this.OnAttackDone ) );
        //    currentAttacks.Add( attackObject );
        //}

        //TODO: have this called by the spawned attack
        //private void OnAttackDone( GameObject attackDone )
        //{
        //    currentAttacks.Remove( attackDone );
        //}

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

            AddDelegate( HandleButtonPressed_Jump, InputActionEventType.ButtonJustPressed, "Jump" );
            AddDelegate( HandleButtonHeld_Jump, InputActionEventType.ButtonPressed, "Jump" );

            AddDelegate( HandleAxis_H, InputActionEventType.ButtonPressed, "Move_H" );
            AddDelegate( HandleAxis_H, InputActionEventType.NegativeButtonPressed, "Move_H" );

            AddDelegate( HandleAxis_V, InputActionEventType.ButtonPressed, "Move_V" );
            AddDelegate( HandleAxis_V, InputActionEventType.NegativeButtonPressed, "Move_V" );

            AddDelegate( HandleButtonPressed_Attack, InputActionEventType.ButtonJustPressed, "Attack" );

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
