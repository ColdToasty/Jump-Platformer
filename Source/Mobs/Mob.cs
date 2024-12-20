using Godot;
using Godot.Collections;
using Platformer.Source.Util;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Mob : CharacterBody2D
{
    [Export]
    public float Speed = 50.0f;

    [Export]
	protected bool moveLeft;

    public bool MoveLeft { get { return moveLeft; } }

	private Random random;

	protected AnimatedSprite2D animatedSprite;

    protected enum MobState { Move, Attack}

    protected MobState mobState;

    protected Area2D attackRange;
    public enum TypeOfMob {Croc, Magician, Spinny}

    [Export]
    public float KnockbackPower = 100; 

    [Export]
    public TypeOfMob MobType { get; protected set; }

    public override void _Ready()
    {
        base._Ready();

		random = new Random();
		int moveLeftValue = random.Next(2);
        moveLeftValue = 0;


        animatedSprite = this.GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        attackRange = this.GetNode<Area2D>("AttackRange");


    }

    public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

        // Add the gravity.
        if (!IsOnFloor())
        {
            velocity += GetGravity() * (float)delta;

        }

		if(mobState == MobState.Move)
		{
            int direction = 0;
            if (moveLeft)
            {
                direction = -1;
                animatedSprite.Play("MoveLeft");

            }
            else
            {
                direction = 1;
                animatedSprite.Play("MoveRight");

            }

            velocity.X = direction * Speed;

            Velocity = velocity;
            bool isColliding = MoveAndSlide();


            Vector2I position = (Vector2I)PositionCalculator.GetPosition(this.GlobalPosition).Item1;

            HashSet<Vector2I> surroundingCells = this.GetParent<LevelBase>().GetSurroundingCellsInTileSizeCoOrds().ToHashSet<Vector2I>();



            if (moveLeft)
            {
                //Checks to see if there is an empty tile to its left
                Vector2I left = new Vector2I(position.X - 1, position.Y);

                if (surroundingCells.Contains(left))
                {
                    moveLeft = false;
                }
                //Checks to see if there is an empty tile to its left bottom
                Vector2I leftBottom = new Vector2I(position.X - 1, position.Y + 1);

                if (!surroundingCells.Contains(left) && !surroundingCells.Contains(leftBottom))
                {
                    moveLeft = false;
                }
            }
            else
            {
                //Checks to see if there is an empty tile to its right
                Vector2I right = new Vector2I(position.X + 1, position.Y);
                //Checks to see if there is an empty tile to its right bottom
                Vector2I rightBottom = new Vector2I(position.X + 1, position.Y + 1);
                if (surroundingCells.Contains(right))
                {
                    moveLeft = true;
                }

                if (!surroundingCells.Contains(right) && !surroundingCells.Contains(rightBottom))
                {
                    moveLeft = true;
                }
            }
        }
        
    }

    public virtual void _on_player_entered(Player player)
    {
        //Gets the direction of the mob from player position
        Vector2 normal = (this.GlobalPosition - player.GlobalPosition).Normalized().Round();


        if (normal.X == 1)
        {
            animatedSprite.Play("AttackLeft");
            mobState = MobState.Attack;
        }
        else
        {
            animatedSprite.Play("AttackRight");
            mobState = MobState.Attack;
        }
        attackRange.SetDeferred("monitoring", false);
    }

    public virtual void _on_animated_sprite_2d_animation_finished()
    {
        mobState = MobState.Move;
        attackRange.SetDeferred("monitoring", true);
    }



}
