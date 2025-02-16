using Godot;
using Platformer.Source.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;

public partial class Player : CharacterBody2D
{
    [Signal]
    public delegate void PlayerJumpEventHandler();

    public enum Direction { Default, Left, Right, Portal }
    public enum CharacterState { Grounded, Falling, Jumping, FallFlat }

    private Random random;

    [Export]
    public float MoveSpeed;

    [Export]
    public float JumpSpeed;

    //Maximum Y height of a jump
    [Export]
    public float MaximumJumpVelocity;

    //Minimum Y height of a jump
    [Export]
    private float MinimumJumpVelocity;

    [Export]
    private float jumpGravity;
    [Export]
    private float fallGravity;

    [Export]
    private float knockbackGravity;

    [Export]
    private float wallKnockbackGravity;

    [Export]
    private float wallKnockdowGravity;

    [Export]
    private float CurrentJumpVelocity;

    //Max Height of Y when hit by a mob or hitting a wall while in jumping motion

    [Export]
    private float knockBackVelocity;

    //Max length of X when hit by a mob or hitting a wall while in jumping motion
    [Export]
    private float knockBackSpeed;


    ///////////////////// when hitting a wall while in the jumping motion of a jump 

    //Max Height of Y when hitting a wall while in jumping motion
    [Export]
    private float wallKnockBackVelocity;

    //Max length of X when hitting a wall while in jumping motion
    [Export]
    private float wallKnockBackSpeed;


    ///////////////////// when hitting a wall while in the falling motion of a jump 
    [Export]
    private float wallKnockDownVelocity;

    //Max length of X when hitting a wall while in falling motion
    [Export]
    private float wallKnockDownSpeed;

    [Export]
    private int increaseJumpPowerAmount;


    [Export]
    private float fallFlatValue;

    private float currentFallFlatValue;

    private int fallFlatEffectCount;

    public AnimatedSprite2D animatedSprite;
    private Area2D collisionDetectionArea;
    private CollisionShape2D collisionShape;
    private AudioStreamPlayer2D audioStreamPlayer;

    public Direction FaceDirection { get; set; }
    public CharacterState CurrentCharacterState { get; set; }



    private bool knockback;
    private bool knockDown;
    private bool wallKnockback;
    private bool ceilingHit;
    private bool canMove;
    public bool IsOnDiagonal;

    private bool magicianHit = false;

    public bool CanMove
    {
        get { return canMove; }
        set { canMove = value; }
    }
    //X value to tell us which way we should knockback
    private Vector2 hitDirection;


    private Dictionary<string, AudioStreamWav> playerSoundEffects;

    public override void _Ready()
    {
        base._Ready();
        animatedSprite = this.GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        collisionDetectionArea = this.GetNode<Area2D>("CollisionArea");
        collisionShape = this.GetNode<CollisionShape2D>("CollisionShape2D");
        audioStreamPlayer = this.GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D");



        playerSoundEffects = new Dictionary<string, AudioStreamWav>();

        playerSoundEffects.Add("Jump", GD.Load<AudioStreamWav>("res://Content/SoundEffects/Jump.wav"));
        playerSoundEffects.Add("WallHit", GD.Load<AudioStreamWav>("res://Content/SoundEffects/WallHit.wav"));
        playerSoundEffects.Add("FallFlat", GD.Load<AudioStreamWav>("res://Content/SoundEffects/FallFlat.wav"));

        FaceDirection = Direction.Default;
        CurrentCharacterState = CharacterState.Grounded;

        knockback = false;
        knockDown = false;
        wallKnockback = false;
        canMove = true;
        ceilingHit = false;
        IsOnDiagonal = false;
        hitDirection = Vector2.Zero;
        random = new Random();



        fallFlatEffectCount = 0;
        currentFallFlatValue = 0;
        CurrentJumpVelocity = MinimumJumpVelocity;

    }
    public Vector2 GetGravityValue()
    {
        
        //if we hit a wall
        if (wallKnockback && !knockDown)
        {
            return new Vector2(0, jumpGravity);
        }
        //Gravity if we are hit a wall while in the falling state
        else if (knockDown)
        {
            return new Vector2(0, wallKnockdowGravity);
        }
        //Gravity if we are hit by a mob
        else if(knockback)
        {
            return new Vector2(0, jumpGravity);
        }


        if(this.CurrentCharacterState == CharacterState.Falling)
        {
            return new Vector2(0, fallGravity);
        }
        else
        {
            return new Vector2(0, jumpGravity);
        }
    }

    public override void _PhysicsProcess(double delta)
	{
		    Vector2 velocity = Velocity;

            if (FaceDirection != Direction.Portal)
            {
                // Add the gravity.
                if (!IsOnFloor())
                {
                    velocity += GetGravityValue() * (float)delta;
                    if (velocity.Y > 0)
                    {
                    
                        currentFallFlatValue++;
                        if (currentFallFlatValue >= fallFlatValue)
                        {
                            this.CurrentCharacterState = CharacterState.FallFlat;
                        }
                        else
                        {
                            CurrentCharacterState = CharacterState.Falling;
                        }

                    }
                    else
                    {
                        CurrentCharacterState = CharacterState.Jumping;
                    }

                }
                else
                {
                    //set wallKnockback to false as soon as we hit the ground
                    if (wallKnockback)
                    {
                        wallKnockback = false;
                    }

                    knockDown = false;

                    if(CurrentCharacterState == CharacterState.FallFlat)
                    {
                    }
                    else
                    {
                        CurrentCharacterState = CharacterState.Grounded;

                        if (!IsOnDiagonal)
                        {
                            currentFallFlatValue = 0;
                            fallFlatEffectCount = 0;
                        }
                    }


                }


                if (knockback)
                {
                    if (wallKnockback)
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
                    velocity.X = wallKnockDownSpeed * hitDirection.X;
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

                    if (!wallKnockback && CurrentCharacterState == CharacterState.Grounded)
                    {
                        // Get the input direction and handle the movement/deceleration.
                        // As good practice, you should replace UI actions with custom gameplay actions.
                        Vector2 direction = Input.GetVector("MoveLeft", "MoveRight", "MoveUp", "MoveDown");

                        if(canMove)
                        {
                            if (direction.X < 0)
                            {
                                this.FaceDirection = Direction.Left;
                            }
                            else if (direction.X > 0)
                            {
                                this.FaceDirection = Direction.Right;
                            }
                        }


                        if (direction != Vector2.Zero)
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
                        if (Input.IsActionPressed("Jump"))
                        {

                            animatedSprite.Play("Jump");
                            velocity.X = Mathf.MoveToward(Velocity.X, 0, JumpSpeed);
                            CurrentJumpVelocity -= increaseJumpPowerAmount;
                            CurrentJumpVelocity = Mathf.Clamp(CurrentJumpVelocity, MaximumJumpVelocity, MinimumJumpVelocity);

                            if(direction == Vector2.Zero)
                            {
                                FaceDirection = Direction.Default;
                            }
                        }

    
                        if (Input.IsActionJustReleased("Jump") || CurrentJumpVelocity == MaximumJumpVelocity)
                        {
                            audioStreamPlayer.Stream = playerSoundEffects["Jump"];

                            EmitSignal(SignalName.PlayerJump);

                            velocity.Y = CurrentJumpVelocity;
                            CurrentJumpVelocity = MinimumJumpVelocity;

                            audioStreamPlayer.Play();

                            velocity.X = direction.X * JumpSpeed;

                            if (direction == Vector2.Zero)
                            {
                                FaceDirection = Direction.Default;
                            }
                    }

                    }
                    else
                    {
                        if (CurrentCharacterState == CharacterState.FallFlat)
                        {
                            //Need 
                            //play fallflat animationm
                            if (this.FaceDirection == Direction.Left)
                            {
                                animatedSprite.Play("FallFlatLeft");
                            }
                            else
                            {
                                animatedSprite.Play("FallFlatRight");
                            }


                            Vector2 direction = Input.GetVector("MoveLeft", "MoveRight", "MoveUp", "MoveDown");

                        
                            if (direction != Vector2.Zero) {
                                CurrentCharacterState = CharacterState.Grounded;
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
                    else if (collisionObject is Mob)
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

        //Check if we hit the bottom of a tile
        if(collisionNormal.Y == 1)
        {
            if (FaceDirection == Direction.Left)
            {
                hitDirection = Vector2.Left;
            }
            else if (FaceDirection == Direction.Right)
            {
                hitDirection = Vector2.Right;
            }
            else
            {
                hitDirection = Vector2.Zero;
            }
            ceilingHit = true;
            knockDown = true;
            knockback = false;
            wallKnockback = true;
        }
        else if ((CurrentCharacterState == CharacterState.Jumping || CurrentCharacterState == CharacterState.Falling) && collisionNormal.Y == 0)
        {

            //if we hit the wall from the right
            if (collisionNormal.X > 0)
            {

                hitDirection = Vector2.Right;
            }
            //if we hit the wall from the left
            else if (collisionNormal.X < 0)
            {
                hitDirection = Vector2.Left;
            }


            ceilingHit = false;
            

            if (CurrentCharacterState == CharacterState.Jumping)
            {
                knockback = true;
                knockDown = false;

            }
            else if(CurrentCharacterState == CharacterState.Falling)
            {
                knockback = false;
                knockDown = true;
            }

            wallKnockback = true;


        }
        if (collisionNormal.Y == 0 && (CurrentCharacterState != CharacterState.Grounded && CurrentCharacterState != CharacterState.FallFlat))
        {
            audioStreamPlayer.Stream = playerSoundEffects["WallHit"];
            audioStreamPlayer.Play();
        }

        else if(collisionNormal.Y == -1 && (CurrentCharacterState != CharacterState.Grounded))
        {
            if(CurrentCharacterState == CharacterState.FallFlat)
            {
                audioStreamPlayer.Stream = playerSoundEffects["FallFlat"];
                //Need to only play it once
                if(fallFlatEffectCount < 3)
                {
                    audioStreamPlayer.Play();
                    fallFlatEffectCount++;
                }

            }
        }
        else if (collisionNormal.Y == 1 && (CurrentCharacterState != CharacterState.Grounded))
        {
            audioStreamPlayer.Stream = playerSoundEffects["WallHit"];
            audioStreamPlayer.Play();
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
            magicianHit = true;
        }
        else
        {
            CurrentCharacterState = CharacterState.Grounded;
            audioStreamPlayer.Stream = playerSoundEffects["WallHit"];
            audioStreamPlayer.Play();
            knockback = true;
        }

    }

    public void _on_audio_stream_player_2d_finished()
    {

    }
    public void _on_animated_sprite_2d_animation_finished()
    {
        if (FaceDirection == Direction.Portal)
        {
            FaceDirection = Direction.Default;
            collisionDetectionArea.SetDeferred("monitoring", true);
            collisionShape.SetDeferred("disabled", false);

            if (magicianHit)
            {
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
                        Vector2 newPosition = new Vector2(teleportedPosition.X * PositionCalculator.TileSize, teleportedPosition.Y * PositionCalculator.TileSize);

                        this.GlobalPosition = newPosition;
                        break;
                    }

                }
            }
            magicianHit = false;





            //if Position has a tile then Y+5 and so on
        }
    }





}
