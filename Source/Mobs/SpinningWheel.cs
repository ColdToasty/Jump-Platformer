using Godot;
using System;
using System.Data;

public partial class SpinningWheel : Mob
{

	public override void _PhysicsProcess(double delta)
	{
        if(mobState == MobState.Move)
        {
            if (this.MoveLeft)
            {
                animatedSprite.Play("MoveLeft");
            }
            else
            {
                animatedSprite.Play("MoveRight");
            }

        }

    }

    public override void _on_player_entered(Player player)
    {
        if (moveLeft)
        {
            animatedSprite.Play("AttackLeft");
        }
        else
        {
            animatedSprite.Play("AttackRight");

        }
        this.mobState = MobState.Attack;
        attackRange.SetDeferred("monitoring", false);
    }

    public override void _on_animated_sprite_2d_animation_finished()
    {
        this.mobState = MobState.Move;
        attackRange.SetDeferred("monitoring", true);
    }
}
