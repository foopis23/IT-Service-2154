using UnityEngine;
using CallbackEvents;
using System.Linq;

public class UpdateHUDContext : EventContext
{
	public float rotation;

	public UpdateHUDContext(float rotation)
	{
		this.rotation = rotation;
	}
}

public class ShowInteractionDialogueContext : EventContext
{
    public string InteractionText;

    public ShowInteractionDialogueContext(string InteractionText) {
        this.InteractionText = InteractionText;
    }
}

public class HideInteractionDialogueContext : EventContext
{

}

enum CompassDirection
{
	NORTH,
	NORTHEAST,
	EAST,
	SOUTHEAST,
	SOUTH,
	SOUTHWEST,
	WEST,
	NORTHWEST
}


public class HealthOverlay : MonoBehaviour
{
    // health bar
	private int healthX = 1;
	private int healthY = 1;
	private int healthWidth = 20;
	private int healthHeight = 4;

    // compass
	private int compassX;
	private int compassY;

    // dialogue box
    private int dialogueX;
    private int dialogueY;
    private int dialogueWidth;
    private int dialogueHeight;
    private bool isDialogueVisible;

	private CompassDirection lastDirection;

	private float lastHealth;

	// Start is called before the first frame update
	void Start()
	{
		EventSystem.Current.RegisterEventListener<PlayerHealthChangedCtx>(OnHealthChange);
		EventSystem.Current.RegisterEventListener<UpdateHUDContext>(OnUpdateHUD);
        EventSystem.Current.RegisterEventListener<ShowInteractionDialogueContext>(OnShowDialogue);
        EventSystem.Current.RegisterEventListener<HideInteractionDialogueContext>(OnHideDialogue);

		drawHealth(1.0f);
		drawCross();
	}

	void drawHealth(float percent)
	{
		int count = (int)(percent * (healthWidth - 2));

		ASCII.Instance.ClearArea(healthX, healthY, healthWidth, healthHeight);
		ASCII.Instance.DrawBox('#', Color.white, Color.black, healthX, healthY, healthWidth, healthHeight);
		string filledHealth = string.Concat(Enumerable.Repeat("#", count));
		for (int y = this.healthY + 1; y < healthHeight; y++)
		{
			ASCII.Instance.PutString(filledHealth, Color.red, this.healthX + 1, y);
		}
	}

	void drawCross()
	{
		int centX = (int)(((float)ASCII.Instance.columns) / 2.0f);
		int centY = (int)(((float)ASCII.Instance.rows) / 2.0f);
		ASCII.Instance.PutChar('|', Color.white, centX, centY - 1);
		ASCII.Instance.PutChar('-', Color.white, centX - 2, centY);
		ASCII.Instance.PutChar('-', Color.white, centX + 2, centY);
		ASCII.Instance.PutChar('+', Color.white, centX, centY);
		ASCII.Instance.PutChar('|', Color.white, centX, centY + 1);
	}

	void drawCompass(float angle)
	{
		compassX = (int)ASCII.Instance.columns - 5;
		compassY = 2;

		CompassDirection direction = CompassDirection.SOUTH;

		if (angle > -22.5 && angle <= 22.5)
		{ //north
			direction = CompassDirection.NORTH;
			if (direction != lastDirection)
			{
				ASCII.Instance.ClearArea(compassX, compassY, 3, 3);
				ASCII.Instance.DrawBox('#', Color.white, Color.black, compassX - 1, compassY - 1, 5, 5);
				ASCII.Instance.PutChar('|', Color.red, compassX + 1, compassY);
				ASCII.Instance.PutChar('0', Color.white, compassX + 1, compassY + 1);
				ASCII.Instance.PutChar('|', Color.white, compassX + 1, compassY + 2);
			}
		}
		else if (angle > 22.5 && angle <= 67.5)
		{// northeast
			direction = CompassDirection.NORTHEAST;
			if (direction != lastDirection)
			{
				ASCII.Instance.ClearArea(compassX, compassY, 3, 3);
				ASCII.Instance.DrawBox('#', Color.white, Color.black, compassX - 1, compassY - 1, 5, 5);
				ASCII.Instance.PutChar('\\', Color.red, compassX, compassY);
				ASCII.Instance.PutChar('0', Color.white, compassX + 1, compassY + 1);
				ASCII.Instance.PutChar('\\', Color.white, compassX + 2, compassY + 2);
			}
		}
		else if (angle > 67.5 && angle <= 112.5)
		{ // east
			direction = CompassDirection.EAST;
			if (direction != lastDirection)
			{
				ASCII.Instance.ClearArea(compassX, compassY, 3, 3);
				ASCII.Instance.DrawBox('#', Color.white, Color.black, compassX - 1, compassY - 1, 5, 5);
				ASCII.Instance.PutChar('-', Color.red, compassX, compassY + 1);
				ASCII.Instance.PutChar('0', Color.white, compassX + 1, compassY + 1);
				ASCII.Instance.PutChar('-', Color.white, compassX + 2, compassY + 1);
			}
		}
		else if (angle > 112.5 && angle <= 157.5)
		{ // southeast
			direction = CompassDirection.SOUTHEAST;
			if (direction != lastDirection)
			{
				ASCII.Instance.ClearArea(compassX, compassY, 3, 3);
				ASCII.Instance.DrawBox('#', Color.white, Color.black, compassX - 1, compassY - 1, 5, 5);
				ASCII.Instance.PutChar('/', Color.white, compassX + 2, compassY);
				ASCII.Instance.PutChar('0', Color.white, compassX + 1, compassY + 1);
				ASCII.Instance.PutChar('/', Color.red, compassX, compassY + 2);
			}
		}
		else if (angle > 157.5 || angle < -157.5)
		{ //south
			direction = CompassDirection.SOUTH;
			if (direction != lastDirection)
			{
				ASCII.Instance.ClearArea(compassX, compassY, 3, 3);
				ASCII.Instance.DrawBox('#', Color.white, Color.black, compassX - 1, compassY - 1, 5, 5);
				ASCII.Instance.PutChar('|', Color.white, compassX + 1, compassY);
				ASCII.Instance.PutChar('0', Color.white, compassX + 1, compassY + 1);
				ASCII.Instance.PutChar('|', Color.red, compassX + 1, compassY + 2);
			}
		}
		else if (angle > -67.5 && angle <= -22.5)
		{// northwest
			direction = CompassDirection.NORTHWEST;
			if (direction != lastDirection)
			{
				ASCII.Instance.ClearArea(compassX, compassY, 3, 3);
				ASCII.Instance.DrawBox('#', Color.white, Color.black, compassX - 1, compassY - 1, 5, 5);
				ASCII.Instance.PutChar('/', Color.red, compassX + 2, compassY);
				ASCII.Instance.PutChar('0', Color.white, compassX + 1, compassY + 1);
				ASCII.Instance.PutChar('/', Color.white, compassX, compassY + 2);
			}
		}
		else if (angle > -112.5 && angle < -67.5)
		{ //west 
			direction = CompassDirection.WEST;
			if (direction != lastDirection)
			{
				ASCII.Instance.ClearArea(compassX, compassY, 3, 3);
				ASCII.Instance.DrawBox('#', Color.white, Color.black, compassX - 1, compassY - 1, 5, 5);
				ASCII.Instance.PutChar('-', Color.white, compassX, compassY + 1);
				ASCII.Instance.PutChar('0', Color.white, compassX + 1, compassY + 1);
				ASCII.Instance.PutChar('-', Color.red, compassX + 2, compassY + 1);
			}
		}
		else if (angle > -157.5 && angle < -112.5)
		{//southwest dressing
			direction = CompassDirection.SOUTHWEST;
			if (direction != lastDirection)
			{
				ASCII.Instance.ClearArea(compassX, compassY, 3, 3);
				ASCII.Instance.DrawBox('#', Color.white, Color.black, compassX - 1, compassY - 1, 5, 5);
				ASCII.Instance.PutChar('\\', Color.white, compassX, compassY);
				ASCII.Instance.PutChar('0', Color.white, compassX + 1, compassY + 1);
				ASCII.Instance.PutChar('\\', Color.red, compassX + 2, compassY + 2);
			}
		}

		lastDirection = direction;
	}

	void OnHealthChange(PlayerHealthChangedCtx ctx)
	{
		if (ctx.currentHealth >= 0 && ctx.currentHealth != lastHealth)
		{
			lastHealth = ctx.currentHealth;
			float percent = ctx.currentHealth / ctx.player.maxHealth;
			drawHealth(percent);
		}
	}

	void OnUpdateHUD(UpdateHUDContext ctx)
	{
		drawCompass(ctx.rotation);
	}

    void OnShowDialogue(ShowInteractionDialogueContext ctx) {
	    if (isDialogueVisible) return;

        dialogueWidth = ctx.InteractionText.Length + 4;
        dialogueHeight = 5;
        dialogueX = (int) ((ASCII.Instance.columns - dialogueWidth) / 2);
        dialogueY = (int) ((ASCII.Instance.rows/2) + (((ASCII.Instance.rows/2) - dialogueHeight) / 4));

        ASCII.Instance.DrawBox('#', Color.white, Color.black, dialogueX, dialogueY, dialogueWidth, dialogueHeight);
        ASCII.Instance.PutString(ctx.InteractionText, Color.white, dialogueX + 2, dialogueY + 2);
        isDialogueVisible = true;
    }

    void OnHideDialogue(HideInteractionDialogueContext ctx) {
        if (!isDialogueVisible) return;

        ASCII.Instance.ClearArea(dialogueX, dialogueY, dialogueWidth, dialogueHeight);
        isDialogueVisible = false;
    }

	public void RedrawUI()
	{
		ASCII.Instance.ClearOverlay();
		drawHealth(1.0f);
        drawCross();
	}
}
