using Godot;
using Platformer.Source.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using static Godot.TextServer;
using static Player;

public partial class Player : CharacterBody2D
{
    [Signal]
    public delegate void PlayerJumpEventHandler();

    public enum Direction { Default, Left, Right, Portal }
    public enum CharacterState { Grounded, Falling, Jumping, FallFlat }

    private Random random;

    [Export]
    public float MoveSpeed = 90.0f;

    [Export]
    public float JumpSpeed = 100.0f;

    [Export]
    public float JumpVelocity = -310.0f;



    //Max Height of Y when hit by a mob or hitting a wall while in jumping motion
    [Export]
    private float knockBackVelocity = -350.0f;

    //Max length of X when hit by a mob or hitting a wall while in jumping motion
    [Export]
    private float knockBackSpeed = 100.0f;



    //Max Height of Y when hitting a wall while in jumping motion
    [Export]
    private float wallKnockBackVelocity = -300.0f;

    //Max length of X when hitting a wall while in jumping motion
    [Export]
    private float wallKnockBackSpeed = 100.0f;


    [Export]
    private float knockDownVelocity = 175.0f;

    [Export]
    private float knockDownSpeed = 80.0f;

    private int knockDownCount = 0;
    private int knockBackCount = 0;

    [Export]
    private int fallThreshHold = 2;

    [Export]
    private int currentFallThreshHold;


    [Export]
    private double timeBeforeFallFlat = 0.5f;


    private AnimatedSprite2D animatedSprite;
    private Area2D collisionDetectionArea;
    private CollisionShape2D collisionShape;
    private Timer airTimer;

   


    public Direction FaceDirection { get; private set; }
    public CharacterState CurrentCharacterState { get; private set; }


    private bool isHit;
    private bool knockback;
    private bool knockDown;
    private bool wallKnock;
    private bool fallFlat;

    //X value to tell us which way we should knockback
    private Vector2 hitDirection;


    public override void _Ready()
    {
        base._Ready();
        animatedSprite = this.GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        collisionDetectionArea = this.GetNode<Area2D>("CollisionArea");
        collisionShape = this.GetNode<CollisionShape2D>("CollisionShape2D");
        airTimer = this.GetNode<Timer>("AirTimer");
        FaceDirection = Direction.Default;
        CurrentCharacterState = CharacterState.Grounded;
        isHit = false;
        knockback = false;
        knockDown = false;
        wallKnock = false;
        fallFlat = false;
        hitDirection = Vector2.Zero;
        random = new Random();
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
                if (airTimer.IsStopped() && !fallFlat)
                {
                    airTimer.Start(timeBeforeFallFlat);
                }
            }
            else
            {
                airTimer.Stop();
                if (wallKnock)
                {
                    wallKnock = false;
                }

                isHit = false;
                knockDown = false;
                if (currentFallThreshHold >= fallThreshHold || fallFlat == true)
                {
                    CurrentCharacterState = CharacterState.FallFlat;
                    fallFlat = true;
                    if (this.FaceDirection == Direction.Left)
                    {
                        animatedSprite.Play("FallFlatLeft");
                    }
                    else
                    {
                        animatedSprite.Play("FallFlatRight");
                    }
                }
                else
                {
                    CurrentCharacterState = CharacterState.Grounded;
                }
            }


            if (knockback)
            {
                if (wallKnock)
                {
                    velocity.Y = wallKnockBackVelocity;
                    velocity.X = wallKnockBackSpeed * hitDirection.X;
                }
                else
                {
                    velocity.Y = knockBackVelocity;
                    velocity.X = knockBackSpeed * hitDirection.X;
                }

                knockback = false;

                if (hitDirection.X == Vector2.Left.X)
                {
                    this.FaceDirection = Direction.Left;
                    animatedSprite.Play("JumpLeft");
                }
                else if (hitDirection.X == Vector2.Right.X)
                {
                    this.FaceDirection = Direction.Right;
                    animatedSprite.Play("JumpRight");
                }
            }
            else if (knockDown)
            {

                velocity.Y = knockDownVelocity;
                velocity.X = knockDownSpeed * hitDirection.X;

                if (hitDirection.X == Vector2.Left.X)
                {
                    this.FaceDirection = Direction.Left;

                }
                else if (hitDirection.X == Vector2.Right.X)
                {
                    this.FaceDirection = Direction.Right;
                }
            }
            else
            {
                if(CurrentCharacterState == CharacterState.FallFlat)
                {
                    velocity = Vector2.Zero;
                    Vector2 direction = Input.GetVector("MoveLeft", "MoveRight", "MoveUp", "MoveDown");
                    if (direction != Vector2.Zero)
                    {
                        CurrentCharacterState = CharacterState.Grounded;
                        currentFallThreshHold = 0;
                        fallFlat = false;
                    }
                }
                else if (!wallKnock && CurrentCharacterState == CharacterState.Grounded)
                {
                    currentFallThreshHold = 0;
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

                    if (direction != Vector2.Zero )
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
                    else if (direction == Vector2.Zero && !Input.IsActionPressed("Jump"))
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


                    // Handle Jump.
                    if (Input.IsActionPressed("Jump") )
                    {
                        FaceDirection = Direction.Default;

                        animatedSprite.Play("Jump");
                        velocity.X = Mathf.MoveToward(Velocity.X, 0, MoveSpeed);
                    }


                    if (Input.IsActionJustReleased("Jump"))
                    {
                        EmitSignal(SignalName.PlayerJump);
                        velocity.Y = JumpVelocity;
                    }
                
                }
                else
                {
                    if (FaceDirection == Direction.Left)
                    {
                        animatedSprite.Play("JumpLeft");
                    }
                    else if (FaceDirection == Direction.Right)
                    {
                        animatedSprite.Play("JumpRight");
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
                    
                }
   

            }


        }
    }

    private void SetWallKnockBack(KinematicCollision2D collision)
    {

        Vector2 collisionNormal = collision.GetNormal().Round();
        // (1 , 0) player on right
        // (-1 , 0) player on left
        // (0 , 0) player in air
        // (0 , 1) // player under 
        // (0 , -1) // player on top

        // (1 , 1) // player on right under
        // (1 , -1) // player on right top

        // (-1 , -1) // player on left under
        // (-1 , 1) // player on left top

        if (collisionNormal == new Vector2(1, 1))
        {
            GD.Print("right under");
        }

        if (collisionNormal == new Vector2(1, -1))
        {
            GD.Print("right top");
        }

        if (collisionNormal == new Vector2(-1, 1))
        {
            GD.Print("left under");
        }

        if (collisionNormal == new Vector2(-1, -1))
        {
            GD.Print("left top");
        }

        if (CurrentCharacterState == CharacterState.Jumping || CurrentCharacterState == CharacterState.Falling)
        {
            float goLeft = collisionNormal.X;
            hitDirection = goLeft < 0 ? Vector2.Left : Vector2.Right;
            if (CurrentCharacterState == CharacterState.Jumping)
            {
                knockback = true;
                knockDown = false;
            }
            else if(CurrentCharacterState == CharacterState.Falling)
            {
                knockDown = true;
                knockback = false;

                if(collisionNormal.Y != -1)
                {
                    currentFallThreshHold++;
                    currentFallThreshHold = Math.Clamp(currentFallThreshHold, 0, fallThreshHold);
                }


            }
            wallKnock = true;
            isHit = true;
            if (collision.GetNormal().Y > 0)
            {
                if(goLeft == 0)
                {
                    Vector2 direction = Vector2.Zero;
                    if(FaceDirection == Direction.Left)
                    {
                        direction = Vector2.Left;
                    }
                    else if(FaceDirection == Direction.Right)
                    {
                        direction = Vector2.Right;
                    }

                    hitDirection = direction;
                }
            }
        }
    }



    public void _on_collision_area_body_entered(Mob mob)
    {
        Vector2 normal = (this.GlobalPosition - mob.GlobalPosition).Normalized().Round();

        float goLeft = normal.X;
        hitDirection = goLeft < 0 ? Vector2.Left : Vector2.Right;

        if (mob.MobType == Mob.TypeOfMob.Magician)
        {
            animatedSprite.Play("Portal");
            FaceDirection = Direction.Portal;
            collisionDetectionArea.SetDeferred("monitoring", false);
            collisionShape.SetDeferred("disabled", true);
        }
        else
        {
            knockback = true;
            isHit = true;
        }

    }


    public void _on_animated_sprite_2d_animation_finished()
    {
        if (FaceDirection == Direction.Portal)
        {
            FaceDirection = Direction.Default;
            collisionDetectionArea.SetDeferred("monitoring", true);
            collisionShape.SetDeferred("disabled", false);
            Vector2 playerTilePosition = PositionCalculator.GetPosition(this.GlobalPosition).Item1;

            HashSet<Vector2I> usedCells = this.GetParent<LevelBase>().GetSurroundingCellsInTileSizeCoOrds().ToHashSet<Vector2I>();

            Vector2I teleportedPosition = Vector2I.Zero;
            int y = 4;

            while (true)
            {
                int x;
                if (hitDirection == Vector2.Right)
                {
                    x = random.Next(2, 4);
                }
                else
                {
                    x = random.Next(-4, -1);
                }
                //Check if there is a tile present
                teleportedPosition = new Vector2I((int)playerTilePosition.X + x, (int)playerTilePosition.Y + y);
    
                if (!usedCells.Contains(teleportedPosition))
                {
                    Vector2 newPosition  = new Vector2(teleportedPosition.X * PositionCalculator.TileSize, teleportedPosition.Y * PositionCalculator.TileSize);

                    this.GlobalPosition = newPosition;
                    break;
                }
      
            }




            //if Position has a tile then Y+5 and so on
        }
    }

    public void _on_air_timer_timeout()
    {
        fallFlat = true;
    }



}
