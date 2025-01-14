using Godot;
using Platformer.Source.Util;
using Platformer.Source.Util.JsonContainers;
using System;
using System.Collections.Generic;

public partial class LoreDropper : CharacterBody2D
{
	public const float Speed = 300.0f;
	public const float JumpVelocity = -400.0f;

	private RichTextLabel label;

	private AnimatedSprite2D animatedSprite;

	private Area2D playerDetectionArea;

    private bool showLabel;
    private bool pauseStartLabel;

    private Timer timer;

    public enum LoreDropperName {JerryScroll, JerryNotebook, Jerry, Janitor}

    [Export]
    private LoreDropperName LoreDropperType;

    
    private int currentDialogIndex = 0;

    private Dictionary<string, LoreDropperDialog> loreDropperDialog;

    private string currentDialogID;
   

    public override void _Ready()
    {
        base._Ready();

		label = this.GetNode<RichTextLabel>("Container/RichTextLabel");

        animatedSprite = this.GetNode<AnimatedSprite2D>("AnimatedSprite2D");

        playerDetectionArea = this.GetNode<Area2D>("Area2D");


        timer = this.GetNode<Timer>("Timer");


        label.Visible = false;
        showLabel = false;
        pauseStartLabel = false; 

        LoreDropperDialogLoader.LoadLoreDropperDialogs();
        loreDropperDialog = LoreDropperDialogLoader.NameDialogs[LoreDropperType];

        currentDialogID = loreDropperDialog[LoreDropperType.ToString() + "_" + currentDialogIndex.ToString("D2")].ID;

        label.Text = loreDropperDialog[currentDialogID].Dialog;
        switch (LoreDropperType)
        {
            case LoreDropperName.Jerry:

                //get jerry dialogs from level
                animatedSprite.Play("Jerry");

                break;

            case LoreDropperName.Janitor:
                //get janitor dialogs from level
                animatedSprite.Play("Janitor");
                break;

        }
    }

    private void UpdateDialog()
    {
        currentDialogIndex++;
        SetCurrentDialogIndex();

    }

    private void ResetDialog()
    {
        currentDialogIndex = 0;
        SetCurrentDialogIndex();
    }

    private void SetCurrentDialogIndex()
    {
        currentDialogID = loreDropperDialog[LoreDropperType.ToString() + "_" + currentDialogIndex.ToString("D2")].ID;
        label.Text = loreDropperDialog[currentDialogID].Dialog;
        label.VisibleRatio = -1;
       
    }


    public override void _PhysicsProcess(double delta)
	{
        if (showLabel)
        {
            label.VisibleCharacters++;
            if(label.VisibleRatio == 1)
            {
                if (loreDropperDialog[currentDialogID].NextDialogID == null)
                {
                    timer.Start(6);
                    pauseStartLabel = true;
                }
                else
                {
                    switch (loreDropperDialog[currentDialogID].PlaySpeed)
                    {
                        case LoreDropperDialog.DialogPlaySpeed.Fast:
                            timer.Start(2);
                            break;

                        case LoreDropperDialog.DialogPlaySpeed.Slow:
                            timer.Start(4);
                            break;

                        default:
                            timer.Start(3);
                            break;
                    }
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
        timer.Stop();
        pauseStartLabel = false;
        ResetDialog();
    }

    public void _on_timer_timeout()
    {
        if (pauseStartLabel)
        {
            pauseStartLabel = false;
        }
        else
        {
            if (loreDropperDialog[currentDialogID].NextDialogID != null)
            {
                UpdateDialog();
            }
            else
            {
                ResetDialog();
            }
        }


    }


}
