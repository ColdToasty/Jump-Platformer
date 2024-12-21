using Godot;
using System;

public partial class Platform : CharacterBody2D
{

    public const float MoveSpeed = 100.0f;
    protected enum PlatformMovementType { Default, Moving }

    [Export]
    protected PlatformMovementType PlatformMovement;

    [Export]
	private bool isBreakable;

	protected bool moveLeft;


    private Timer timer;
    private AnimatedSprite2D animatedSprite;
    private CollisionShape2D collisionShape;
    private Area2D playerDetectionArea;


    public override void _Ready()
    {
        base._Ready();
		animatedSprite = this.GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        collisionShape = this.GetNode<CollisionShape2D>("CollisionShape2D");
        timer = this.GetNode<Timer>("Timer");
        playerDetectionArea = this.GetNode<Area2D>("PlayerDetection");
        moveLeft = false;

    }

    public override void _PhysicsProcess(double delta)
	{
		if(this.PlatformMovement == PlatformMovementType.Moving)
		{

            Vector2 velocity = Velocity;
            if (moveLeft)
            {
                velocity.X = Vector2.Left.X * MoveSpeed;
            }
            else
            {
                velocity.X = Vector2.Right.X * MoveSpeed;
            }

            velocity.Y = 0;
            Velocity = velocity;
            bool isColliding = MoveAndSlide();
			if (isColliding)
			{
				moveLeft = !moveLeft;
			}
        }
	}

	public virtual void _on_animated_sprite_2d_animation_finished()
	{
		this.Visible = false;
		this.timer.Start(3);
        collisionShape.SetDeferred("disabled", true);
        playerDetectionArea.SetDeferred("monitoring", false);
    }

	public virtual void _on_player_detection_body_entered(Player player)
	{
		if (this.isBreakable)
		{
			animatedSprite.Play("Break");
		}
	}
    public virtual void _on_player_detection_body_exited(Player player)
    {
        //this.animatedSprite.Play("Default");
    }


    public virtual void _on_timer_timeout()
	{
        this.Visible = true;
		this.animatedSprite.Play("Default");
		collisionShape.SetDeferred("disabled", false);
        playerDetectionArea.SetDeferred("monitoring", true);

    }

}
