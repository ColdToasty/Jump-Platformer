using Godot;
using Platformer.Source.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using static Godot.TextServer;

public partial class Player : CharacterBody2D
{
    [Signal]
    public delegate void PlayerJumpEventHandler();

    public enum Direction { Default, Left, Right, Portal }
    public enum CharacterState { Grounded, Falling, Jumping }

    private Random random;

    public const float MoveSpeed = 100.0f;
    public const float JumpSpeed = 100.0f;
    public const float JumpVelocity = -310.0f;

    [Export]
    private float knockBackVelocity = -400.0f;
    [Export]
    private float knockBackSpeed = 150.0f;

    private AnimatedSprite2D animatedSprite;

    private Area2D collisionDetectionArea;
    private CollisionShape2D collisionShape;



    public Direction FaceDirection { get; private set; }
    public CharacterState CurrentCharacterState { get; private set; }


    private bool isHitByCroc;
    private bool knockback;
    private bool wallKnock;
    private bool canMove;

    private Vector2 hitDirection;





    public override void _Ready()
    {
        base._Ready();
        animatedSprite = this.GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        collisionDetectionArea = this.GetNode<Area2D>("CollisionArea");
        collisionShape = this.GetNode<CollisionShape2D>("CollisionShape2D");
        FaceDirection = Direction.Default;
        CurrentCharacterState = CharacterState.Grounded;
        isHitByCroc = false;
        knockback = false;
        wallKnock = false;
        canMove = true;
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

            }
            else
            {
                if (wallKnock)
                {
                    wallKnock = false;
                }
                CurrentCharacterState = CharacterState.Grounded;
                canMove = true;
            }


            if (knockback)
            {
                velocity.Y = knockBackVelocity;
                velocity.X = knockBackSpeed * hitDirection.X;
                knockback = false;
                canMove = false;
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
                if (!wallKnock & canMove)
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


            GD.Print(canMove);
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
        if(CurrentCharacterState == CharacterState.Jumping)
        {
            float goLeft = collision.GetNormal().X;
            hitDirection = goLeft < 0 ? Vector2.Left : Vector2.Right;
            knockback = true;
            wallKnock = true;
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

    private void SetMobKnockBack(KinematicCollision2D collision)
    {
        float goLeft = collision.GetNormal().X;
        hitDirection = goLeft < 0 ? Vector2.Left : Vector2.Right;

        Mob mob = (Mob)collision.GetCollider();

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



}
