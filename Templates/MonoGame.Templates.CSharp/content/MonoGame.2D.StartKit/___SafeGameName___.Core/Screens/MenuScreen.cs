using ___SafeGameName___.Core;
using GameStateManagement.Inputs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;

namespace ___SafeGameName___.Screens;

/// <summary>
/// Base class for screens that contain a menu of options. The user can
/// move up and down to select an entry, or cancel to back out of the screen.
/// </summary>
abstract class MenuScreen : GameScreen
{
    List<MenuEntry> menuEntries = new List<MenuEntry>();
    int selectedEntry = 0;
    string menuTitle;
    public string Title { get => menuTitle; set => menuTitle = value; }
    Color menuTitleColor = new Color(0, 0, 0); // default color is black. use new Color(192, 192, 192) for off white

    /// <summary>
    /// Gets the list of menu entries, so derived classes can add
    /// or change the menu contents.
    /// </summary>
    protected IList<MenuEntry> MenuEntries
    {
        get { return menuEntries; }
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    public MenuScreen(string menuTitle)
    {
        this.menuTitle = menuTitle;

        TransitionOnTime = TimeSpan.FromSeconds(0.5);
        TransitionOffTime = TimeSpan.FromSeconds(0.5);
    }

    /// <summary>
    /// LoadContent will be called once per game and is the place to load
    /// all of your content for the game and ScaleOurPresentationArea befor we start.
    /// </summary>
    public override void LoadContent()
    {
        base.LoadContent();
    }

    /// <summary>
    /// Responds to user input, changing the selected entry and accepting
    /// or cancelling the menu.
    /// </summary>
    public override void HandleInput(GameTime gameTime, InputState inputState)
    {
        base.HandleInput(gameTime, inputState);

        if (___SafeGameName___Game.IsMobile)
        {
            var touchState = inputState.CurrentTouchState;
            if (touchState.Count > 0)
            {
                foreach (var touch in touchState)
                {
                    if (touch.State == TouchLocationState.Pressed)
                    {
                        TextSelectedCheck(inputState.CurrentCursorLocation);
                    }
                }
            }
        }
        else if (___SafeGameName___Game.IsDesktop)
        {
            var mouseState = inputState.CurrentMouseState;
            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                TextSelectedCheck(inputState.CurrentCursorLocation);
            }
            else if (mouseState.MiddleButton == ButtonState.Pressed)
            {
                OnSelectEntry(selectedEntry, PlayerIndex.One);
            }
        }

        // Move to the previous menu entry?
        if (inputState.IsMenuUp(ControllingPlayer))
        {
            selectedEntry--;

            if (selectedEntry < 0)
                selectedEntry = menuEntries.Count - 1;

            while (!menuEntries[selectedEntry].Enabled)
            {
                selectedEntry--;

                if (selectedEntry < 0)
                    selectedEntry = menuEntries.Count - 1;
            }
        }

        // Move to the next menu entry?
        if (inputState.IsMenuDown(ControllingPlayer))
        {
            selectedEntry++;

            if (selectedEntry >= menuEntries.Count)
                selectedEntry = 0;

            SetNextEnabledMenu();
        }

        // Accept or cancel the menu? We pass in our ControllingPlayer, which may
        // either be null (to accept input from any player) or a specific index.
        // If we pass a null controlling player, the InputState helper returns to
        // us which player actually provided the input. We pass that through to
        // OnSelectEntry and OnCancel, so they can tell which player triggered them.
        PlayerIndex playerIndex;

        if (inputState.IsMenuSelect(ControllingPlayer, out playerIndex))
        {
            OnSelectEntry(selectedEntry, playerIndex);
        }
        else if (inputState.IsMenuCancel(ControllingPlayer, out playerIndex))
        {
            OnCancel(playerIndex);
        }
    }

    private void TextSelectedCheck(Vector2 touchLocation)
    {
        for (int i = 0; i < menuEntries.Count; i++)
        {
            var textSize = ScreenManager.Font.MeasureString(menuEntries[i].Text);
            var entryBounds = new Rectangle((int)menuEntries[i].Position.X, (int)menuEntries[i].Position.Y, (int)textSize.X, (int)textSize.Y);

            touchLocation.Y += 5; // TODO work out why this magic nunber is needed

            if (entryBounds.Contains(touchLocation))
            {
                selectedEntry = i;
                OnSelectEntry(selectedEntry, ControllingPlayer ?? PlayerIndex.One);
                break;
            }
        }
    }

    private void SetNextEnabledMenu()
    {
        while (!menuEntries[selectedEntry].Enabled)
        {
            selectedEntry++;

            if (selectedEntry >= menuEntries.Count)
                selectedEntry = 0;
        }
    }


    /// <summary>
    /// Handler for when the user has chosen a menu entry.
    /// </summary>
    protected virtual void OnSelectEntry(int entryIndex, PlayerIndex playerIndex)
    {
        menuEntries[entryIndex].OnSelectEntry(playerIndex);
    }


    /// <summary>
    /// Handler for when the user has cancelled the menu.
    /// </summary>
    protected virtual void OnCancel(PlayerIndex playerIndex)
    {
        ExitScreen();
    }


    /// <summary>
    /// Helper overload makes it easy to use OnCancel as a MenuEntry event handler.
    /// </summary>
    protected void OnCancel(object sender, PlayerIndexEventArgs e)
    {
        OnCancel(e.PlayerIndex);
    }

    /// <summary>
    /// Allows the screen the chance to position the menu entries. By default
    /// all menu entries are lined up in a vertical list, centered on the screen.
    /// </summary>
    protected virtual void UpdateMenuEntryLocations()
    {
        // Make the menu slide into place during transitions, using a
        // power curve to make things look more interesting (this makes
        // the movement slow down as it nears the end).
        float transitionOffset = (float)Math.Pow(TransitionPosition, 2);

        // start at Y = 175; each X value is generated per entry
        Vector2 position = new Vector2(0f, 175f);

        // update each menu entry's location in turn
        for (int i = 0; i < menuEntries.Count; i++)
        {
            MenuEntry menuEntry = menuEntries[i];
            
            // each entry is to be centered horizontally
            position.X = ScreenManager.BaseScreenSize.X / 2 - menuEntry.GetWidth(this) / 2;

            if (ScreenState == ScreenState.TransitionOn)
                position.X -= transitionOffset * 256;
            else
                position.X += transitionOffset * 512;

            // set the entry's position
            menuEntry.Position = position;

            // move down for the next entry the size of this entry
            position.Y += menuEntry.GetHeight(this);
        }
    }


    /// <summary>
    /// Updates the menu.
    /// </summary>
    public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                   bool coveredByOtherScreen)
    {
        base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

        SetNextEnabledMenu();

        // Update each nested MenuEntry object.
        for (int i = 0; i < menuEntries.Count; i++)
        {
            bool isSelected = IsActive && (i == selectedEntry);

            menuEntries[i].Update(this, isSelected, gameTime);
        }
    }

    /// <summary>
    /// Draws the menu.
    /// </summary>
    public override void Draw(GameTime gameTime)
    {
        // make sure our entries are in the right place before we draw them
        UpdateMenuEntryLocations();

        GraphicsDevice graphics = ScreenManager.GraphicsDevice;
        SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
        SpriteFont font = ScreenManager.Font;

        spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, ScreenManager.GlobalTransformation);

        // Draw each menu entry in turn.
        for (int i = 0; i < menuEntries.Count; i++)
        {
            MenuEntry menuEntry = menuEntries[i];

            bool isSelected = IsActive && (i == selectedEntry);

            menuEntry.Draw(this, isSelected, gameTime);
        }

        // Make the menu slide into place during transitions, using a
        // power curve to make things look more interesting (this makes
        // the movement slow down as it nears the end).
        float transitionOffset = (float)Math.Pow(TransitionPosition, 2);

        // Draw the menu title centered on the screen
        Vector2 titlePosition = new Vector2(ScreenManager.BaseScreenSize.X / 2, 80);
        Vector2 titleOrigin = font.MeasureString(menuTitle) / 2;
        Color titleColor = menuTitleColor * TransitionAlpha;
        float titleScale = 1.25f;

        titlePosition.Y -= transitionOffset * 100;

        spriteBatch.DrawString(font, menuTitle, titlePosition, titleColor, 0,
                               titleOrigin, titleScale, SpriteEffects.None, 0);

        spriteBatch.End();
    }
}