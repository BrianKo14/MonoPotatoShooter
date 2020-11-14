using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.IO;

namespace PotatoShooter
{
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        public string playerName;

        #region VARIABLES
        public Vector2 windowSize = new Vector2(1200, 800);

        Texture2D shooter;
        Vector2 shooterPosition = new Vector2(0, 0);
        Texture2D bullet;
        Texture2D cannon;
        float cannonRotation = 0;
        Vector2 cannonPosition = new Vector2(0, 0);
        Vector2 cannonScale = new Vector2(0.5f, 0.5f);

        Texture2D enemy1;
        Texture2D background;
        SpriteFont scoreFont;
        SpriteFont titleFont;
        SpriteFont subtitleFont;
        SpriteFont subFont; //(Sasla subtitle)

        Texture2D redTexture;
        Texture2D whiteTexture;
        Texture2D powerUpTexture1;
        Texture2D powerUpTexture2;
        Texture2D powerUpTexture3;
        Texture2D boss1;
        Texture2D boss2;
        Texture2D boss2Friend;

        Song shotSound;
        //SoundEffectInstance shotSoundInstance;

        bool withCursor = true;
        Texture2D cursorTexture;
        Vector2 cursorPosition;
        float cursorRotation = 0;
        Vector2 cursorScale = new Vector2(0.1f, 0.1f);
        Vector2 CURSOR_SCALE = new Vector2(0.1f, 0.1f);
        Rectangle cursorRectangle;

        int bulletSpeed = 8; //8
        double reloadTime = 0.35; //0.5
        double enemyTime = 1; //2
        double[] powerUpTimes = new double[] {25, 0, 28}; //0=Sushi, 1=Heart, 2=Coffee
        double[] genericTimes = new double[] {50, 80, 0}; //0=Boss1, 1=Boss2, 2=Spaghetti
        int[] sendObject = new int[] { 0, 0}; //Number of power up to send + GameTime.Seconds + Number of times
        //Codes for [0] --> 0=[None] 1=Heart 2=Spaghetti

        int score = 0;
        const int enemyCost = 10;
        double hp = 100;
        bool lost = false;
        bool bossTime = false;

        List<Bullet> allBullets = new List<Bullet>();
        List<Enemy> allEnemies = new List<Enemy>();
        List<PowerUp> allPowerUps = new List<PowerUp>();
        List<GenericObject> allGenericObjects = new List<GenericObject>();

        #endregion

        #region Initialization
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }
        protected override void Initialize()
        {
            graphics.PreferredBackBufferHeight = 800;
            graphics.PreferredBackBufferWidth = 1200;
            graphics.ApplyChanges();
            //this.IsMouseVisible = true;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            shooter = this.Content.Load<Texture2D>("Tuti_Shooter");
            bullet = this.Content.Load<Texture2D>("Potato");
            cannon = this.Content.Load<Texture2D>("Cannon");
            enemy1 = this.Content.Load<Texture2D>("Enemy1");
            background = Content.Load<Texture2D>("backgroundPicture");
            scoreFont = Content.Load<SpriteFont>("score");
            titleFont = Content.Load<SpriteFont>("title");
            subFont = Content.Load<SpriteFont>("subFont");
            subtitleFont = Content.Load<SpriteFont>("Subtitle");
            cursorTexture = Content.Load<Texture2D>("cursor1");
            boss2Friend = Content.Load<Texture2D>("boss2Friend");

            redTexture = this.Content.Load<Texture2D>("Red");
            whiteTexture = this.Content.Load<Texture2D>("White");
            powerUpTexture1 = this.Content.Load<Texture2D>("PowerUp1");
            powerUpTexture2 = this.Content.Load<Texture2D>("PowerUp2");
            powerUpTexture3 = this.Content.Load<Texture2D>("PowerUp3");
            boss1 = this.Content.Load<Texture2D>("boss1");
            boss2 = this.Content.Load<Texture2D>("boss2");

            shotSound = Content.Load<Song>("shotSound");
            //shotSoundInstance = shotSound.CreateInstance();

            cannonPosition = new Vector2(windowSize.X  / 2, windowSize.Y - 150);
            shooterPosition = new Vector2(cannonPosition.X - 100, cannonPosition.Y - 20);
            CANNON_INITIAL_POSITION = cannonPosition;
        }

        protected override void UnloadContent()
        {
        }
        #endregion

        double moveShooter = -1;
        double moveCannon = -1;
        Vector2 CANNON_INITIAL_POSITION = new Vector2(0, 0);
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            #region Shortcuts
            if (Keyboard.GetState().IsKeyDown(Keys.R))
            {
                //this.Exit();
                Program.Main();
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.RightControl) && Keyboard.GetState().IsKeyDown(Keys.L))
            {
                hp = 0;
            }
            #endregion

            #region Cursor
            MouseState thisMouse = Mouse.GetState();
            cursorPosition = new Vector2(thisMouse.Position.X, thisMouse.Position.Y);
            cursorRectangle = new Rectangle((int)(cursorPosition.X - cursorTexture.Width * cursorScale.X / 2),
                    (int)(cursorPosition.Y - cursorTexture.Height * cursorScale.Y / 2),
                    (int)(cursorTexture.Width * cursorScale.X),
                    (int)(cursorTexture.Height * cursorScale.Y));
            #endregion

            if (hp <= 0 && !lost)
            {
                lost = true;
                leaderboardString = "L E A D E R B O A R D\n" + setLeaderboard();
            }
            else if (hp > 100)
            {
                hp = 100;
            }

            #region Cannon rotation
            if (thisMouse.X < cannonPosition.X)
            {
                cannonRotation = (float)(Math.PI * 2 - Math.Atan(Math.Abs(thisMouse.X - cannonPosition.X) / Math.Abs(thisMouse.Y - cannonPosition.Y)));
            }
            else
            {
                cannonRotation = -(float)(Math.PI * 2 - Math.Atan(Math.Abs(thisMouse.X - cannonPosition.X) / Math.Abs(thisMouse.Y - cannonPosition.Y)));
            }
            #endregion

            double angle = cannonRotation > 0 ? cannonRotation - (Math.PI / 2) : cannonRotation + (7 * Math.PI / 2);
            //Console.WriteLine(angle); //Angle oscillates between 2PI and PI

            #region CREATES NEW ELEMENT

            #region Creates new bullet
            if (gameTime.TotalGameTime.TotalSeconds % reloadTime < 0.001 && gameTime.TotalGameTime.TotalSeconds != 0 && !lost)
            { //Trying chaing 0.001 for 0.1 or 1 for cool effect.
                moveCannon = gameTime.TotalGameTime.TotalSeconds;

                Bullet newBullet = new Bullet();
                newBullet.Position = cannonPosition;
                newBullet.Texture = bullet;
                newBullet.Velocity = new Vector2((float)(Math.Cos(angle) * bulletSpeed), (float)(Math.Sin(angle) * bulletSpeed));
                newBullet.RotationSpeed = 0.1;
                newBullet.Origin = new Vector2(newBullet.Texture.Width/2, newBullet.Texture.Height/2);
                newBullet.Scale = new Vector2(0.1f, 0.1f);
                allBullets.Add(newBullet);
                //shotSoundInstance.Play();
                //MediaPlayer.Play(shotSound);
            }
            else if (gameTime.TotalGameTime.TotalSeconds % (reloadTime - 0.3) < 0.001 && moveShooter == -1)
            { //Reload effect event
                moveShooter = gameTime.TotalGameTime.TotalSeconds;
            }
            #endregion

            #region Creates new enemy
            if (gameTime.TotalGameTime.TotalSeconds % enemyTime < 0.001 && !lost && !bossTime)
            {
                Enemy newEnemy = new Enemy();
                newEnemy.Texture = enemy1;
                Random rnd = new Random();
                newEnemy.Position = new Vector2(rnd.Next(newEnemy.Texture.Width / 2 + 1, graphics.PreferredBackBufferWidth - newEnemy.Texture.Width / 2 - 1), rnd.Next(300)); //300=Vertical line threshold of apparition
                newEnemy.Origin = new Vector2(newEnemy.Texture.Width / 2, newEnemy.Texture.Height / 2);
                newEnemy.Scale = new Vector2(0.2f, 0.2f);
                newEnemy.ShiftProbability = rnd.Next(50, 100);
                newEnemy.direction = rnd.Next(1) == 0 ? true : false;
                allEnemies.Add(newEnemy);
            }
            #endregion

            #region Creates new generic
            if ((((gameTime.TotalGameTime.TotalSeconds % genericTimes[0] < 0.001 && score > 30) || score == 30) && !bossTime && !lost) || (Keyboard.GetState().IsKeyDown(Keys.RightControl) && Keyboard.GetState().IsKeyDown(Keys.J) && !bossTime)) //Boss 1
            {
                GenericObject newGenericObject = new GenericObject();
                newGenericObject.Texture = boss1;
                Random rnd = new Random();
                newGenericObject.Position = new Vector2(rnd.Next(newGenericObject.Texture.Width / 2 + 1, graphics.PreferredBackBufferWidth - newGenericObject.Texture.Width / 2 - 1), 0); //300=Vertical line threshold of apparition
                newGenericObject.Origin = new Vector2(newGenericObject.Texture.Width / 2, newGenericObject.Texture.Height / 2);
                newGenericObject.Scale = new Vector2(0.5f, 0.5f);
                newGenericObject.ShiftProbability = rnd.Next(50, 100);
                newGenericObject.Health = 10;
                newGenericObject.VerticalSpeed = 1f;
                newGenericObject.HorizontalSpeed = 3f;
                newGenericObject.Type = 0;
                newGenericObject.direction = rnd.Next(1) == 0 ? true : false;
                allGenericObjects.Add(newGenericObject);
                bossTime = true;
            }
            else if ((((gameTime.TotalGameTime.TotalSeconds % genericTimes[1] < 0.001 && score > 100) || score == 100) && !bossTime && !lost) || (Keyboard.GetState().IsKeyDown(Keys.RightControl) && Keyboard.GetState().IsKeyDown(Keys.K) && !bossTime)) //Boss 2
            {
                GenericObject newGenericObject = new GenericObject();
                newGenericObject.Texture = boss2;
                Random rnd = new Random();
                newGenericObject.Position = new Vector2(rnd.Next(newGenericObject.Texture.Width / 2 + 1, graphics.PreferredBackBufferWidth - newGenericObject.Texture.Width / 2 - 1), 0); //300=Vertical line threshold of apparition
                newGenericObject.Origin = new Vector2(newGenericObject.Texture.Width / 2, newGenericObject.Texture.Height / 2);
                newGenericObject.Scale = new Vector2(0.5f, 0.5f);
                newGenericObject.ShiftProbability = rnd.Next(50, 100);
                newGenericObject.Health = 18;
                newGenericObject.VerticalSpeed = 0.6f;
                newGenericObject.HorizontalSpeed = 5f;
                newGenericObject.Type = 1;
                newGenericObject.direction = rnd.Next(1) == 0 ? true : false;
                newGenericObject.RotationalSpeed = 0.001f;
                allGenericObjects.Add(newGenericObject);
                bossTime = true;
            }

            if (sendObject[0] == 2 && !lost) //Josue
            {
                    GenericObject newGenericObject = new GenericObject();
                    newGenericObject.Texture = boss2Friend;
                    Random rnd = new Random();
                    newGenericObject.Position = new Vector2(rnd.Next(newGenericObject.Texture.Width / 2 + 1, graphics.PreferredBackBufferWidth - newGenericObject.Texture.Width / 2 - 1), 0); //300=Vertical line threshold of apparition
                    newGenericObject.Scale = new Vector2(0.2f, 0.2f);
                    newGenericObject.Origin = new Vector2(newGenericObject.Texture.Width / 2, newGenericObject.Texture.Height / 2);
                    newGenericObject.ShiftProbability = rnd.Next(50, 100);
                    newGenericObject.Health = 2;
                    newGenericObject.VerticalSpeed = 0.5f;
                    newGenericObject.HorizontalSpeed = 1f;
                newGenericObject.RotationalSpeed = 0.1f;
                    newGenericObject.Type = 2;
                    newGenericObject.direction = rnd.Next(1) == 0 ? true : false;
                    allGenericObjects.Add(newGenericObject);
                    bossTime = true;
            }
            #endregion

            #region Create new power up
            if (gameTime.TotalGameTime.TotalSeconds % powerUpTimes[0] < 0.001 && !lost && gameTime.TotalGameTime.TotalSeconds != 0 && !bossTime) // Faster bullets & slightly more enemies
            { //Sushi
                PowerUp newPowerUp = new PowerUp();
                newPowerUp.Texture = powerUpTexture1;
                Random rnd = new Random();
                newPowerUp.Position = new Vector2(rnd.Next(newPowerUp.Texture.Width / 2 + 1, graphics.PreferredBackBufferWidth - newPowerUp.Texture.Width / 2 - 1), rnd.Next(300)); //300=Vertical line threshold of apparition
                newPowerUp.Origin = new Vector2(newPowerUp.Texture.Width / 2, newPowerUp.Texture.Height / 2);
                newPowerUp.Scale = new Vector2(0.2f, 0.2f);
                newPowerUp.ShiftProbability = rnd.Next(50, 100);
                newPowerUp.Type = 0;
                newPowerUp.Duration = 5;
                allPowerUps.Add(newPowerUp);
            }
            else if (gameTime.TotalGameTime.TotalSeconds % powerUpTimes[2] < 0.001 && !lost && gameTime.TotalGameTime.TotalSeconds != 0) // Faster enemies
            { //Coffee
                PowerUp newPowerUp = new PowerUp();
                newPowerUp.Texture = powerUpTexture3;
                Random rnd = new Random();
                newPowerUp.Position = new Vector2(rnd.Next(newPowerUp.Texture.Width / 2 + 1, graphics.PreferredBackBufferWidth - newPowerUp.Texture.Width / 2 - 1), rnd.Next(300)); //300=Vertical line threshold of apparition
                newPowerUp.Origin = new Vector2(newPowerUp.Texture.Width / 2, newPowerUp.Texture.Height / 2);
                newPowerUp.Scale = new Vector2(0.3f, 0.3f);
                newPowerUp.ShiftProbability = rnd.Next(50, 100);
                newPowerUp.Type = 2;
                newPowerUp.Duration = 13;
                newPowerUp.VerticalSpeed = 1.8f;
                newPowerUp.HorizontalSpeed = 2f;
                newPowerUp.direction = rnd.Next(1) == 0 ? true : false;
                allPowerUps.Add(newPowerUp);
            }

            if (sendObject[0] == 1 && gameTime.TotalGameTime.TotalSeconds + 3 > sendObject[1] && !lost) // More health
            { //Heart
                    sendObject[0] = 0;
                    sendObject[1] = 0;
                    PowerUp newPowerUp = new PowerUp();
                    newPowerUp.Texture = powerUpTexture2;
                    Random rnd = new Random();
                    newPowerUp.Position = new Vector2(rnd.Next(newPowerUp.Texture.Width / 2 + 1, graphics.PreferredBackBufferWidth - newPowerUp.Texture.Width / 2 - 1), rnd.Next(300)); //300=Vertical line threshold of apparition
                    newPowerUp.Origin = new Vector2(newPowerUp.Texture.Width / 2, newPowerUp.Texture.Height / 2);
                    newPowerUp.Scale = new Vector2(0.2f, 0.2f);
                    newPowerUp.ShiftProbability = rnd.Next(70, 90);
                    newPowerUp.Type = 1;
                    newPowerUp.VerticalSpeed = 3f;
                    newPowerUp.HorizontalSpeed = 8f;
                    newPowerUp.Duration = 1;
                    allPowerUps.Add(newPowerUp);
            }
            #endregion

            #endregion

            #region Shooter and cannon animation effect
            //gameTime.TotalGameTime.TotalSeconds - moveShooter = How many seconds since moveShooter started
            if (moveShooter != -1 && !lost) //Reload shooter effect
            {
                if (gameTime.TotalGameTime.TotalSeconds - moveShooter <= 0.2)
                {
                    shooterPosition.Y -= 1f;
                }
                else if (gameTime.TotalGameTime.TotalSeconds - moveShooter <= 0.4)
                {
                    shooterPosition.Y += 1f;
                }
                else
                {
                    moveShooter = -1;
                }
            }
            if (moveCannon != -1) //Recoil effect
            {
                double x = gameTime.TotalGameTime.TotalSeconds - moveCannon;
                double mu = -1.1;
                double r = 3.3;
                double sigma = 0.7;
                if (x <= 2 && x != 0)
                {
                    double func = (r * (1 / x) * (1 / (sigma * Math.Sqrt(2 * Math.PI)) * Math.Exp(-Math.Pow((Math.Log(x - mu)), 2) / (2 * Math.Pow(sigma, 2)))));
                    cannonPosition.Y = CANNON_INITIAL_POSITION.Y - (float)(Math.Sin(angle) * func);
                    cannonPosition.X = CANNON_INITIAL_POSITION.X - (float)(Math.Cos(angle) * func);
                }
                else if (x > 0)
                {
                    moveCannon = -1;
                }

            }
            #endregion

            #region Update stuff
            bool willShrinkCursor = false;
            #region UPDATE BULLET
            foreach (Bullet singleBullet in allBullets.ToArray())
            {
                singleBullet.Position.Y += singleBullet.Velocity.Y;
                singleBullet.Position.X += singleBullet.Velocity.X;
                singleBullet.RotationAngle += (float)singleBullet.RotationSpeed;
                
                if (singleBullet.Position.Y + singleBullet.Texture.Height < 0 || (singleBullet.Position.X > graphics.PreferredBackBufferWidth || singleBullet.Position.X + singleBullet.Texture.Width < 0))
                { //Remove when bullet leaves display
                    allBullets.RemoveAt(allBullets.IndexOf(singleBullet));
                }

                #region Sets bullets' rectangles:
                singleBullet.Rectangle = new Rectangle((int)(singleBullet.Position.X - singleBullet.Texture.Width * singleBullet.Scale.X / 2),
                        (int)(singleBullet.Position.Y - singleBullet.Texture.Height * singleBullet.Scale.Y / 2),
                        (int)(singleBullet.Texture.Width * singleBullet.Scale.X),
                        (int)(singleBullet.Texture.Height * singleBullet.Scale.Y));
                #endregion
            }
            #endregion

            #region UPDATE ENEMY
            foreach (Enemy singleEnemy in allEnemies.ToArray())
            {
                Random rnd = new Random();
                if (rnd.Next(singleEnemy.ShiftProbability) == 0 ||
                    singleEnemy.Position.X - singleEnemy.Texture.Width * singleEnemy.Scale.X / 2 <= 0 ||
                singleEnemy.Position.X + singleEnemy.Texture.Width * singleEnemy.Scale.X / 2 > windowSize.X)
                {
                    singleEnemy.direction = !singleEnemy.direction;
                }
                singleEnemy.Update();

                #region Sets enemies' rectangles:
                singleEnemy.Rectangle = new Rectangle((int)(singleEnemy.Position.X - singleEnemy.Texture.Width * singleEnemy.Scale.X / 2),
                    (int)(singleEnemy.Position.Y - singleEnemy.Texture.Height * singleEnemy.Scale.Y / 2),
                    (int)(singleEnemy.Texture.Width * singleEnemy.Scale.X),
                    (int)(singleEnemy.Texture.Height * singleEnemy.Scale.Y));
                #endregion

                if (singleEnemy.Rectangle.Intersects(cursorRectangle))
                {
                    willShrinkCursor = true;
                }

                foreach (Bullet singleBullet in allBullets.ToArray())
                {

                    if (singleBullet.Rectangle.Intersects(singleEnemy.Rectangle))
                    {
                        allBullets.Remove(singleBullet);
                        singleEnemy.isDisappearing = true;
                        //allEnemies.Remove(singleEnemy);
                        score++;
                    }
                }
                
                
                
                if (singleEnemy.isDisappearing && singleEnemy.Scale.X > 0)
                {
                    singleEnemy.Scale.X -= 0.03f; singleEnemy.Scale.Y -= 0.03f;
                    singleEnemy.RotationAngle += 1;
                }
                else if (singleEnemy.Scale.X <= 0)
                {
                    allEnemies.Remove(singleEnemy);
                }

                if (singleEnemy.Position.Y >= windowSize.Y)
                {
                    hp -= enemyCost;
                    allEnemies.Remove(singleEnemy);
                }
            }
            #endregion

            #region UPDATE POWERUPS
            foreach (PowerUp singlePowerUp in allPowerUps.ToArray())
            {
                Random rnd = new Random();
                if (rnd.Next(singlePowerUp.ShiftProbability) == 0 ||
                    singlePowerUp.Position.X - singlePowerUp.Texture.Width * singlePowerUp.Scale.X / 2 <= 0 ||
                singlePowerUp.Position.X + singlePowerUp.Texture.Width * singlePowerUp.Scale.X / 2 > windowSize.X)
                {
                    singlePowerUp.direction = !singlePowerUp.direction;
                }
                singlePowerUp.Update();

                #region Sets power ups' rectangles:
                singlePowerUp.Rectangle = new Rectangle((int)(singlePowerUp.Position.X - singlePowerUp.Texture.Width * singlePowerUp.Scale.X / 2),
                    (int)(singlePowerUp.Position.Y - singlePowerUp.Texture.Height * singlePowerUp.Scale.Y / 2),
                    (int)(singlePowerUp.Texture.Width * singlePowerUp.Scale.X),
                    (int)(singlePowerUp.Texture.Height * singlePowerUp.Scale.Y));
                #endregion

                if (singlePowerUp.Rectangle.Intersects(cursorRectangle))
                {
                    willShrinkCursor = true;
                }

                foreach (Bullet singleBullet in allBullets.ToArray()) //If intersection
                {
                    if (singleBullet.Rectangle.Intersects(singlePowerUp.Rectangle))
                    {
                        allBullets.Remove(singleBullet);
                        singlePowerUp.isDisappearing = true;
                        singlePowerUp.Activated = true;
                    }
                }

                if (singlePowerUp.isDisappearing && singlePowerUp.Scale.X > 0.02)
                {
                    singlePowerUp.Scale.X -= 0.02f; singlePowerUp.Scale.Y -= 0.02f;
                    singlePowerUp.RotationAngle += 1;
                }
                if (singlePowerUp.Position.Y >= windowSize.Y && singlePowerUp.isDisappearing == false)
                {
                    allPowerUps.Remove(singlePowerUp);
                }

                #region >>>> POWER UP EFFECTS <<<<
                if (singlePowerUp.Activated)
                {
                    if (singlePowerUp.Timer <= singlePowerUp.Duration)
                    {
                        if (singlePowerUp.Type == 0 && singlePowerUp.Timer == 0)
                        {
                            reloadTime *= 0.01;
                            enemyTime /= 3;
                        }
                        else if (singlePowerUp.Type == 1 && singlePowerUp.Timer == 0)
                        {
                            hp += 30;
                        }
                        else if (singlePowerUp.Type == 2 && singlePowerUp.Timer == 0)
                        {
                            foreach (Enemy singleEnemy in allEnemies)
                            {
                                singleEnemy.VerticalSpeed *= 1.5f;
                            }
                            enemyTime *= 0.7;
                        }
                        singlePowerUp.Timer += gameTime.ElapsedGameTime.TotalSeconds;
                    }
                    else
                    {
                        if (singlePowerUp.Type == 0)
                        {
                            reloadTime /= 0.01;
                            enemyTime *= 3;
                            sendObject[0] = 1;
                            sendObject[1] = (int)gameTime.TotalGameTime.TotalSeconds;
                        }
                        else if (singlePowerUp.Type == 1)
                        {
                            allPowerUps.Remove(singlePowerUp);
                        }
                        else if (singlePowerUp.Type == 2)
                        {
                            foreach (Enemy singleEnemy in allEnemies)
                            {
                                singleEnemy.VerticalSpeed /= 1.5f;
                            }
                            enemyTime /= 0.7;
                        }
                        singlePowerUp.Activated = false;
                    }
                    //Console.WriteLine(singlePowerUp.Timer);
                }
                #endregion

                if (singlePowerUp.Type == 2)
                {
                    singlePowerUp.RotationAngle += 0.01f;
                }
            }
            #endregion

            #region UPDATE GENERIC OBJECT
            foreach (GenericObject singleGeneric in allGenericObjects.ToArray())
            {
                #region Non-specific set ups. Bouncer, rectangles and cursor intersection.
                Random rnd = new Random();
                if (rnd.Next(singleGeneric.ShiftProbability) == 0 ||
                    singleGeneric.Position.X - singleGeneric.Texture.Width * singleGeneric.Scale.X / 2 <= 0 ||
                singleGeneric.Position.X + singleGeneric.Texture.Width * singleGeneric.Scale.X / 2 > windowSize.X)
                {
                    singleGeneric.direction = !singleGeneric.direction;
                }
                singleGeneric.Update();

                #region Sets generics' rectangles:
                singleGeneric.Rectangle = new Rectangle((int)(singleGeneric.Position.X - singleGeneric.Texture.Width * singleGeneric.Scale.X / 2),
                    (int)(singleGeneric.Position.Y - singleGeneric.Texture.Height * singleGeneric.Scale.Y / 2),
                    (int)(singleGeneric.Texture.Width * singleGeneric.Scale.X),
                    (int)(singleGeneric.Texture.Height * singleGeneric.Scale.Y));
                #endregion

                if (singleGeneric.Rectangle.Intersects(cursorRectangle))
                {
                    willShrinkCursor = true;
                }
                #endregion

                foreach (Bullet singleBullet in allBullets.ToArray())
                {

                    if (singleBullet.Rectangle.Intersects(singleGeneric.Rectangle))
                    {
                        allBullets.Remove(singleBullet);
                        if (singleGeneric.Type == 0) //Changes with type
                        {
                            singleGeneric.Health--;
                            singleGeneric.Scale.X *= 0.9f; singleGeneric.Scale.Y *= 0.9f;
                        }
                        else if (singleGeneric.Type == 1)
                        {
                            singleGeneric.Health--;
                            singleGeneric.Scale.X *= 0.95f; singleGeneric.Scale.Y *= 0.95f;
                            Random rnd1 = new Random();
                            if (rnd1.Next(4) == 1)
                            {
                                singleGeneric.Position.X = rnd.Next(singleGeneric.Texture.Height * (int)singleGeneric.Scale.Y, (int)windowSize.X - singleGeneric.Texture.Height * (int)singleGeneric.Scale.Y);
                                singleGeneric.RotationAngle = rnd.Next(3) / 2;
                            }
                            singleGeneric.RotationalSpeed += 0.001f;
                            singleGeneric.VerticalSpeed += 0.05f;
                            //singleGeneric.HorizontalSpeed += 0.1f;
                            singleGeneric.HorizontalSpeed = (float)(-0.00002 * Math.Pow(2, singleGeneric.Health) + 10);
                        }
                        else if (singleGeneric.Type == 2)
                        {
                            singleGeneric.Health--;
                            singleGeneric.Scale.X *= 0.5f; singleGeneric.Scale.Y *= 0.5f;
                            singleGeneric.VerticalSpeed *= 8f; singleGeneric.HorizontalSpeed *= 8f;
                        }
                    }
                }

                if (singleGeneric.Type == 1) //Changes with type
                {
                    singleGeneric.RotationAngle += singleGeneric.RotationalSpeed;
                    singleGeneric.Scale *= 0.9999f;
                    Random rnd1 = new Random();
                    if (rnd1.Next(200) == 0)
                    {
                        sendObject[0] = 2;
                        sendObject[1] = (int)gameTime.TotalGameTime.TotalSeconds;
                    }
                    else
                    {
                        sendObject[0] = 0;
                        sendObject[1] = 0;

                    }
                }
                else if (singleGeneric.Type == 2 && singleGeneric.Health == 1)
                {
                    singleGeneric.RotationAngle += singleGeneric.RotationalSpeed;
                }

                if (singleGeneric.Health <= 0) //Changes with type
                {
                    if (bossTime && singleGeneric.Type == 0)
                    {
                        singleGeneric.isDisappearing = true;
                        score += 10;
                        bossTime = false;
                    }
                    else if (bossTime && singleGeneric.Type == 1)
                    {
                        singleGeneric.isDisappearing = true;
                        score += 25;
                        bossTime = false;
                        //sendObject[0] = 1;
                        //sendObject[1] = (int)gameTime.TotalGameTime.TotalSeconds;
                    }
                    else if (singleGeneric.Type == 2)
                    {
                        singleGeneric.isDisappearing = true;
                        score += 1;
                    }
                }


                #region Non-specifics
                if (singleGeneric.isDisappearing && singleGeneric.Scale.X > 0)
                {
                    singleGeneric.Scale.X -= 0.03f; singleGeneric.Scale.Y -= 0.03f;
                    singleGeneric.RotationAngle += 1;
                }
                else if (singleGeneric.Scale.X <= 0)
                {
                    allGenericObjects.Remove(singleGeneric);
                }
                #endregion

                if (singleGeneric.Position.Y >= windowSize.Y) //Changes with type
                {
                    if (singleGeneric.Type == 0)
                    {
                        hp -= enemyCost * 2; //When boss survives
                        allGenericObjects.Remove(singleGeneric);
                        bossTime = false;
                    }
                    else if (singleGeneric.Type == 1)
                    {
                        hp *= 0.5;
                        allGenericObjects.Remove(singleGeneric);
                        bossTime = false;
                    }
                    else if (singleGeneric.Type == 2)
                    {
                        hp -= singleGeneric.Health*10;
                        allGenericObjects.Remove(singleGeneric);
                    }

                }
            }
            #endregion
            #endregion

            
            float sh = 0.8f;
            if (willShrinkCursor)
            {
                cursorScale = CURSOR_SCALE * sh;
            }
            else
            {
                cursorScale = CURSOR_SCALE / sh;
            }

            base.Update(gameTime);
        }

        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);


            spriteBatch.Begin();

            //Rectangle mainFrame = new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            //priteBatch.Draw(background, mainFrame, Color.White);

            #region Draw fallers
            foreach (Enemy singleEnemy in allEnemies)
            {
                spriteBatch.Draw(singleEnemy.Texture, position: singleEnemy.Position, scale: singleEnemy.Scale, rotation: singleEnemy.RotationAngle, origin: singleEnemy.Origin);

                #region Draw hitbox
                //Texture2D texture = new Texture2D(GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
                //texture.SetData<Color>(new Color[] { Color.White });
                //spriteBatch.Draw(texture, singleEnemy.Rectangle, Color.White);
                #endregion
            }
            foreach (Bullet singleBullet in allBullets)
            {
                spriteBatch.Draw(singleBullet.Texture, position: singleBullet.Position, scale: singleBullet.Scale, rotation: singleBullet.RotationAngle, origin: singleBullet.Origin);

                #region Draw hitbox
                //Texture2D texture = new Texture2D(GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
                //texture.SetData<Color>(new Color[] { Color.Red });
                //spriteBatch.Draw(texture, singleBullet.Rectangle, Color.White);
                #endregion
            }
            foreach (PowerUp singlePowerUp in allPowerUps)
            {
                spriteBatch.Draw(singlePowerUp.Texture, position: singlePowerUp.Position, scale: singlePowerUp.Scale, rotation: singlePowerUp.RotationAngle, origin: singlePowerUp.Origin);
            }
            foreach (GenericObject singleGeneric in allGenericObjects)
            {
                spriteBatch.Draw(singleGeneric.Texture, position: singleGeneric.Position, scale: singleGeneric.Scale, rotation: singleGeneric.RotationAngle, origin: singleGeneric.Origin);
            }
            #endregion

            spriteBatch.Draw(cannon, position: cannonPosition, scale: cannonScale, origin: new Vector2(cannon.Width/2, cannon.Height/2), rotation: cannonRotation);
            spriteBatch.Draw(shooter, position: shooterPosition, scale: new Vector2(0.18f, 0.18f));

            spriteBatch.DrawString(scoreFont, "Score: " + score.ToString(), new Vector2(windowSize.X - 500, windowSize.Y - 70), Color.White);

            int height = 20;
            int width = 400;
            int edge = 6;
            spriteBatch.Draw(whiteTexture, new Rectangle(20 - edge/2, (int)(windowSize.Y - 40 - edge/2), width + edge, height + edge), Color.White);
            spriteBatch.Draw(redTexture, new Rectangle(20, (int)(windowSize.Y - 40), (int)(width * (hp/100)), height), Color.Red);

            if (lost)
            {
                spriteBatch.DrawString(titleFont, "YOU LOST", new Vector2(-10, windowSize.Y/2 - 280), Color.White);
                spriteBatch.DrawString(subFont, "SASLA WINS", new Vector2(windowSize.X /2 + 250, windowSize.Y / 2 + 75), Color.Black);
                string leaderboard = leaderboardString;
                spriteBatch.DrawString(subtitleFont, leaderboard, new Vector2(20, windowSize.Y / 2 + 75), Color.White);
            }

            if (withCursor)
                spriteBatch.Draw(cursorTexture, cursorPosition, origin: new Vector2(cursorTexture.Width / 2, cursorTexture.Height / 2), rotation: cursorRotation, scale: cursorScale);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        #region Files
        string leaderboardString;
        string setLeaderboard()
        {
            string rtrn = "";
            string filePath = "Leaderboard.txt";

            if (!File.Exists(filePath))
            {
                TextWriter tw = new StreamWriter(filePath);
                tw.WriteLine("Player/0");
                tw.WriteLine("Player/0");
                tw.WriteLine("Player/0");
                tw.WriteLine("Player/0");
                tw.WriteLine("Player/0");
                tw.Close();
            }

            string[] leaderboard = File.ReadAllLines(filePath);
            if (score > Convert.ToInt32(leaderboard[0].Split('/')[1]))
            {
                leaderboard[4] = leaderboard[3]; 
                leaderboard[3] = leaderboard[2]; 
                leaderboard[2] = leaderboard[1]; 
                leaderboard[1] = leaderboard[0]; 
                leaderboard[0] = playerName + "/" + score.ToString();
            }
            else if(score > Convert.ToInt32(leaderboard[1].Split('/')[1]))
            {
                leaderboard[4] = leaderboard[3];
                leaderboard[3] = leaderboard[2];
                leaderboard[2] = leaderboard[1];
                leaderboard[1] = playerName + "/" + score.ToString();
            }
            else if (score > Convert.ToInt32(leaderboard[2].Split('/')[1]))
            {
                leaderboard[4] = leaderboard[3];
                leaderboard[3] = leaderboard[2];
                leaderboard[2] = playerName + "/" + score.ToString();
            }
            else if (score > Convert.ToInt32(leaderboard[3].Split('/')[1]))
            {
                leaderboard[4] = leaderboard[3];
                leaderboard[3] = playerName + "/" + score.ToString();
            }
            else if (score > Convert.ToInt32(leaderboard[4].Split('/')[1]))
            {
                leaderboard[4] = playerName + "/" + score.ToString();
            }
            File.WriteAllLines(filePath, leaderboard);

            foreach (string line in leaderboard)
            {
                if (Array.IndexOf(leaderboard, line) == 4)
                {
                    rtrn += line.Split('/')[0] + " " + line.Split('/')[1];
                }
                else
                {
                    rtrn += line.Split('/')[0] + " " + line.Split('/')[1] + "\n";
                }
            }

            return rtrn;
        }

        #endregion
    }
}

/* To-do list:
 * - Remove each bullet when they leave the display
 * 
 * 
 * Shortcuts:
 * - RightCtrl + L: ==> Health to 0
 * - RightCtrl + K: ==> Boss2
 * - RightCtrl + J: ==> Boss1
 * 
*/

