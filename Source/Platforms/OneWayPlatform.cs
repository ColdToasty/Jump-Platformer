using Godot;
using System;

public partial class OneWayPlatform : StaticBody2D
{

	private enum PlatformType { Default, Left, Right}

	[Export]
	private PlatformType Platform;

	private Sprite2D Sprite;
	public override void _Ready()
	{
        Sprite = this.GetNode<Sprite2D>("Sprite2D");

        switch (Platform)
		{
			case PlatformType.Left:
				Sprite.Frame = 1;
                break;
			case PlatformType.Right:
                Sprite.Frame = 2;
                break;
			default:
                Sprite.Frame = 0;
                break;
		}
	}

}
