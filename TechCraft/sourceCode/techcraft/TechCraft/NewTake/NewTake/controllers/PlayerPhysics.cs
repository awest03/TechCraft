#region License

//  TechCraft - http://techcraft.codeplex.com
//  This source code is offered under the Microsoft Public License (Ms-PL) which is outlined as follows:

//  Microsoft Public License (Ms-PL)
//  This license governs use of the accompanying software. If you use the software, you accept this license. If you do not accept the license, do not use the software.

//  1. Definitions
//  The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under U.S. copyright law.
//  A "contribution" is the original software, or any additions or changes to the software.
//  A "contributor" is any person that distributes its contribution under this license.
//  "Licensed patents" are a contributor's patent claims that read directly on its contribution.

//  2. Grant of Rights
//  (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
//  (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.

//  3. Conditions and Limitations
//  (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
//  (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, your patent license from such contributor to the software ends automatically.
//  (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution notices that are present in the software.
//  (D) If you distribute any portion of the software in source code form, you may do so only under this license by including a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object code form, you may only do so under a license that complies with this license.
//  (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular purpose and non-infringement. 
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using NewTake.model;
using NewTake.view;
using NewTake.view.blocks;
#endregion

namespace NewTake.controllers
{
   public class PlayerPhysics
   {

       #region Fields
       private readonly Player player;
       private readonly FirstPersonCamera camera;

       const float PLAYERJUMPVELOCITY = 6f;
       const float PLAYERGRAVITY = -15f;
       const float PLAYERMOVESPEED = 3.5f;
       #endregion

       public PlayerPhysics(PlayerRenderer playerRenderer)
       {
           this.player = playerRenderer.player;
           this.camera = playerRenderer.camera;
       }

       public void move(GameTime gameTime)
       {
           UpdatePosition(gameTime);

           float headbobOffset = (float)Math.Sin(player.headBob) * 0.06f;
           camera.Position =  player.position + new Vector3(0, 0.15f + headbobOffset, 0);
       }

       #region UpdatePosition
       private void UpdatePosition(GameTime gameTime)
       {
            player.velocity.Y += PLAYERGRAVITY * (float)gameTime.ElapsedGameTime.TotalSeconds;
            Vector3 footPosition = player.position + new Vector3(0f, -1.5f, 0f);
            Vector3 headPosition = player.position + new Vector3(0f, 0.1f, 0f);
            
            //TODO _isAboveSnowline = headPosition.Y > WorldSettings.SNOWLINE;

            if (BlockInformation.IsSolidBlock(player.world.BlockAt(footPosition).Type) || BlockInformation.IsSolidBlock(player.world.BlockAt(headPosition).Type))
            {
                BlockType standingOnBlock = player.world.BlockAt(footPosition).Type;
                BlockType hittingHeadOnBlock = player.world.BlockAt(headPosition).Type;

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
                if (BlockInformation.IsSolidBlock(player.world.BlockAt(headPosition).Type))
                {
                    int blockIn = (int)(headPosition.Y);
                    player.position.Y = (float)(blockIn - 0.15f);
                }

                // If the player is stuck in the ground, bring them out.
                // This happens because we're standing on a block at -1.5, but stuck in it at -1.4, so -1.45 is the sweet spot.
                if (BlockInformation.IsSolidBlock(player.world.BlockAt(footPosition).Type))
                {
                    int blockOn = (int)(footPosition.Y);
                    player.position.Y = (float)(blockOn + 1 + 1.45);
                }

                player.velocity.Y = 0;

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

            player.position += player.velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;

            KeyboardState kstate = Keyboard.GetState();

            Vector3 moveVector = new Vector3();

            if (kstate.IsKeyDown(Keys.Space))
            {
                if (BlockInformation.IsSolidBlock(player.world.BlockAt(footPosition).Type) && player.velocity.Y == 0)
                {
                    player.velocity.Y = PLAYERJUMPVELOCITY;
                    float amountBelowSurface = ((ushort)footPosition.Y) + 1 - footPosition.Y;
                    player.position.Y += amountBelowSurface + 0.01f;
                }
            }

            if (kstate.IsKeyDown(Keys.W))
            {
                moveVector += Vector3.Forward * 2;
            }
            if (kstate.IsKeyDown(Keys.S))
            {
                moveVector += Vector3.Backward * 2;
            }
            if (kstate.IsKeyDown(Keys.A))
            {
                moveVector += Vector3.Left * 2;
            }
            if (kstate.IsKeyDown(Keys.D))
            {
                moveVector += Vector3.Right * 2;
            }

            //moveVector.Normalize();
            moveVector *= PLAYERMOVESPEED * (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector3 rotatedMoveVector = Vector3.Transform(moveVector, Matrix.CreateRotationY(camera.LeftRightRotation));

            // Attempt to move, doing collision stuff.
            if (TryToMoveTo(rotatedMoveVector, gameTime)) { }
            else if (!TryToMoveTo(new Vector3(0, 0, rotatedMoveVector.Z), gameTime)) { }
            else if (!TryToMoveTo(new Vector3(rotatedMoveVector.X, 0, 0), gameTime)) { }
       }
       #endregion

       #region TryToMoveTo
       private bool TryToMoveTo(Vector3 moveVector, GameTime gameTime)
       {
            // Build a "test vector" that is a little longer than the move vector.
            float moveLength = moveVector.Length();
            Vector3 testVector = moveVector;
            testVector.Normalize();
            testVector = testVector * (moveLength + 0.3f);

            // Apply this test vector.
            Vector3 movePosition = player.position + testVector;
            Vector3 midBodyPoint = movePosition + new Vector3(0, -0.7f, 0);
            Vector3 lowerBodyPoint = movePosition + new Vector3(0, -1.4f, 0);

            if (!BlockInformation.IsSolidBlock(player.world.BlockAt(movePosition).Type) && !BlockInformation.IsSolidBlock(player.world.BlockAt(lowerBodyPoint).Type) && !BlockInformation.IsSolidBlock(player.world.BlockAt(midBodyPoint).Type))
            {
                player.position = player.position + moveVector;
                if (moveVector != Vector3.Zero)
                {
                    player.headBob += 0.2;
                }
                return true;
            }

            // It's solid there, so while we can't move we have officially collided with it.
            BlockType lowerBlock = player.world.BlockAt(lowerBodyPoint).Type;
            BlockType midBlock = player.world.BlockAt(midBodyPoint).Type;
            BlockType upperBlock = player.world.BlockAt(movePosition).Type;

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
        #endregion

   }
}
