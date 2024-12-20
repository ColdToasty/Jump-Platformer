using Godot;
using Godot.Collections;
using Platformer.Source.Util;
using System.Collections.Generic;
using System.Linq;

public partial class LevelBase : Node2D
{
	public TileMapLayer collisionTileLayer;

	private Player player;
	private AnimatedSprite2D playerJumpEffect;

	private HashSet<Vector2I> tilesUsed;
	private HashSet<Vector2I> slidingTiles;
    private HashSet<Vector2I> quickSandTiles;
    private HashSet<Vector2I> slowTiles;
    private HashSet<Vector2I> diagonalTiles;


    [Export]
	private Vector2 slideVelocity;

    [Export]
    private Vector2 diagonalSlideVelocityAdditive;


	private Vector2 diagonalSlideVelocity;
    public override void _Ready()
	{
		collisionTileLayer = this.GetNode<TileMapLayer>("TileMapLayer");
		player = this.GetNode<Player>("Player");

        PackedScene playerJumpEffectScene = GD.Load<PackedScene>("res://Source/Player/PlayerJumpEffect.tscn");
        playerJumpEffect = playerJumpEffectScene.Instantiate() as AnimatedSprite2D;

        playerJumpEffect.AnimationFinished += OnAnimationFinishedEvent;

		tilesUsed = GetSurroundingCellsInTileSizeCoOrds().ToHashSet<Vector2I>();


        diagonalSlideVelocity = slideVelocity + diagonalSlideVelocityAdditive;

        slidingTiles = new HashSet<Vector2I>();
        quickSandTiles = new HashSet<Vector2I>();
        slowTiles = new HashSet<Vector2I>();
        diagonalTiles = new HashSet<Vector2I>();

        foreach (Vector2I tile in tilesUsed)
		{
			TileData tileData = collisionTileLayer.GetCellTileData(tile);
			bool isSlide = (bool)tileData.GetCustomData("Slide");
            bool isQuickSand = (bool)tileData.GetCustomData("QuickSand");
            bool isSlow = (bool)tileData.GetCustomData("Slow");
			bool isDiagonal = (bool)tileData.GetCustomData("Diagonal");
            if (isSlide)
			{
				slidingTiles.Add(tile);
            }
			if (isQuickSand)
			{
                quickSandTiles.Add(tile);
            }

			if (isSlow)
			{
				slowTiles.Add(tile);
			}

			if (isDiagonal)
			{
				diagonalTiles.Add(tile);

                if ((bool)tileData.GetCustomData("LeftDiagonal"))
				{
                    tileData.SetConstantLinearVelocity(0, -diagonalSlideVelocity);
                }
				else
				{
                    tileData.SetConstantLinearVelocity(0, diagonalSlideVelocity);
                }
			}
        }
    }

    public List<Vector2I> GetSurroundingCellsInTileSizeCoOrds()
	{
		List<Vector2I> cellsInTileSizeCoOrds = new List<Vector2I>();
		foreach(Vector2I positionInTileSizeCoOrds in collisionTileLayer.GetUsedCells())
		{
            cellsInTileSizeCoOrds.Add(new Vector2I(positionInTileSizeCoOrds.X , positionInTileSizeCoOrds.Y ));

        }
        return cellsInTileSizeCoOrds;
    }

	

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		Vector2I PlayerPosition = (Vector2I)PositionCalculator.GetPosition(player.GlobalPosition).Item1;
		Vector2I TileUnderPlayerPosition = new Vector2I(PlayerPosition.X, PlayerPosition.Y + 1);

		//Check to see if player is on a sliding tile
        if (slidingTiles.Contains(TileUnderPlayerPosition) && !diagonalTiles.Contains(TileUnderPlayerPosition))
		{
			if(player.FaceDirection == Player.Direction.Left)
			{
                collisionTileLayer.GetCellTileData(TileUnderPlayerPosition).SetConstantLinearVelocity(0, -slideVelocity);
            }
            else if (player.FaceDirection == Player.Direction.Right)
            {
                collisionTileLayer.GetCellTileData(TileUnderPlayerPosition).SetConstantLinearVelocity(0, slideVelocity);
            }
		}

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
