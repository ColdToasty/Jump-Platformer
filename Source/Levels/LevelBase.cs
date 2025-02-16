using Godot;
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

    public bool PlayerWon { get; private set; }


    private Godot.Collections.Array<Node> loreDroppers;

    private LoreDropper janitor;

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
        PlayerWon = false;



        Node2D loreDroppersList = this.GetNode<Node2D>("LoreDroppers");

        loreDroppers = loreDroppersList.GetChildren();

        foreach (Node2D node in loreDroppers)
        {
            LoreDropper loreDropper = (LoreDropper)node;

            if(loreDropper.LoreDropperType == LoreDropper.LoreDropperName.Janitor)
            {
                janitor = loreDropper;
            }
        }

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

        Vector2I RightTileNextToPlayerPosition = new Vector2I(PlayerPosition.X + 1, PlayerPosition.Y);
        Vector2I LeftTileNextToPlayerPosition = new Vector2I(PlayerPosition.X - 1, PlayerPosition.Y);

        Vector2I LeftTileUnderPlayerPosition = new Vector2I(PlayerPosition.X - 1, PlayerPosition.Y + 1);
        Vector2I RightTileUnderPlayerPosition = new Vector2I(PlayerPosition.X + 1, PlayerPosition.Y + 1);

        //Check to see if player is on a sliding tile and not a diagonal tile
        if (slidingTiles.Contains(TileUnderPlayerPosition) && !diagonalTiles.Contains(TileUnderPlayerPosition))
		{
            if (player.FaceDirection == Player.Direction.Left)
            {
                if (tilesUsed.Contains(LeftTileNextToPlayerPosition))
                {
                    //check if there is a tile on the left of the player
                    if ((bool)collisionTileLayer.GetCellTileData(LeftTileNextToPlayerPosition).GetCustomData("Slide") == true && collisionTileLayer.GetCellTileData(TileUnderPlayerPosition).GetConstantLinearVelocity(0) != Vector2.Zero)
                    {
                        collisionTileLayer.GetCellTileData(TileUnderPlayerPosition).SetConstantLinearVelocity(0, Vector2.Zero);
                        GD.Print("No Slide Left");
                    }
                }
                else if (collisionTileLayer.GetCellTileData(TileUnderPlayerPosition).GetConstantLinearVelocity(0) != -slideVelocity)
                {
                    collisionTileLayer.GetCellTileData(TileUnderPlayerPosition).SetConstantLinearVelocity(0, -slideVelocity);
                    GD.Print("Slide Left");
                }

                if(tilesUsed.Contains(LeftTileUnderPlayerPosition) && !tilesUsed.Contains(LeftTileNextToPlayerPosition))
                {
                    if ((bool)collisionTileLayer.GetCellTileData(LeftTileUnderPlayerPosition).GetCustomData("Slide") == true && collisionTileLayer.GetCellTileData(LeftTileUnderPlayerPosition).GetConstantLinearVelocity(0) != -slideVelocity)
                    {
                        collisionTileLayer.GetCellTileData(LeftTileUnderPlayerPosition).SetConstantLinearVelocity(0, -slideVelocity);
                        GD.Print("No Slide Left");
                    }
                }

            }
            else if (player.FaceDirection == Player.Direction.Right)
            {
                if (tilesUsed.Contains(RightTileNextToPlayerPosition))
                {
                    if ((bool)collisionTileLayer.GetCellTileData(RightTileNextToPlayerPosition).GetCustomData("Slide") == true && collisionTileLayer.GetCellTileData(TileUnderPlayerPosition).GetConstantLinearVelocity(0) != Vector2.Zero)
                    {
                        collisionTileLayer.GetCellTileData(TileUnderPlayerPosition).SetConstantLinearVelocity(0, Vector2.Zero);
                        GD.Print("No Slide right");
                    }
                }

                else if (collisionTileLayer.GetCellTileData(TileUnderPlayerPosition).GetConstantLinearVelocity(0) != slideVelocity)
                {
                    collisionTileLayer.GetCellTileData(TileUnderPlayerPosition).SetConstantLinearVelocity(0, slideVelocity);
                    GD.Print("Slide Right");
                }

                if (tilesUsed.Contains(RightTileUnderPlayerPosition) && !tilesUsed.Contains(RightTileNextToPlayerPosition))
                {
                    if ((bool)collisionTileLayer.GetCellTileData(RightTileUnderPlayerPosition).GetCustomData("Slide") == true && collisionTileLayer.GetCellTileData(RightTileUnderPlayerPosition).GetConstantLinearVelocity(0) != slideVelocity)
                    {
                        collisionTileLayer.GetCellTileData(RightTileUnderPlayerPosition).SetConstantLinearVelocity(0, slideVelocity);
                        GD.Print("No Slide Left");
                    }
                }
            }
		}

        //On diagonal slidey tile
        if (slidingTiles.Contains(TileUnderPlayerPosition) && diagonalTiles.Contains(TileUnderPlayerPosition))
        {
            player.CanMove = false;
            player.IsOnDiagonal = true;
        }
        else
        {
            player.IsOnDiagonal = false;
            player.CanMove = true;
 
        }




        if(player.CurrentCharacterState == Player.CharacterState.FallFlat)
        {
            if (tilesUsed.Contains(TileUnderPlayerPosition))
            {
                player.Velocity = new Vector2(0, player.Velocity.Y);
                
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

	public void _on_socks_claimed()
    {
        PlayerWon = true;
        janitor.PlayerWon = true;
        janitor.ResetDialog();
        player.FaceDirection = Player.Direction.Portal;
        player.animatedSprite.Play("Portal");
        player.GlobalPosition = new Vector2(-384, 472);

    }
}
