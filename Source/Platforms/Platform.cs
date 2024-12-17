using Godot;
using System;

public partial class Platform : AnimatableBody2D
{

	private Timer timer;
	private AnimatedSprite2D animatedSprite;

	[Export]
	private bool isBreakable;


    public override void _Ready()
    {
        base._Ready();
		animatedSprite = this.GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        timer = this.GetNode<Timer>("Timer");
    }

    public override void _PhysicsProcess(double delta)
	{


	}
}
