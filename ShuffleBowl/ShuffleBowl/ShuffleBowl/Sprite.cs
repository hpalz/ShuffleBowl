using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.Collision.Shapes;

namespace ShuffleBowl
{
    public struct Sprite
    {
        private Vector2 origin;
        public Vector2 lastPos;
        private Texture2D texture;
        public Body body;

        public Sprite(Texture2D texture, Vector2 vector, World world,object type)
        {
            this.lastPos = vector;
            this.texture = texture;
            this.origin = new Vector2(texture.Width / 2f, texture.Height / 2f);
            this.body = BodyFactory.CreateCircle(world,ConvertUnits.ToSimUnits(texture.Width / 2f),5f,vector);
            this.body.BodyType = BodyType.Dynamic;
            this.body.Restitution = 1f; //Is is .0? TODO Elasticity
            this.body.Friction = .2f;
            this.body.LinearDamping = .3f;
            this.body.Mass = .5f;
            this.body.CollidesWith = Category.All;
            this.body.UserData = type;
            this.body.BodyType = BodyType.Dynamic;
            this.body.FixedRotation = true;
        }

        public static void Draw(SpriteBatch spritebatch, Sprite sprite, Vector2 position, float rotation)
        {
            spritebatch.Draw(sprite.texture,  ConvertUnits.ToDisplayUnits(position), null, Color.White, rotation, sprite.origin, 1.0f, SpriteEffects.None, 0f);
        }
    }
}
