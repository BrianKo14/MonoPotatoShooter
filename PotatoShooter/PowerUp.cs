using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PotatoShooter
{
    class PowerUp
    {

        public Texture2D Texture;
        public Vector2 Position;
        public Vector2 Scale;
        public Vector2 Origin;
        public float RotationAngle;

        public Rectangle Rectangle;

        public float VerticalSpeed = 6f; //3
        public float HorizontalSpeed = 6f; //3
        public int ShiftProbability = 50; //50

        public int Type; // From 0 fowards
        public bool Activated = false;
        public double Timer = 0;
        public double Duration;

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
