using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace MazeGame
{
    public class Camera
    {
        public float FOVLevel1 = MathHelper.PiOver4;
        public float FOVLevel2 = MathHelper.ToRadians(75);
        public float FOVLevel3 = MathHelper.PiOver2;
        public float nearClip = 0.05f;
        public float farClip = 100f;
        public Vector3 startingPosition = new Vector3(0.5f, 0.5f, 0.5f);

        public float[] FOVLevels;
        public int currentFOVLevel;
        public float aspectRatio;

        private Vector3 position = Vector3.Zero;
        private float leftRightRotation;
        private float upDownRotation;
        private Vector3 lookAt;
        private Vector3 baseCameraReference = new Vector3(0, 0, 1);
        private bool needViewResync = true;
        private Matrix viewMatrix;

        public Vector3 Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
                UpdateLookAt();
            }
        }

        public float LeftRightRotation
        {
            get
            {
                return leftRightRotation;
            }
            set
            {
                leftRightRotation = value;
                UpdateLookAt();
            }
        }

        public Vector3 LookAt
        {
            get
            {
                //lookAt.Normalize();
                return lookAt;
            }
        }

        public float UpDownRotation
        {
            get
            {
                return upDownRotation;
            }
            set{
                upDownRotation = value;
                UpdateLookAt();
            }
        }

        public Matrix Projection { get; set; }

        public Matrix View
        {
            get
            {
                if (needViewResync)
                {
                    viewMatrix = Matrix.CreateLookAt(Position, lookAt, Vector3.Up);
                }
                return viewMatrix;
            }
        }

        public Camera(float aspectRatio)
        {
            FOVLevels = new float[3] { FOVLevel1, FOVLevel2, FOVLevel3 };
            currentFOVLevel = 0;
            this.aspectRatio = aspectRatio;

            Projection = Matrix.CreatePerspectiveFieldOfView(FOVLevels[currentFOVLevel],
                aspectRatio, nearClip, farClip);
            MoveTo(startingPosition, leftRightRotation, upDownRotation);
        }

        private void UpdateLookAt()
        {
            Matrix rotationMatrix = Matrix.CreateRotationX(upDownRotation) * Matrix.CreateRotationY(leftRightRotation);
            Vector3 lookAtOffset = Vector3.Transform(baseCameraReference, rotationMatrix);
            lookAt = position + lookAtOffset;
            needViewResync = true;
        }

        public Vector3 PreviewMove(float scale)
        {
            Matrix rotate = Matrix.CreateRotationY(leftRightRotation);
            Vector3 forward = new Vector3(0, 0, scale);
            forward = Vector3.Transform(forward, rotate);
            return position + forward;
        }

        public void MoveForward(float scale)
        {
            MoveTo(PreviewMove(scale), leftRightRotation, upDownRotation);
        }

        public void MoveTo(Vector3 position, float leftRightRotation, float upDownRotation)
        {
            this.position = position;
            this.leftRightRotation = leftRightRotation;
            this.upDownRotation = upDownRotation;
            UpdateLookAt();
        }

        public void UpdateProjection()
        {
            Projection = Matrix.CreatePerspectiveFieldOfView(FOVLevels[currentFOVLevel],
                aspectRatio, nearClip, farClip);
        }
    }
}