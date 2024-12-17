using Godot;
using Godot.Collections;
using Platformer.Source.Util;
using System.Collections.Generic;

public partial class LevelBase : Node2D
{
	public TileMapLayer collisionTileLayer;

	private Player player;


	private AnimatedSprite2D playerJumpEffect;

	public override void _Ready()
	{
		collisionTileLayer = this.GetNode<TileMapLayer>("TileMapLayer");
		player = this.GetNode<Player>("Player");

        PackedScene playerJumpEffectScene = GD.Load<PackedScene>("res://Source/Player/PlayerJumpEffect.tscn");
        playerJumpEffect = playerJumpEffectScene.Instantiate() as AnimatedSprite2D;

        playerJumpEffect.AnimationFinished += OnAnimationFinishedEvent;
    }

    public List<Vector2I> GetSurroundingCellsInTileSizeCoOrds(Vector2I position)
	{
		List<Vector2I> cellsInTileSizeCoOrds = new List<Vector2I>();
		foreach(Vector2I positionInTileSizeCoOrds in collisionTileLayer.GetUsedCells())
		{
            cellsInTileSizeCoOrds.Add(new Vector2I(positionInTileSizeCoOrds.X , positionInTileSizeCoOrds.Y ));

        }

        return cellsInTileSizeCoOrds;

    }
	

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

	}

	public void _on_player_player_jump()
	{
		this.AddChild(playerJumpEffect);
        playerJumpEffect.GlobalPosition = new Vector2(player.GlobalPosition.X, player.GlobalPosition.Y + 3);

        playerJumpEffect.Play("JumpEffect");

    }

	private void OnAnimationFinishedEvent()
	{
		this.RemoveChild(playerJumpEffect);
    }

	
}
