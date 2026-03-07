using Godot;
using System;

public partial class PlatformerCharacterController2D : Node2D
{
    #region Exports

    [Export] private CharacterBody2D characterBody;
    [Export] private AnimatedSprite2D animatedSprite;
    [Export] private AnimationPlayer animationPlayer;
    [Export] private AnimationTree animationTree;

    #endregion

    #region Movement Parameters

    [ExportGroup("Movement")]
    [Export] private float moveSpeed = 200f;
    [Export] private float acceleration = 1200f;
    [Export] private float friction = 1000f;
    [Export] private float airAcceleration = 600f;
    [Export] private float airFriction = 200f;

    #endregion

    #region Jump Parameters

    [ExportGroup("Jump")]
    [Export] private float jumpVelocity = -400f;
    [Export] private float doubleJumpVelocity = -350f;
    [Export] private float gravity = 980f;
    [Export] private float maxFallSpeed = 600f;
    [Export] private float jumpCutMultiplier = 0.5f;

    #endregion

    #region Dash Parameters

    [ExportGroup("Dash")]
    [Export] private float dashSpeed = 1000f;
    [Export] private float dashDuration = 0.15f;
    [Export] private float dashCooldown = 0.8f;
    [Export] private int maxDashCharges = 2;

    #endregion

    #region Attack Parameters

    [ExportGroup("Attack")]
    [Export] private float heavyAttackHoldTime = 0.3f;

    #endregion

    #region State Machine

    private enum State
    {
        Idle,
        IdleToRun,
        Run,
        RunToIdle,
        Jump,
        JumpToFall,
        DoubleJump,
        Fall,
        Landing,
        FallToIdle,
        Attack1,
        Attack2,
        Attack3,
        HeavyAttack,
        Dash,
        Die
    }

    private State _currentState = State.Idle;
    private int _facingDirection = 1; // 1 = right, -1 = left
    private bool _hasDoubleJump = true;

    // Dash tracking
    private float _dashTimer = 0f;
    private int _dashCharges = 2;
    private float _dashRechargeTimer = 0f;

    // Attack tracking
    private float _attackPressTime = 0f;
    private bool _attackButtonHeld = false;
    private bool _comboRequested = false;
    private bool _heavyAttackTriggered = false;

    // Animations that should loop (all others play once)
    private static readonly string[] LoopingAnimations = { "idle", "run", "fall" };

    #endregion

    public override void _Ready()
    {
        // Auto-find nodes by relative path if not assigned via export
        characterBody ??= GetNode<CharacterBody2D>("CharacterBody2D");
        animatedSprite ??= GetNode<AnimatedSprite2D>("CharacterBody2D/AnimatedSprite2D");
        animationPlayer ??= GetNode<AnimationPlayer>("AnimationPlayer");
        animationTree = GetNodeOrNull<AnimationTree>("AnimationTree");

        // Disable AnimationTree — it overrides AnimationPlayer.Play() calls.
        // We drive animations entirely from code via AnimationPlayer.
        if (animationTree != null)
            animationTree.Active = false;

        animationPlayer.AnimationFinished += OnAnimationFinished;

        // Start in idle
        ChangeState(State.Idle);
    }

    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;

        // Recharge dash charges
        if (_dashCharges < maxDashCharges)
        {
            _dashRechargeTimer -= dt;
            if (_dashRechargeTimer <= 0f)
            {
                _dashCharges++;
                _dashRechargeTimer = dashCooldown;
            }
        }

        // Handle state-specific logic
        GD.Print($"Current State: {_currentState}");
        switch (_currentState)
        {
            case State.Idle:
                ProcessIdle(dt);
                break;
            case State.IdleToRun:
                ProcessIdleToRun(dt);
                break;
            case State.Run:
                ProcessRun(dt);
                break;
            case State.RunToIdle:
                ProcessRunToIdle(dt);
                break;
            case State.Jump:
                ProcessJump(dt);
                break;
            case State.JumpToFall:
                ProcessJumpToFall(dt);
                break;
            case State.DoubleJump:
                ProcessDoubleJump(dt);
                break;
            case State.Fall:
                ProcessFall(dt);
                break;
            case State.Landing:
                ProcessLanding(dt);
                break;
            // case State.FallToIdle:
            //     ProcessFallToIdle(dt);
            //     break;
            case State.Attack1:
            case State.Attack2:
            case State.Attack3:
                ProcessAttack(dt);
                break;
            case State.HeavyAttack:
                ProcessHeavyAttack(dt);
                break;
            case State.Dash:
                ProcessDash(dt);
                break;
        }

        characterBody.MoveAndSlide();
    }

    public override void _Input(InputEvent @event)
    {
        // Track attack button press/release for heavy attack detection
        if (@event.IsActionPressed("attack"))
        {
            _attackPressTime = 0f;
            _attackButtonHeld = true;
            _heavyAttackTriggered = false;
        }
        else if (@event.IsActionReleased("attack"))
        {
            _attackButtonHeld = false;
        }
    }

    #region State Processors

    private void ProcessIdle(float dt)
    {
        ApplyGravity(dt);
        ApplyFriction(dt, true);

        // Heavy attack detection (hold mouse)
        if (_attackButtonHeld)
        {
            _attackPressTime += dt;
            if (_attackPressTime >= heavyAttackHoldTime && !_heavyAttackTriggered)
            {
                _heavyAttackTriggered = true;
                ChangeState(State.HeavyAttack);
                return;
            }
        }

        // Short press attack — trigger on button release
        if (Input.IsActionJustReleased("attack") && !_heavyAttackTriggered)
        {
            ChangeState(State.Attack1);
            return;
        }

        if (Input.IsActionJustPressed("dash") && _dashCharges > 0)
        {
            ChangeState(State.Dash);
            return;
        }

        if (Input.IsActionJustPressed("jump") && characterBody.IsOnFloor())
        {
            ChangeState(State.Jump);
            return;
        }

        // Fall off edge
        if (!characterBody.IsOnFloor())
        {
            ChangeState(State.Fall);
            return;
        }

        float inputDir = GetMoveInput();
        if (Mathf.Abs(inputDir) > 0.1f)
        {
            UpdateFacing(inputDir);
            ChangeState(State.IdleToRun);
        }
    }

    private void ProcessIdleToRun(float dt)
    {
        ApplyGravity(dt);
        ApplyMovement(dt, true);

        if (!characterBody.IsOnFloor())
        {
            ChangeState(State.Fall);
            return;
        }

        if (Input.IsActionJustPressed("jump"))
        {
            ChangeState(State.Jump);
            return;
        }

        // AnimationFinished callback sets state to Run when transition completes
    }

    private void ProcessRun(float dt)
    {
        ApplyGravity(dt);
        ApplyMovement(dt, true);

        // Heavy attack detection
        if (_attackButtonHeld)
        {
            _attackPressTime += dt;
            if (_attackPressTime >= heavyAttackHoldTime && !_heavyAttackTriggered)
            {
                _heavyAttackTriggered = true;
                ChangeState(State.HeavyAttack);
                return;
            }
        }

        if (Input.IsActionJustReleased("attack") && !_heavyAttackTriggered)
        {
            ChangeState(State.Attack1);
            return;
        }

        if (Input.IsActionJustPressed("dash") && _dashCharges > 0)
        {
            ChangeState(State.Dash);
            return;
        }

        if (!characterBody.IsOnFloor())
        {
            ChangeState(State.Fall);
            return;
        }

        if (Input.IsActionJustPressed("jump"))
        {
            ChangeState(State.Jump);
            return;
        }

        float inputDir = GetMoveInput();
        if (Mathf.Abs(inputDir) < 0.1f)
        {
            ChangeState(State.RunToIdle);
            return;
        }

        UpdateFacing(inputDir);
    }

    private void ProcessRunToIdle(float dt)
    {
        ApplyGravity(dt);
        ApplyFriction(dt, true);

        if (!characterBody.IsOnFloor())
        {
            ChangeState(State.Fall);
            return;
        }

        if (Input.IsActionJustPressed("jump"))
        {
            ChangeState(State.Jump);
            return;
        }

        float inputDir = GetMoveInput();
        if (Mathf.Abs(inputDir) > 0.1f)
        {
            UpdateFacing(inputDir);
            ChangeState(State.IdleToRun);
            return;
        }

        // AnimationFinished callback handles transition to Idle
    }

    private void ProcessJump(float dt)
    {
        ApplyGravity(dt);
        ApplyMovement(dt, false);

        // Variable jump height
        if (Input.IsActionJustReleased("jump") && characterBody.Velocity.Y < 0)
        {
            var vel = characterBody.Velocity;
            vel.Y *= jumpCutMultiplier;
            characterBody.Velocity = vel;
        }

        if (Input.IsActionJustPressed("jump") && _hasDoubleJump)
        {
            ChangeState(State.DoubleJump);
            return;
        }

        if (Input.IsActionJustPressed("dash") && _dashCharges > 0)
        {
            ChangeState(State.Dash);
            return;
        }

        // Transition to fall when starting to descend
        if (characterBody.Velocity.Y > 0)
        {
            ChangeState(State.JumpToFall);
            return;
        }

        if (characterBody.IsOnFloor())
        {
            ChangeState(State.Landing);
        }
    }

    private void ProcessJumpToFall(float dt)
    {
        ApplyGravity(dt);
        ApplyMovement(dt, false);

        if (Input.IsActionJustPressed("jump") && _hasDoubleJump)
        {
            ChangeState(State.DoubleJump);
            return;
        }

        if (Input.IsActionJustPressed("dash") && _dashCharges > 0)
        {
            ChangeState(State.Dash);
            return;
        }

        if (characterBody.IsOnFloor())
        {
            ChangeState(State.Landing);
            return;
        }

        // AnimationFinished callback handles transition to Fall
    }

    private void ProcessDoubleJump(float dt)
    {
        ApplyGravity(dt);
        ApplyMovement(dt, false);

        if (Input.IsActionJustReleased("jump") && characterBody.Velocity.Y < 0)
        {
            var vel = characterBody.Velocity;
            vel.Y *= jumpCutMultiplier;
            characterBody.Velocity = vel;
        }

        if (Input.IsActionJustPressed("dash") && _dashCharges > 0)
        {
            ChangeState(State.Dash);
            return;
        }

        if (characterBody.Velocity.Y > 0)
        {
            ChangeState(State.Fall);
            return;
        }

        if (characterBody.IsOnFloor())
        {
            ChangeState(State.Landing);
        }
    }

    private void ProcessFall(float dt)
    {
        ApplyGravity(dt);
        ApplyMovement(dt, false);

        if (Input.IsActionJustPressed("jump") && _hasDoubleJump)
        {
            ChangeState(State.DoubleJump);
            return;
        }

        if (Input.IsActionJustPressed("dash") && _dashCharges > 0)
        {
            ChangeState(State.Dash);
            return;
        }

        if (characterBody.IsOnFloor())
        {
            ChangeState(State.Landing);
        }
    }

    private void ProcessLanding(float dt)
    {
        ApplyGravity(dt);
        ApplyFriction(dt, true);

        // Allow player to cancel landing animation with movement or jump
        if (Input.IsActionJustPressed("jump") && characterBody.IsOnFloor())
        {
            ChangeState(State.Jump);
            return;
        }

        float inputDir = GetMoveInput();
        if (Mathf.Abs(inputDir) > 0.1f)
        {
            UpdateFacing(inputDir);
            ChangeState(State.IdleToRun);
            return;
        }

        // AnimationFinished callback handles transition to Idle
    }

    private void ProcessAttack(float dt)
    {
        ApplyGravity(dt);
        ApplyFriction(dt, characterBody.IsOnFloor());

        // Dash cancels attack
        if (Input.IsActionJustPressed("dash") && _dashCharges > 0)
        {
            ChangeState(State.Dash);
            return;
        }

        // Buffer combo input during attack animation
        if (Input.IsActionJustReleased("attack") && !_heavyAttackTriggered)
        {
            _comboRequested = true;
        }

        // AnimationFinished callback handles combo chain / return to Idle
    }

    private void ProcessHeavyAttack(float dt)
    {
        ApplyGravity(dt);
        ApplyFriction(dt, characterBody.IsOnFloor());

        // Dash cancels heavy attack
        if (Input.IsActionJustPressed("dash") && _dashCharges > 0)
        {
            ChangeState(State.Dash);
            return;
        }

        // AnimationFinished callback handles return to Idle
    }

    private void ProcessDash(float dt)
    {
        _dashTimer -= dt;

        // Override velocity during dash (no gravity)
        var vel = characterBody.Velocity;
        vel.X = _facingDirection * dashSpeed;
        vel.Y = 0;
        characterBody.Velocity = vel;

        if (_dashTimer <= 0f)
        {
            // Kill dash momentum so the character doesn't slide
            characterBody.Velocity = Vector2.Zero;

            if (characterBody.IsOnFloor())
                ChangeState(State.Idle);
            else
                ChangeState(State.Fall);
        }
    }

    #endregion

    #region State Transitions

    private void ChangeState(State newState)
    {
        _currentState = newState;

        switch (newState)
        {
            case State.Idle:
                PlayAnimation("idle");
                _hasDoubleJump = true;
                break;

            case State.IdleToRun:
                PlayAnimation("idle_to_run");
                break;

            case State.Run:
                PlayAnimation("run");
                break;

            case State.RunToIdle:
                PlayAnimation("run_to_idle");
                break;

            case State.Jump:
                var jumpVel = characterBody.Velocity;
                jumpVel.Y = jumpVelocity;
                characterBody.Velocity = jumpVel;
                PlayAnimation("jump");
                break;

            case State.JumpToFall:
                PlayAnimation("jump_to_fall");
                break;

            case State.DoubleJump:
                _hasDoubleJump = false;
                var djVel = characterBody.Velocity;
                djVel.Y = doubleJumpVelocity;
                characterBody.Velocity = djVel;
                PlayAnimation("double_jump");
                break;

            case State.Fall:
                PlayAnimation("fall");
                break;

            case State.Landing:
                _hasDoubleJump = true;
                PlayAnimation("landing");
                break;

            case State.FallToIdle:
                PlayAnimation("fall_to_idle");
                break;

            case State.Attack1:
                _comboRequested = false;
                PlayAnimation("attack1");
                break;

            case State.Attack2:
                _comboRequested = false;
                PlayAnimation("attack2");
                break;

            case State.Attack3:
                _comboRequested = false;
                PlayAnimation("attack3");
                break;

            case State.HeavyAttack:
                PlayAnimation("heavy_attack");
                break;

            case State.Dash:
                _dashCharges--;
                _dashRechargeTimer = dashCooldown;
                _dashTimer = dashDuration;
                PlayAnimation("dash");
                break;
        }
    }

    #endregion

    #region Animation Handling

    private void PlayAnimation(string animName)
    {
        // Set correct loop mode before playing:
        // Only idle, run, fall should loop. Everything else plays once.
        if (animationPlayer.HasAnimation(animName))
        {
            var anim = animationPlayer.GetAnimation(animName);
            bool shouldLoop = Array.Exists(LoopingAnimations, a => a == animName);
            anim.LoopMode = shouldLoop
                ? Animation.LoopModeEnum.Linear
                : Animation.LoopModeEnum.None;
        }

        animationPlayer.Play(animName);
    }

    private void OnAnimationFinished(StringName animName)
    {
        string name = animName.ToString();

        switch (name)
        {
            // Transition animations → advance to next state
            case "idle_to_run":
                if (_currentState == State.IdleToRun)
                    ChangeState(State.Run);
                break;

            case "run_to_idle":
                if (_currentState == State.RunToIdle)
                    ChangeState(State.Idle);
                break;

            case "jump_to_fall":
                if (_currentState == State.JumpToFall)
                    ChangeState(State.Fall);
                break;

            case "landing":
            case "fall_to_idle":
                if (_currentState == State.Landing || _currentState == State.FallToIdle)
                    ChangeState(State.Idle);
                break;

            // Jump finishes → transition to fall if descending
            case "jump":
                if (_currentState == State.Jump)
                {
                    if (characterBody.Velocity.Y >= 0)
                        ChangeState(State.JumpToFall);
                }
                break;

            case "double_jump":
                if (_currentState == State.DoubleJump)
                {
                    if (characterBody.Velocity.Y >= 0)
                        ChangeState(State.Fall);
                }
                break;

            // Attack combo chain
            case "attack1":
                if (_currentState == State.Attack1)
                {
                    if (_comboRequested)
                        ChangeState(State.Attack2);
                    else
                        ChangeState(State.Idle);
                }
                break;

            case "attack2":
                if (_currentState == State.Attack2)
                {
                    if (_comboRequested)
                        ChangeState(State.Attack3);
                    else
                        ChangeState(State.Idle);
                }
                break;

            case "attack3":
            case "heavy_attack":
                ChangeState(State.Idle);
                break;

            case "dash":
                if (_currentState == State.Dash)
                {
                    characterBody.Velocity = Vector2.Zero;
                    if (characterBody.IsOnFloor())
                        ChangeState(State.Idle);
                    else
                        ChangeState(State.Fall);
                }
                break;
        }
    }

    #endregion

    #region Physics Helpers

    private float GetMoveInput()
    {
        return Input.GetAxis("move_left", "move_right");
    }

    private void ApplyGravity(float dt)
    {
        if (!characterBody.IsOnFloor())
        {
            var vel = characterBody.Velocity;
            vel.Y = Mathf.Min(vel.Y + gravity * dt, maxFallSpeed);
            characterBody.Velocity = vel;
        }
    }

    private void ApplyMovement(float dt, bool grounded)
    {
        float inputDir = GetMoveInput();
        float accel = grounded ? acceleration : airAcceleration;
        float fric = grounded ? friction : airFriction;

        var vel = characterBody.Velocity;

        if (Mathf.Abs(inputDir) > 0.1f)
        {
            vel.X = Mathf.MoveToward(vel.X, inputDir * moveSpeed, accel * dt);
            if (grounded) UpdateFacing(inputDir);
        }
        else
        {
            vel.X = Mathf.MoveToward(vel.X, 0, fric * dt);
        }

        characterBody.Velocity = vel;
    }

    private void ApplyFriction(float dt, bool grounded)
    {
        float fric = grounded ? friction : airFriction;
        var vel = characterBody.Velocity;
        vel.X = Mathf.MoveToward(vel.X, 0, fric * dt);
        characterBody.Velocity = vel;
    }

    private void UpdateFacing(float direction)
    {
        if (direction > 0.1f)
            _facingDirection = 1;
        else if (direction < -0.1f)
            _facingDirection = -1;

        animatedSprite.FlipH = _facingDirection < 0;
    }

    #endregion
}
