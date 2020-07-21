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
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

using NewTake.model;
#endregion

namespace NewTake.view
{
    public class FirstPersonCamera : Camera
    {
        #region Fields
        private const float _rotationSpeed = 0.05f;
        private float _leftRightRotation = 0f;
        private float _upDownRotation = 0f;
        private Vector3 _cameraFinalTarget;
        private Vector3 _lookVector;
        #endregion

        public FirstPersonCamera(Viewport viewport) : base(viewport) { }

        #region Initialize
        public override void Initialize()
        {
            _upDownRotation = 0;
            _leftRightRotation = 0;

            base.Initialize();
        }
        #endregion

        public Vector3 Target
        {
            get { return _cameraFinalTarget; }
        }

        public float LeftRightRotation
        {
            get { return _leftRightRotation; }
            set
            {
                _leftRightRotation = value;
                CalculateView();
            }
        }

        public float UpDownRotation
        {
            get { return _upDownRotation; }
            set
            {
                _upDownRotation = value;
                CalculateView();
            }
        }

        #region CalculateView
        protected override void CalculateView()
        {
            Matrix rotationMatrix = Matrix.CreateRotationX(_upDownRotation) * Matrix.CreateRotationY(_leftRightRotation);
            _lookVector = Vector3.Transform(Vector3.Forward, rotationMatrix);

            _cameraFinalTarget = Position + _lookVector;
           
            Vector3 cameraRotatedUpVector = Vector3.Transform(Vector3.Up, rotationMatrix);
            View = Matrix.CreateLookAt(Position, _cameraFinalTarget, cameraRotatedUpVector);

            base.CalculateView();
        }
        #endregion

        public void LookAt(Vector3 target)
        {
            // Doesn't take into account the rotated UP vector
            // Should calculate rotations here!
            View = Matrix.CreateLookAt(Position, target, Vector3.Up);
        }

        public Vector3 LookVector
        {
            get
            {
                return _lookVector;
            }
        }

        #region Update
        public override void Update(GameTime gameTime)
        {
            CalculateView();
            base.Update(gameTime);
        }
        #endregion

        #region FacingCardinal
        public Cardinal FacingCardinal()
        {
            //TODO optimize with modulo (see url)
            //http://gamedev.stackexchange.com/questions/7325/snapping-an-angle-to-the-closest-cardinal-direction

            float a = MathHelper.WrapAngle(_leftRightRotation);
            a = MathHelper.PiOver4 * (float)Math.Round(a / MathHelper.PiOver4);

            if (a == 0)
                return (Cardinal.N);
            else if (a.CompareTo(MathHelper.PiOver4) == 0)
                return (Cardinal.NW);
            else if (a.CompareTo(-MathHelper.PiOver4) == 0)
                return (Cardinal.NE);
            else if (a.CompareTo(MathHelper.Pi - MathHelper.PiOver4) == 0)
                return (Cardinal.SW);
            else if (a.CompareTo(-(MathHelper.Pi - MathHelper.PiOver4)) == 0)
                return (Cardinal.SE);
            else if (a.CompareTo(MathHelper.PiOver2) == 0)
                return (Cardinal.W);
            else if (a.CompareTo(-MathHelper.PiOver2) == 0)
                return (Cardinal.E);
            else if (a.CompareTo(MathHelper.Pi) == 0 || a.CompareTo(-MathHelper.Pi) == 0)
                return (Cardinal.S);
            else
                throw new NotImplementedException();
        }
        #endregion

    }
}
