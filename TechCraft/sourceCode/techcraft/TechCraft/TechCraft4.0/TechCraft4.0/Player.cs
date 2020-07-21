using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

using TechCraftEngine;
using TechCraftEngine.WorldEngine;
using TechCraftEngine.Cameras;
using TechCraft.States;
using TechCraftEngine.Particles;
using TechCraft.ParticleSystems;

namespace TechCraft
{
    public class Player
    {
        private TechCraftGame _game;
        private World _world;
        private Vector3 _position;
        private Vector3 _playerVelocity;
        private double _headBob;
        private PlayingState _state;

        private BubbleParticleSystem _bubbleParticleSystem;
        private SnowParticleSystem _snowParticleSystem;
        private SoundEffect _bubbleSound;
        private SoundEffectInstance _bubbleSoundInstance;
        private SoundEffect _splashSound;
        private bool _wasUnderwater;
        private bool _isAboveSnowline;
        private MouseState _previousMouseState;

        public Player(TechCraftGame game, PlayingState state, World world, Vector3 startPosition)
        {
            _game = game;
            _world = world;
            _position = startPosition;
            _state = state;
        }

        public Vector3 Position
        {
            get { return _position; }
        }

        public FirstPersonCamera Camera
        {
            get
            {
                return (FirstPersonCamera)_game.Camera;
            }
        }

        public void Initialize()
        {
            _playerVelocity = Vector3.Zero;
            _bubbleParticleSystem = new BubbleParticleSystem(_game, _game.Content);
            _bubbleParticleSystem.Initialize();
            _snowParticleSystem = new SnowParticleSystem(_game, _game.Content);
            _snowParticleSystem.Initialize();
        }

        public void LoadContent()
        {
            _splashSound = _game.Content.Load<SoundEffect>("Sounds\\splash");
            _bubbleSound = _game.Content.Load<SoundEffect>("Sounds\\bubbles");
            _bubbleSoundInstance = _bubbleSound.CreateInstance();
            _bubbleSoundInstance.IsLooped = true;
            _bubbleSoundInstance.Play(); _bubbleSoundInstance.Pause();
            _bubbleSoundInstance.Volume = 0.2f;
            _bubbleSoundInstance.Pitch = -1f;
        }

        Matrix _rotationMatrix;
        Vector3 _lookVector;
        Random r = new Random();

        public bool IsAboveSnowLine
        {
            get { return _isAboveSnowline; }
        }

        private void UpdateParticles(GameTime gameTime)
        {
            if (IsUnderWater)
            {
                _bubbleParticleSystem.SetCamera(_game.Camera.View, _game.Camera.Projection);
                _bubbleParticleSystem.Update(gameTime);
                Vector3 offset = new Vector3((float)(r.NextDouble() - r.NextDouble()) * 5, (float)(r.NextDouble() - r.NextDouble()) * 5, (float)(r.NextDouble() - r.NextDouble()) * 5);
                _bubbleParticleSystem.AddParticle(_game.Camera.Position + offset, Vector3.Zero);
            }
            if (IsAboveSnowLine)
            {
                _snowParticleSystem.SetCamera(_game.Camera.View, _game.Camera.Projection);
                _snowParticleSystem.Update(gameTime);
                Vector3 offset = new Vector3((float)(r.NextDouble() - r.NextDouble()) * 5, (float)(r.NextDouble()) * 5, (float)(r.NextDouble() - r.NextDouble()) * 5);
                _snowParticleSystem.AddParticle(_game.Camera.Position + offset, Vector3.Zero);
                _snowParticleSystem.AddParticle(_game.Camera.Position + offset, Vector3.Zero);
            }
        }

        private void UpdateSounds()
        {
            bool underWater = IsUnderWater;

            if (underWater && !_wasUnderwater)
            {
                _splashSound.Play(0.2f, 0f, 0);
                _bubbleSoundInstance.Resume();
            }
            if (!underWater && _wasUnderwater)
            {
                _splashSound.Play(0.2f, 0f, 0);
                _bubbleSoundInstance.Pause();
            }
            _wasUnderwater = underWater;
        }

        public Vector3 LookVector
        {
            get { return _lookVector; }
        }

        public void Update(GameTime gameTime)
        {

            _rotationMatrix = Matrix.CreateRotationX(Camera.UpDownRotation) * Matrix.CreateRotationY(Camera.LeftRightRotation);
            _lookVector = Vector3.Transform(Vector3.Forward, _rotationMatrix);
            _lookVector.Normalize();

            UpdateParticles(gameTime);
            UpdateSounds();

            PlayerIndex controlIndex;

            if (_game.InputState.IsButtonDown(Buttons.A, _game.ActivePlayerIndex, out controlIndex) ||
                _game.InputState.IsKeyDown(Keys.Space, _game.ActivePlayerIndex, out controlIndex))
            {
                Vector3 footPosition = _position + new Vector3(0f, -1.5f, 0f);
                //XXX fly mode
                //if (_world.SolidAtPoint(footPosition) &&  _playerVelocity.Y == 0)
                //{
                _playerVelocity.Y = WorldSettings.PLAYERJUMPVELOCITY;
                float amountBelowSurface = ((ushort)footPosition.Y) + 1 - footPosition.Y;
                _position.Y += amountBelowSurface + 0.01f;
                //}
            }
            UpdatePosition(gameTime);

            float headbobOffset = (float)Math.Sin(_headBob) * 0.06f;
            Camera.Position = _position + new Vector3(0, 0.15f + headbobOffset, 0);

            CheckBuild(gameTime);

            _previousMouseState = Mouse.GetState();
        }

        public void CheckBuild(GameTime gameTime)
        {
            PlayerIndex controlIndex;

            MouseState mouseState = Mouse.GetState();

            if (_game.InputState.IsButtonPressed(Buttons.RightTrigger, _game.ActivePlayerIndex, out controlIndex) ||
                _game.InputState.IsKeyPressed(Keys.Q, _game.ActivePlayerIndex, out controlIndex)
                || (mouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton != ButtonState.Pressed))
            {
                for (float x = 0.5f; x < 5f; x += 0.2f)
                {
                    Vector3 targetPoint = Camera.Position + (_lookVector * x);
                    BlockType blockType = _world.BlockAtPoint(targetPoint);
                    if (blockType != BlockType.None && blockType != BlockType.Water)
                    {
                        if (targetPoint.Y > 2)
                        {
                            // Can't dig water or lava
                            BlockType targetType = _world.BlockAtPoint(targetPoint);
                            if (BlockInformation.IsDiggable(targetType))
                            {
                                _world.RemoveBlock((ushort)targetPoint.X, (ushort)targetPoint.Y, (ushort)targetPoint.Z);
                            }
                        }
                        break;
                    }
                }
            }
            if (_game.InputState.IsKeyPressed(Keys.V, _game.ActivePlayerIndex, out controlIndex))
            {
                for (float x = 0.5f; x < 5f; x += 0.2f)
                {
                    Vector3 targetPoint = Camera.Position + (_lookVector * x);
                    if (_world.BlockAtPoint(targetPoint) != BlockType.None)
                    {
                        Random r = new Random();
                        for (ushort dy = (ushort)(targetPoint.Y - 3); dy < (ushort)(targetPoint.Y + 3); dy++)
                        {
                            for (ushort dx = (ushort)(targetPoint.X - 3); dx < (ushort)(targetPoint.X + 3); dx++)
                            {
                                for (ushort dz = (ushort)(targetPoint.Z - 3); dz < (ushort)(targetPoint.Z + 3); dz++)
                                {
                                    if (r.Next(3) == 0) _world.RemoveBlock(dx, dy, dz);
                                }
                            }
                        }
                        break;
                    }
                }
            }
            if (_game.InputState.IsButtonPressed(Buttons.LeftTrigger, _game.ActivePlayerIndex, out controlIndex) ||
                _game.InputState.IsKeyPressed(Keys.E, _game.ActivePlayerIndex, out controlIndex)
                || (mouseState.RightButton == ButtonState.Pressed
                && _previousMouseState.RightButton != ButtonState.Pressed))
            {
                float hit = 0;
                for (float x = 0.8f; x < 5f; x += 0.1f)
                {
                    Vector3 targetPoint = Camera.Position + (_lookVector * x);
                    if (_world.BlockAtPoint(targetPoint) != BlockType.None)
                    {
                        hit = x;
                        break;
                    }
                }
                if (hit != 0)
                {
                    for (float x = hit; x > 0.7f; x -= 0.1f)
                    {
                        Vector3 targetPoint = Camera.Position + (_lookVector * x);
                        if (_world.BlockAtPoint(targetPoint) == BlockType.None)
                        {
                            _world.AddBlock((ushort)targetPoint.X, (ushort)targetPoint.Y, (ushort)targetPoint.Z, _state.BlockPicker.SelectedBlockType, true, true);
                            break;
                        }
                    }
                }
            }
        }

        public bool IsUnderWater
        {
            get
            {
                return (_world.BlockAtPoint(_game.Camera.Position) == BlockType.Water);
            }
        }

        private void UpdatePosition(GameTime gameTime)
        {
            _playerVelocity.Y += WorldSettings.PLAYERGRAVITY * (float)gameTime.ElapsedGameTime.TotalSeconds;
            Vector3 footPosition = _position + new Vector3(0f, -1.5f, 0f);
            Vector3 headPosition = _position + new Vector3(0f, 0.1f, 0f);
            _isAboveSnowline = headPosition.Y > WorldSettings.SNOWLINE;
            if (_world.SolidAtPoint(footPosition) || _world.SolidAtPoint(headPosition))
            {
                BlockType standingOnBlock = _world.BlockAtPoint(footPosition);
                BlockType hittingHeadOnBlock = _world.BlockAtPoint(headPosition);

                // If we"re hitting the ground with a high velocity, die!
                //if (standingOnBlock != BlockType.None && _P.playerVelocity.Y < 0)
                //{
                //    float fallDamage = Math.Abs(_P.playerVelocity.Y) / DIEVELOCITY;
                //    if (fallDamage >= 1)
                //    {
                //        _P.PlaySoundForEveryone(InfiniminerSound.GroundHit, _P.playerPosition);
                //        _P.KillPlayer(Defines.deathByFall);//"WAS KILLED BY GRAVITY!");
                //        return;
                //    }
                //    else if (fallDamage > 0.5)
                //    {
                //        // Fall damage of 0.5 maps to a screenEffectCounter value of 2, meaning that the effect doesn't appear.
                //        // Fall damage of 1.0 maps to a screenEffectCounter value of 0, making the effect very strong.
                //        if (standingOnBlock != BlockType.Jump)
                //        {
                //            _P.screenEffect = ScreenEffect.Fall;
                //            _P.screenEffectCounter = 2 - (fallDamage - 0.5) * 4;
                //            _P.PlaySoundForEveryone(InfiniminerSound.GroundHit, _P.playerPosition);
                //        }
                //    }
                //}

                // If the player has their head stuck in a block, push them down.
                if (_world.SolidAtPoint(headPosition))
                {
                    int blockIn = (int)(headPosition.Y);
                    _position.Y = (float)(blockIn - 0.15f);
                }

                // If the player is stuck in the ground, bring them out.
                // This happens because we're standing on a block at -1.5, but stuck in it at -1.4, so -1.45 is the sweet spot.
                if (_world.SolidAtPoint(footPosition))
                {
                    int blockOn = (int)(footPosition.Y);
                    _position.Y = (float)(blockOn + 1 + 1.45);
                }

                _playerVelocity.Y = 0;

                // Logic for standing on a block.
                // switch (standingOnBlock)
                //  {
                //case BlockType.Jump:
                //    _P.playerVelocity.Y = 2.5f * JUMPVELOCITY;
                //    _P.PlaySoundForEveryone(InfiniminerSound.Jumpblock, _P.playerPosition);
                //    break;

                //case BlockType.Road:
                //    movingOnRoad = true;
                //    break;

                //case BlockType.Lava:
                //    _P.KillPlayer(Defines.deathByLava);
                //    return;
                //  }

                // Logic for bumping your head on a block.
                // switch (hittingHeadOnBlock)
                // {
                //case BlockType.Shock:
                //    _P.KillPlayer(Defines.deathByElec);
                //    return;

                //case BlockType.Lava:
                //    _P.KillPlayer(Defines.deathByLava);
                //    return;
                //}
            }

            // Death by falling off the map.
            //if (_P.playerPosition.Y < -30)
            //{
            //    _P.KillPlayer(Defines.deathByMiss);
            //    return;
            //}

            _position += _playerVelocity * (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector3 moveVector = new Vector3(
                GamePad.GetState(_game.ActivePlayerIndex).ThumbSticks.Left.X,
                0,
                -GamePad.GetState(_game.ActivePlayerIndex).ThumbSticks.Left.Y);

            PlayerIndex controlIndex;
            if (_game.InputState.IsKeyDown(Keys.W, _game.ActivePlayerIndex, out controlIndex))
            {
                moveVector += Vector3.Forward * 2;
            }
            if (_game.InputState.IsKeyDown(Keys.S, _game.ActivePlayerIndex, out controlIndex))
            {
                moveVector += Vector3.Backward * 2;
            }
            if (_game.InputState.IsKeyDown(Keys.A, _game.ActivePlayerIndex, out controlIndex))
            {
                moveVector += Vector3.Left * 2;
            }
            if (_game.InputState.IsKeyDown(Keys.D, _game.ActivePlayerIndex, out controlIndex))
            {
                moveVector += Vector3.Right * 2;
            }

            //moveVector.Normalize();
            moveVector *= WorldSettings.PLAYERMOVESPEED * (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector3 rotatedMoveVector = Vector3.Transform(moveVector, Matrix.CreateRotationY(Camera.LeftRightRotation));

            // Attempt to move, doing collision stuff.
            if (TryToMoveTo(rotatedMoveVector, gameTime)) { }
            else if (!TryToMoveTo(new Vector3(0, 0, rotatedMoveVector.Z), gameTime)) { }
            else if (!TryToMoveTo(new Vector3(rotatedMoveVector.X, 0, 0), gameTime)) { }
        }

        private bool TryToMoveTo(Vector3 moveVector, GameTime gameTime)
        {
            // Build a "test vector" that is a little longer than the move vector.
            float moveLength = moveVector.Length();
            Vector3 testVector = moveVector;
            testVector.Normalize();
            testVector = testVector * (moveLength + 0.3f);

            // Apply this test vector.
            Vector3 movePosition = _position + testVector;
            Vector3 midBodyPoint = movePosition + new Vector3(0, -0.7f, 0);
            Vector3 lowerBodyPoint = movePosition + new Vector3(0, -1.4f, 0);

            if (!_world.SolidAtPoint(movePosition) && !_world.SolidAtPoint(lowerBodyPoint) && !_world.SolidAtPoint(midBodyPoint))
            {
                _position = _position + moveVector;
                if (moveVector != Vector3.Zero)
                {
                    _headBob += 0.2;
                }
                return true;
            }

            // It's solid there, so while we can't move we have officially collided with it.
            BlockType lowerBlock = _world.BlockAtPoint(lowerBodyPoint);
            BlockType midBlock = _world.BlockAtPoint(midBodyPoint);
            BlockType upperBlock = _world.BlockAtPoint(movePosition);

            // It's solid there, so see if it's a lava block. If so, touching it will kill us!
            //if (upperBlock == BlockType.Lava || lowerBlock == BlockType.Lava || midBlock == BlockType.Lava)
            //{
            //    _P.KillPlayer(Defines.deathByLava);
            //    return true;
            //}

            // If it's a ladder, move up.
            //if (upperBlock == BlockType.Ladder || lowerBlock == BlockType.Ladder || midBlock == BlockType.Ladder)
            //{
            //    _P.playerVelocity.Y = CLIMBVELOCITY;
            //    Vector3 footPosition = _P.playerPosition + new Vector3(0f, -1.5f, 0f);
            //    if (_P.blockEngine.SolidAtPointForPlayer(footPosition))
            //        _P.playerPosition.Y += 0.1f;
            //    return true;
            //}

            return false;
        }

        public void Draw(GameTime gameTime)
        {
            if (IsUnderWater)
            {
                _bubbleParticleSystem.Draw(gameTime);
            }
            _snowParticleSystem.Draw(gameTime);
        }
    }
}
