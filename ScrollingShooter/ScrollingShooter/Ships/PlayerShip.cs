﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using System;

namespace ScrollingShooter
{
    /// <summary>
    /// The different types of player ships available in the game
    /// </summary>
    public enum PlayerShipType
    {
        Shrike,
    }

    /// <summary>
    /// Represents the five possible steering states for our ships
    /// </summary>
    enum SteeringState
    {
        HardLeft = 0,
        Left = 1,
        Straight = 2,
        Right = 3,
        HardRight = 4,
    }

    /// <summary>
    /// A base class for all player ships
    /// </summary>
    public abstract class PlayerShip : GameObject
    {
        // Timers
        /// <summary>
        /// Timer for the default gun
        /// </summary>
        public float defaultGunTimer = 5;
        float bombTimer = 1.5f;

        //Timer for how longs the blades have been active.
        float bladesPowerupTimer = 0;

        /// <summary>
        /// Timer for the energy blast gun
        /// </summary>
        public float energyBlastTimer = 0;

        // Powerup Levels
        /// <summary>
        /// Level of the energy blast powerup
        /// </summary>
        public int energyBlastLevel = 0;

        /// <summary>
        /// The velocity of the ship - varies from ship to ship
        /// </summary>
        protected Vector2 velocity;

        /// <summary>
        /// The position of the ship in the game world.  We use a Vector2 for position 
        /// rather than a Rectangle, as floats allow us to move less than a pixel
        /// </summary>
        protected Vector2 position;

        /// <summary>
        /// The spritesheet our ship is found upon
        /// </summary>
        protected Texture2D spriteSheet;

        /// <summary>
        /// The spritebounds for our ship, corresponding to the five possible steering states
        /// </summary>
        protected Rectangle[] spriteBounds = new Rectangle[5];

        /// <summary>
        /// The keyboard state from the previous frame, used to see if a key was just pressed,
        /// or held down
        /// </summary>
        KeyboardState oldKeyboardState = Keyboard.GetState();

        // The current steering state of the ship
        SteeringState steeringState = SteeringState.Straight;

        // The PowerupType equipped on this ship
        PowerupType PowerupType = PowerupType.None;


        /// <summary>
        /// The bounding rectangle for the ship.  Generated from the animation frame and the ship's
        /// position.
        /// </summary>
        public override Rectangle Bounds
        {
            get { return new Rectangle((int)position.X, (int)position.Y, spriteBounds[0].Width, spriteBounds[0].Height); }
        }


        /// <summary>
        /// Creates a new player ship instance
        /// </summary>
        /// <param name="id">the unique id of the player ship</param>
        public PlayerShip(uint id) : base(id) { }


        /// <summary>
        /// Applies the specified powerup to the ship
        /// </summary>
        /// <param name="powerup">the indicated powerup</param>
        public void ApplyPowerup(PowerupType powerup)
        {
            // Store the new powerup in the PowerupType bitmask
            this.PowerupType |= powerup;

            //Apply special logic for powerups here
            switch (powerup)
            {
                case PowerupType.Blades:
                    ApplyBlades();
                    break;
                case PowerupType.EightBallShield:
                    TriggerEightBallShield();
                    break;
                case PowerupType.TriShield:
                    ApplyTriShield();
                    break;
            }

        }


        /// <summary>
        /// Updates the ship
        /// </summary>
        /// <param name="elapsedTime"></param>
        public override void Update(float elapsedTime)
        {
            KeyboardState currentKeyboardState = Keyboard.GetState();

            // Update timers
            defaultGunTimer += elapsedTime;
            bladesPowerupTimer += elapsedTime;
            energyBlastTimer -= elapsedTime;
            bombTimer += elapsedTime;

            // Steer the ship up or down according to user input
            if(currentKeyboardState.IsKeyDown(Keys.Up))
            {
                position.Y -= elapsedTime * velocity.Y;
            } 
            else if(currentKeyboardState.IsKeyDown(Keys.Down))
            {
                position.Y += elapsedTime * velocity.Y;
            }

            // Steer the ship left or right according to user input
            steeringState = SteeringState.Straight;

            if (currentKeyboardState.IsKeyDown(Keys.Left))
            {
                if (currentKeyboardState.IsKeyDown(Keys.LeftShift) ||
                    currentKeyboardState.IsKeyDown(Keys.RightShift))
                {
                    steeringState = SteeringState.HardLeft;
                    position.X -= elapsedTime * 2 * velocity.X;

                }
                else
                {
                    steeringState = SteeringState.Left;
                    position.X -= elapsedTime * velocity.X;
                }
            }
            else if (currentKeyboardState.IsKeyDown(Keys.Right))
            {
                if (currentKeyboardState.IsKeyDown(Keys.LeftShift) ||
                    currentKeyboardState.IsKeyDown(Keys.RightShift))
                {
                    position.X += elapsedTime * 2 * velocity.X;
                    steeringState = SteeringState.HardRight;
                }
                else
                {
                    position.X += elapsedTime * velocity.X;
                    steeringState = SteeringState.Right;
                }
            }
            // Fire bomb
            if (currentKeyboardState.IsKeyDown(Keys.B))
            {
                //checks if player has the bomb power up
                if ((PowerupType & PowerupType.Bomb) > 0)
                {
                    if (bombTimer > 1.5f)
                    {
                        ScrollingShooterGame.GameObjectManager.CreateProjectile(ProjectileType.Bomb, position);
                        bombTimer = 0f;
                    }
                }
            }
            if (bladesPowerupTimer > 10.0f && (PowerupType & PowerupType.Blades) > 0)
            {
                unApplyBlades();
            }
            
            // Used to test the energy blast powerup levels
            //if (currentKeyboardState.IsKeyDown(Keys.F) && oldKeyboardState.IsKeyUp(Keys.F))
            //    energyBlastLevel++;

            if ((PowerupType & PowerupType.Blades) == 0)
            {

                // Fire weapons
                if (currentKeyboardState.IsKeyDown(Keys.Space))
                {
                    // Streaming weapons
                    if ((PowerupType & PowerupType.BubbleBeam) > 0)
                    {
                        if (defaultGunTimer > BubbleBullet.FIRE_INTERVAL_MS)
                        {
                            ScrollingShooterGame.GameObjectManager.CreateProjectile(ProjectileType.BubbleBullet, position);
                            defaultGunTimer = 0f;
                        }
                    }

                    // Default gun
                    if (defaultGunTimer > 0.25f)
                    {
                        ScrollingShooterGame.GameObjectManager.CreateProjectile(ProjectileType.Bullet, position);
                        defaultGunTimer = 0f;
                    }
                        
                    // Energy Blast Gun
                    if (((PowerupType & PowerupType.EnergyBlast) > 0) && energyBlastTimer < 0)
                    {
                        TriggerEnergyBlast();
                    }
                    
                    // store the current keyboard state for next frame
                    oldKeyboardState = currentKeyboardState;

                    // Fire-once weapons
                    if (oldKeyboardState.IsKeyUp(Keys.Space))
                    {

                        if ((PowerupType & PowerupType.Fireball) > 0)
                            TriggerFireball();

                         if ((PowerupType & PowerupType.DroneWave) > 0)
                        {
                            TriggerDroneWave();
                        }
                    }

                    if ((PowerupType & PowerupType.Frostball) > 0)
                        TriggerFrostball();

                    if ((PowerupType & PowerupType.Birdcrap) > 0)
                    {
                        TriggerBirdcrap();
                    }
                     if ((PowerupType & PowerupType.Bomb) > 0)
                        TriggerBomb();
                }
            }
                    
            // store the current keyboard state for next frame
            oldKeyboardState = currentKeyboardState;
        }


        /// <summary>
        /// Draw the ship on-screen
        /// </summary>
        /// <param name="elaspedTime">The elapsed time</param>
        /// <param name="spriteBatch">An already-initialized spritebatch, ready for Draw() commands</param>
        public override void Draw(float elaspedTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(spriteSheet, Bounds, spriteBounds[(int)steeringState], Color.White, 0f, new Vector2(Bounds.Width / 2, Bounds.Height / 2), SpriteEffects.None, 1f);
        }


        /// <summary>
        /// A helper function that fires a fireball from the ship, 
        /// corresponding to the fireball powerup
        /// </summary>
        void TriggerFireball()
        {
            ScrollingShooterGame.GameObjectManager.CreateProjectile(ProjectileType.Fireball, position);
        }

        public Vector2 GetPosition()
        {
            return position;
        }
        void TriggerBomb()
        {
            // TODO: Fire Bomb
        }
                /// <summary>
        /// A helper that fires birdcrap from the ship. Coraspondes to the birdcrap power up.
        /// </summary>
        void TriggerBirdcrap()
        {
            //TODO:Create effect and calculations when colides with enemy ship, the event will determin if the enimy is "blind" or if his ship will crash. Also when audio is implemented it will make a pooping sound and splat sounds.
            ScrollingShooterGame.GameObjectManager.CreateProjectile(ProjectileType.BirdCrap, position);
        }

        /// <summary>
        /// A helper function that fires a frostball from the ship, 
        /// corresponding to the frostball powerup
        /// </summary>
        void TriggerFrostball()
        {
            ScrollingShooterGame.GameObjectManager.CreateProjectile(ProjectileType.Frostball, position);
        }

        /// <summary>
        /// A helper function that initializes the blades powerup.
        /// //Puts a giant spinning blade over player position and doubles the players velocity.
        /// </summary>
        void ApplyBlades()
        {
            ScrollingShooterGame.GameObjectManager.CreateProjectile(ProjectileType.Blades, position);
            this.velocity *= 2;
            bladesPowerupTimer = 0;
            //TO DO: make player invulerable for 10 secs, since not implemented yet.
        }

        /// <summary>
        /// A helper function that will remove the Blade powerup and restore defaults.
        /// </summary>
        void unApplyBlades()
        {
            this.PowerupType = this.PowerupType ^= PowerupType.Blades;
            this.velocity /= 2;
            bladesPowerupTimer = 0;
            //TO DO: make player vulerable again, since not implemented yet.

        }


        /// <summary>
        /// Helper function to create an eightballshield around the ship
        /// </summary>
        void TriggerEightBallShield()
        {
            ScrollingShooterGame.GameObjectManager.CreateShield(ShieldType.EightBallShield, position, this);
        }

        /// <summary>
        /// A helper function that initializes the tri-shield.
        /// Creates three trishield balls.
        /// </summary>
        void ApplyTriShield()
        {
            //Create three balls, each at different rotations around the ship
            ScrollingShooterGame.GameObjectManager.CreateProjectile(ProjectileType.TrishieldBall, new Vector2(Bounds.Center.X, Bounds.Center.Y));
        }

        /// <summary>
        /// A helper function that fires a wide drone wave from the ship, corresponding to the fireball powerup.
        /// </summary>
        void TriggerDroneWave()
        {
            ScrollingShooterGame.GameObjectManager.CreateProjectile(ProjectileType.DroneWave, new Vector2(position.X, position.Y));
        }

        /// <summary>
        /// A helper function that fires an energy blast from the ship, 
        /// corresponding to the energy blast powerup
        /// </summary>
        void TriggerEnergyBlast()
        {
            // Set the offset depending on which sprite we are using, note blastWidth is the sprite's width/2 as found in the EnergyBlast class
            int blastWidth = 4;
            energyBlastTimer = 0.5f;
            if (energyBlastLevel == 1)
            {
                blastWidth = 6;
                energyBlastTimer = 0.4f;
            }
            else if (energyBlastLevel == 2)
            {
                blastWidth = 5;
                energyBlastTimer = 0.3f;
            }
            else if (energyBlastLevel >= 3)
            {
                blastWidth = 11;
                energyBlastTimer = 0.25f;
            }
            Vector2 position = new Vector2(this.position.X + this.Bounds.Width / 2 - blastWidth, this.position.Y);

            ScrollingShooterGame.GameObjectManager.CreateProjectile(ProjectileType.EnergyBlast, position);
            //ScrollingShooterGame.Game.projectiles.Add(new EnergyBlast(ScrollingShooterGame.Game.Content, position, energyBlastLevel));
        }
    }
}