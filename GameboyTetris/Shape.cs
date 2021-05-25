using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameboyTetris
{
    internal class Shape
    {
        public List<Sprite> sprites { private set; get; }

        public bool active { private set; get; }
        private Vector2 origin;
        public int id { private set; get; }
        private int yOffset = 1;
        private int xOffset = 33;

        public Shape(Texture2D texture, int _id)
        {
            sprites = new List<Sprite>();
            sprites.Add(new Sprite(texture, new Vector2(0 + xOffset, 0 + yOffset)));
            sprites.Add(new Sprite(texture, new Vector2(8 + xOffset, 0 + yOffset)));
            sprites.Add(new Sprite(texture, new Vector2(8 + xOffset, 8 + yOffset)));
            sprites.Add(new Sprite(texture, new Vector2(16 + xOffset, 8 + yOffset)));
            origin = new Vector2();
            for (int i = 0; i < sprites.Count; i++)
            {
                origin += sprites[i].position;
            }
            origin /= sprites.Count;
            id = _id;
            active = true;
        }

        public void RotateRight(List<Sprite> colliding)
        {
            if (active)
            {
                for (int i = 0; i < sprites.Count; i++)
                {
                    if (colliding.Any(o => o.BasicIntersects(new Rectangle((int)AdvancedMath.Rotate(sprites[i].position, 90).X, (int)AdvancedMath.Rotate(sprites[i].position, 90).Y, sprites[i].rectangle.Width, sprites[i].rectangle.Height))))
                    {
                        active = false;
                        return;
                    }
                }
                for (int i = 0; i < sprites.Count; i++)
                {
                    sprites[i].position = AdvancedMath.Rotate(sprites[i].position, 90);
                    sprites[i].UpdatePos(0);
                }
                origin = new Vector2();
                for (int i = 0; i < sprites.Count; i++)
                {
                    origin += sprites[i].position;
                }
                origin /= sprites.Count;
            }
        }

        public void RotateRight(List<Shape> colliding)
        {
            if (active)
            {
                for (int i = 0; i < sprites.Count; i++)
                {
                    for (int a = 0; a < colliding.Count; a++)
                    {
                        if (colliding[a].sprites.Any(o => o.BasicIntersects(new Rectangle((int)AdvancedMath.Rotate(sprites[i].position, 90).X, (int)AdvancedMath.Rotate(sprites[i].position, 90).Y, sprites[i].rectangle.Width, sprites[i].rectangle.Height))))
                        {
                            active = false;
                            return;
                        }
                    }
                }
                for (int i = 0; i < sprites.Count; i++)
                {
                    sprites[i].position = AdvancedMath.Rotate(sprites[i].position - origin, 90) + origin;
                    sprites[i].position = new Vector2(AdvancedMath.GetNearestMultiple((int)Math.Round(sprites[i].position.X - xOffset), 8) + xOffset, AdvancedMath.GetNearestMultiple((int)Math.Round(sprites[i].position.Y - yOffset), 8) + yOffset);
                    sprites[i].UpdatePos(0);
                }
                origin = new Vector2();
                for (int i = 0; i < sprites.Count; i++)
                {
                    origin += sprites[i].position;
                }
                origin /= sprites.Count;
            }
        }

        public void RotateLeft(List<Sprite> colliding)
        {
            if (active)
            {
                for (int i = 0; i < sprites.Count; i++)
                {
                    if (colliding.Any(o => o.BasicIntersects(new Rectangle((int)AdvancedMath.Rotate(sprites[i].position, -90).X, (int)AdvancedMath.Rotate(sprites[i].position, -90).Y, sprites[i].rectangle.Width, sprites[i].rectangle.Height))))
                    {
                        active = false;
                        return;
                    }
                }
                for (int i = 0; i < sprites.Count; i++)
                {
                    sprites[i].position = AdvancedMath.Rotate(sprites[i].position, -90);
                    sprites[i].UpdatePos(0);
                }
                origin = new Vector2();
                for (int i = 0; i < sprites.Count; i++)
                {
                    origin += sprites[i].position;
                }
                origin /= sprites.Count;
            }
        }

        public void RotateLeft(List<Shape> colliding)
        {
            if (active)
            {
                for (int i = 0; i < sprites.Count; i++)
                {
                    for (int a = 0; a < colliding.Count; a++)
                    {
                        if (colliding[a].sprites.Any(o => o.BasicIntersects(new Rectangle((int)AdvancedMath.Rotate(sprites[i].position, -90).X, (int)AdvancedMath.Rotate(sprites[i].position, -90).Y, sprites[i].rectangle.Width, sprites[i].rectangle.Height))))
                        {
                            active = false;
                            return;
                        }
                    }
                }
                for (int i = 0; i < sprites.Count; i++)
                {
                    sprites[i].position = AdvancedMath.Rotate(sprites[i].position - origin, -90) + origin;
                    sprites[i].position = new Vector2(AdvancedMath.GetNearestMultiple((int)Math.Round(sprites[i].position.X - xOffset), 8) + xOffset, AdvancedMath.GetNearestMultiple((int)Math.Round(sprites[i].position.Y - yOffset), 8) + yOffset);
                    sprites[i].UpdatePos(0);
                }
                origin = new Vector2();
                for (int i = 0; i < sprites.Count; i++)
                {
                    origin += sprites[i].position;
                }
                origin /= sprites.Count;
            }
        }

        public void MoveLeft(List<Shape> colliding)
        {
            if (active)
            {
                for (int i = 0; i < sprites.Count; i++)
                {
                    for (int a = 0; a < colliding.Count; a++)
                    {
                        if (colliding[a].sprites.Any(o => o.rectangle == (new Rectangle((int)sprites[i].position.X - sprites[i].rectangle.Width, (int)sprites[i].position.Y, sprites[i].rectangle.Width, sprites[i].rectangle.Height))))
                        {
                            return;
                        }
                    }
                    if (sprites[i].position.X - sprites[i].rectangle.Width < 17)
                    {
                        return;
                    }
                }
                for (int i = 0; i < sprites.Count; i++)
                {
                    sprites[i].position.X -= sprites[i].rectangle.Width;
                    sprites[i].UpdatePos(0);
                }
                origin = new Vector2();
                for (int i = 0; i < sprites.Count; i++)
                {
                    origin += sprites[i].position;
                }
                origin /= sprites.Count;
            }
        }

        public void MoveRight(List<Shape> colliding)
        {
            if (active)
            {
                for (int i = 0; i < sprites.Count; i++)
                {
                    for (int a = 0; a < colliding.Count; a++)
                    {
                        if (colliding[a].sprites.Any(o => o.rectangle == (new Rectangle((int)sprites[i].position.X + sprites[i].rectangle.Width, (int)sprites[i].position.Y, sprites[i].rectangle.Width, sprites[i].rectangle.Height))))
                        {
                            return;
                        }
                    }
                    if (sprites[i].position.X + sprites[i].rectangle.Width > 89)
                    {
                        return;
                    }
                }
                for (int i = 0; i < sprites.Count; i++)
                {
                    sprites[i].position.X += sprites[i].rectangle.Width;
                    sprites[i].UpdatePos(0);
                }
                origin = new Vector2();
                for (int i = 0; i < sprites.Count; i++)
                {
                    origin += sprites[i].position;
                }
                origin /= sprites.Count;
            }
        }

        public void Update(List<Sprite> colliding)
        {
            if (active)
            {
                for (int i = 0; i < sprites.Count; i++)
                {
                    if (colliding.Any(o => o.rectangle == (new Rectangle(sprites[i].rectangle.X, sprites[i].rectangle.Y + sprites[i].rectangle.Height, sprites[i].rectangle.Width, sprites[i].rectangle.Height))) || sprites[i].position.Y + sprites[i].rectangle.Height > 137)
                    {
                        active = false;
                        return;
                    }
                }
                for (int i = 0; i < sprites.Count; i++)
                {
                    sprites[i].position.Y += sprites[i].rectangle.Height;
                    sprites[i].UpdatePos(0);
                }
                origin = new Vector2();
                for (int i = 0; i < sprites.Count; i++)
                {
                    origin += sprites[i].position;
                }
                origin /= sprites.Count;
            }
        }

        public void Update(List<Shape> colliding)
        {
            if (active)
            {
                for (int i = 0; i < sprites.Count; i++)
                {
                    for (int a = 0; a < colliding.Count; a++)
                    {
                        if (colliding[a].sprites.Any(o => o.rectangle == (new Rectangle(sprites[i].rectangle.X, sprites[i].rectangle.Y + sprites[i].rectangle.Height, sprites[i].rectangle.Width, sprites[i].rectangle.Height))))
                        {
                            active = false;
                            return;
                        }
                    }
                    if (sprites[i].position.Y + sprites[i].rectangle.Height > 137)
                    {
                        active = false;
                        return;
                    }
                }
                for (int i = 0; i < sprites.Count; i++)
                {
                    sprites[i].position.Y += sprites[i].rectangle.Height;
                    sprites[i].UpdatePos(0);
                }
                origin = new Vector2();
                for (int i = 0; i < sprites.Count; i++)
                {
                    origin += sprites[i].position;
                }
                origin /= sprites.Count;
            }
        }

        public bool AboveBorder()
        {
            for (int i = 0; i < sprites.Count; i++)
            {
                if (sprites[i].position.Y <= 9)
                {
                    return true;
                }
            }
            return false;
        }
    }
}