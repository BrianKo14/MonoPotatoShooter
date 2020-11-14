using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PotatoShooter
{
    class Bullet
    {
        public Texture2D Texture;
        public Vector2 Position;
        public Vector2 Scale;
        public Vector2 Velocity;
        public Vector2 Origin;
        public double RotationSpeed;
        public float RotationAngle;

        public Rectangle Rectangle;
    }
}
