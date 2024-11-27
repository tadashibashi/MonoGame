﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace ___SafeGameName___.Screens;

/// <summary>
/// The background screen sits behind all the other menu screens.
/// It draws a background image that remains fixed in place regardless
/// of whatever transitions the screens on top of it may be doing.
/// </summary>
class BackgroundScreen : GameScreen
{
    ContentManager content;
    Texture2D backgroundTexture;

    /// <summary>
    /// Constructor.
    /// </summary>
    public BackgroundScreen()
    {
        TransitionOnTime = TimeSpan.FromSeconds(0.5);
        TransitionOffTime = TimeSpan.FromSeconds(0.5);
    }


    /// <summary>
    /// Loads graphics content for this screen. The background texture is quite
    /// big, so we use our own local ContentManager to load it. This allows us
    /// to unload before going from the menus into the game itself, wheras if we
    /// used the shared ContentManager provided by the Game class, the content
    /// would remain loaded forever.
    /// </summary>
    public override void LoadContent()
    {
        if (content == null)
            content = new ContentManager(ScreenManager.Game.Services, "Content");

        backgroundTexture = content.Load<Texture2D>("Backgrounds/Layer0_2");
    }


    /// <summary>
    /// Unloads graphics content for this screen.
    /// </summary>
    public override void UnloadContent()
    {
        content.Unload();
    }

    /// <summary>
    /// Updates the background screen. Unlike most screens, this should not
    /// transition off even if it has been covered by another screen: it is
    /// supposed to be covered, after all! This overload forces the
    /// coveredByOtherScreen parameter to false in order to stop the base
    /// Update method wanting to transition off.
    /// </summary>
    public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                   bool coveredByOtherScreen)
    {
        base.Update(gameTime, otherScreenHasFocus, false);
    }


    /// <summary>
    /// Draws the background screen.
    /// </summary>
    public override void Draw(GameTime gameTime)
    {
        SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
        Rectangle fullscreen = new Rectangle(0, 0, (int)ScreenManager.BaseScreenSize.X, (int)ScreenManager.BaseScreenSize.Y);

        spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, ScreenManager.GlobalTransformation);

        spriteBatch.Draw(backgroundTexture, fullscreen,
                         new Color(TransitionAlpha, TransitionAlpha, TransitionAlpha));

        spriteBatch.End();
    }
}