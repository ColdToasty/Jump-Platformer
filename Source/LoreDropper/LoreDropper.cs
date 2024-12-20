using Godot;
using System;

public partial class LoreDropper : CharacterBody2D
{
	public const float Speed = 300.0f;
	public const float JumpVelocity = -400.0f;

	private RichTextLabel label;

	private AnimatedSprite2D animatedSprite;

	private Area2D playerDetectionArea;

    private bool showLabel;

    private Timer timer;

    [Export]
    private string labelText;

    public override void _Ready()
    {
        base._Ready();

		label = this.GetNode<RichTextLabel>("Container/RichTextLabel");

        animatedSprite = this.GetNode<AnimatedSprite2D>("AnimatedSprite2D");

        playerDetectionArea = this.GetNode<Area2D>("Area2D");


        timer = this.GetNode<Timer>("Timer");


        label.Text = labelText;

        label.Visible = false;
        showLabel = false;
    }

    public override void _PhysicsProcess(double delta)
	{
        if (showLabel)
        {
            label.VisibleCharacters++;
            if(label.VisibleRatio == 1)
            {
                if (showLabel == true){
                    timer.Start(5);
                }
            }
        }
	}

	public virtual void _on_player_entered(Player player)
	{
        label.Visible = true;
        showLabel = true;

    }

    public virtual void _on_player_exited(Player player)
    {
        label.Visible = false;
        showLabel = false;
        label.VisibleCharacters = 0;
        timer.Stop();
    }

    public void _on_timer_timeout()
    {
        label.Visible = false;
        showLabel = false;
        label.VisibleCharacters = 0;
    }


}
