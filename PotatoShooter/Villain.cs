using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PotatoShooter
{
    class Enemy
    {
        public Texture2D Texture;
        public Vector2 Position;
        public Vector2 Scale;
        public Vector2 Origin;
        public float RotationAngle;

        public Rectangle Rectangle;

        public float VerticalSpeed = 3f; //3
        public float HorizontalSpeed = 3f; //3
        public int ShiftProbability = 50; //50

        public bool isDisappearing = false;

        public bool direction = true; //true=right, false=left
        public void Update()
        {
            Position.Y += VerticalSpeed;
         
            Random rnd = new Random();

            //if (rnd.Next(ShiftProbability) == 0)
            //    direction = !direction;

            if (direction) Position.X += HorizontalSpeed; else Position.X -= HorizontalSpeed;
            
           
        }
    }
}
