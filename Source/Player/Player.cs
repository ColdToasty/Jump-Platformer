using Godot;
using Platformer.Source.Util;
using System;
using static Godot.TextServer;

public partial class Player : CharacterBody2D
{
    public const float MoveSpeed = 125.0f;
    public const float JumpSpeed = 125.0f;
    public const float JumpVelocity = -300.0f;

    [Export]
    private float knockBackVelocity = -300.0f;

    private AnimatedSprite2D animatedSprite;

    public enum Direction { Default, Left, Right, Portal }

    public Direction FaceDirection {get; private set;}

    private bool isHitByCroc;
    private bool knockback;
    private  bool wallKnock;


    private Vector2 hitDirection;

    public enum CharacterState { Grounded, Falling, Jumping}

    public CharacterState CurrentCharacterState { get; private set; }

    private Area2D collisionDetectionArea;



    [Signal]
    public delegate void PlayerJumpEventHandler();

    public override void _Ready()
    {
        base._Ready();
        animatedSprite = this.GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        collisionDetectionArea = this.GetNode<Area2D>("CollisionArea");
        FaceDirection = Direction.Default;
        CurrentCharacterState = CharacterState.Grounded;
        isHitByCroc = false;
        knockback = false;
        wallKnock = false;
        hitDirection = Vector2.Zero;

    }

    public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

        if(FaceDirection != Direction.Portal)
        {
            // Add the gravity.
            if (!IsOnFloor())
            {
                velocity += GetGravity() * (float)delta;

                if (velocity.Y > 0)
                {
                    CurrentCharacterState = CharacterState.Falling;
                }
                else
                {
                    CurrentCharacterState = CharacterState.Jumping;
                }

            }
            else
            {
                if (wallKnock)
                {
                    wallKnock = false;
                }

                CurrentCharacterState = CharacterState.Grounded;
            }


            if (knockback)
            {
                velocity.Y = knockBackVelocity;
                velocity.X = JumpSpeed * hitDirection.X;
                knockback = false;

                if (hitDirection.X < 0)
                {
                    this.FaceDirection = Direction.Left;
                    animatedSprite.Play("JumpLeft");
                }
                else if (hitDirection.X > 0)
                {
                    this.FaceDirection = Direction.Right;
                    animatedSprite.Play("JumpRight");
                }
            }
            else
            {
                if (!wallKnock)
                {

                    // Get the input direction and handle the movement/deceleration.
                    // As good practice, you should replace UI actions with custom gameplay actions.
                    Vector2 direction = Input.GetVector("MoveLeft", "MoveRight", "MoveUp", "MoveDown");

                    if (direction.X < 0)
                    {
                        this.FaceDirection = Direction.Left;
                    }
                    else if (direction.X > 0)
                    {
                        this.FaceDirection = Direction.Right;

                    }


                    if (direction != Vector2.Zero && CurrentCharacterState == CharacterState.Grounded)
                    {
                        velocity.X = direction.X * MoveSpeed;

                        if (direction.X < 0)
                        {
                            animatedSprite.Play("MoveLeft");
                        }
                        else if (direction.X > 0)
                        {
                            animatedSprite.Play("MoveRight");
                        }
                    }
                    else if (direction == Vector2.Zero && CurrentCharacterState == CharacterState.Grounded && !Input.IsActionPressed("Jump"))
                    {
                        velocity.X = Mathf.MoveToward(Velocity.X, 0, MoveSpeed);
                        if (CurrentCharacterState == CharacterState.Grounded)
                        {
                            if (FaceDirection == Direction.Left)
                            {
                                animatedSprite.Play("IdleLeft");
                            }
                            else if (FaceDirection == Direction.Right)
                            {
                                animatedSprite.Play("IdleRight");
                            }
                            else
                            {
                                animatedSprite.Play("Idle");
                            }
                        }

                    }


                    if (CurrentCharacterState == CharacterState.Falling || CurrentCharacterState == CharacterState.Jumping)
                    {
                        if (FaceDirection == Direction.Left)
                        {
                            animatedSprite.Play("JumpLeft");
                        }
                        else if (FaceDirection == Direction.Right)
                        {
                            animatedSprite.Play("JumpRight");
                        }

                        if (direction != Vector2.Zero)
                        {
                            velocity.X = direction.X * MoveSpeed;
                        }
                    }




                    // Handle Jump.
                    if (Input.IsActionPressed("Jump") && CurrentCharacterState == CharacterState.Grounded)
                    {
                        FaceDirection = Direction.Default;
                        animatedSprite.Play("Jump");
                        velocity.X = Mathf.MoveToward(Velocity.X, 0, MoveSpeed);
                    }


                    if (Input.IsActionJustReleased("Jump") && CurrentCharacterState == CharacterState.Grounded)
                    {
                        EmitSignal(SignalName.PlayerJump);
                        velocity.Y = JumpVelocity;
                    }
                
                }
            }

            Velocity = velocity;
            bool isColliding = MoveAndSlide();

            if (isColliding)
            {

                var collision = this.GetLastSlideCollision();

                GodotObject collisionObject = collision.GetCollider();
                if (collisionObject is TileMapLayer)
                {
                    SetWallKnockBack(collision);
                }
                else if(collisionObject is Mob)
                {
                    SetMobKnockBack(collision);
                }
   

            }


        }
    }

    private void SetWallKnockBack(KinematicCollision2D collision)
    {
        if(CurrentCharacterState == CharacterState.Jumping)
        {
            float goLeft = collision.GetNormal().X;
            hitDirection = goLeft < 0 ? Vector2.Left : Vector2.Right;
            knockback = true;
            wallKnock = true;
        }
    }

    private void SetMobKnockBack(KinematicCollision2D collision)
    {
        float goLeft = collision.GetNormal().X;
        hitDirection = goLeft < 0 ? Vector2.Left : Vector2.Right;
        knockback = true;

        Mob mob = (Mob)collision.GetCollider();

        if (mob.MobType == Mob.TypeOfMob.Magician)
        {
            animatedSprite.Play("Portal");
            FaceDirection = Direction.Portal;
            collisionDetectionArea.SetDeferred("monitoring", false);
        }
        else
        {
            knockback = true;
        }
    }

    public void _on_animated_sprite_2d_animation_finished()
    {
        if (FaceDirection == Direction.Portal)
        {

            FaceDirection = Direction.Default;
            collisionDetectionArea.SetDeferred("monitoring", true);
        }
    }



}
