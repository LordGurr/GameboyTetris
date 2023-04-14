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

        private Sprite[] blocks;

        public bool active { private set; get; }
        private Vector2 origin;
        public Vector2 AccessOrigin { get => origin; }

        public int id { private set; get; }
        private int yOffset = 1;
        private int xOffset = 41;

        private int shape;

        //private static Sprite[][] AllShapes = new Sprite[][]
        //{
        //     new Sprite[]
        //     {
        //         new Sprite(null, new Vector2(0,0)),
        //         new Sprite(null, new Vector2(0,8)),
        //         new Sprite(null, new Vector2(0,16)),
        //         new Sprite(null, new Vector2(0,24)),
        //     },
        //     new Sprite[]
        //     {
        //          new Sprite(null, new Vector2(0,0)),
        //          new Sprite(null, new Vector2(16,0)),
        //          new Sprite(null, new Vector2(8,8)),
        //          new Sprite(null, new Vector2(16,16)),
        //     },
        //     new Sprite[]
        //     {
        //          new Sprite(null, new Vector2(0,0)),
        //          new Sprite(null, new Vector2(8,0)),
        //          new Sprite(null, new Vector2(16,0)),
        //          new Sprite(null, new Vector2(16,8)),
        //     },
        //     new Sprite[]
        //     {
        //          new Sprite(null, new Vector2(0,0)),
        //          new Sprite(null, new Vector2(0,8)),
        //          new Sprite(null, new Vector2(8,0)),
        //          new Sprite(null, new Vector2(16,0)),
        //     },
        //     new Sprite[]
        //     {
        //         new Sprite(null, new Vector2(0, 0)),
        //         new Sprite(null, new Vector2(8, 0)),
        //         new Sprite(null, new Vector2(8, 8)),
        //         new Sprite(null, new Vector2(16, 8)),
        //     },
        //     new Sprite[]
        //     {
        //         new Sprite(null, new Vector2(0,0)),
        //         new Sprite(null, new Vector2(8,0)),
        //         new Sprite(null, new Vector2(16,0)),
        //         new Sprite(null, new Vector2(8,8)),
        //     },
        //     new Sprite[]
        //     {
        //         new Sprite(null, new Vector2(8,0)),
        //         new Sprite(null, new Vector2(16,0)),
        //         new Sprite(null, new Vector2(16,8)),
        //         new Sprite(null, new Vector2(24,8)),
        //     },
        //};

        private static Vector2Int[][] AllShapes = new Vector2Int[][]
        {
             new Vector2Int[] // Long
             {
                 new Vector2Int(2,0),
                 new Vector2Int(2,1),
                 new Vector2Int(2,2),
                 new Vector2Int(2,3),
             },
             new Vector2Int[] // Right hook
             {
                  new Vector2Int(1,0),
                  new Vector2Int(1,1),
                  new Vector2Int(1,2),
                  new Vector2Int(2,2),
             },
             new Vector2Int[] // Left hook
             {
                  new Vector2Int(2,0),
                  new Vector2Int(2,1),
                  new Vector2Int(2,2),
                  new Vector2Int(1,2),
             },
             new Vector2Int[] // Right twist
             {
                 new Vector2Int(1,0),
                 new Vector2Int(1,1),
                 new Vector2Int(2,1),
                 new Vector2Int(2,2),
             },
             new Vector2Int[] // Triangle
             {
                 new Vector2Int(2, 0),
                 new Vector2Int(2, 1),
                 new Vector2Int(1, 1),
                 new Vector2Int(2, 2),
             },
             new Vector2Int[] // Left twist
             {
                 new Vector2Int(2,0),
                 new Vector2Int(1,1),
                 new Vector2Int(2,1),
                 new Vector2Int(1,2),
             },
             new Vector2Int[] // Cube
             {
                 new Vector2Int(1,1),
                 new Vector2Int(2,1),
                 new Vector2Int(1,2),
                 new Vector2Int(2,2),
             },
        };

        private static Vector2[] Origins = new Vector2[]
        {
            new Vector2(1.5f,1.5f),// Long
            new Vector2(1,1),// Right Hook
            new Vector2(2,1), // Left Hook
            new Vector2(2,1), // Right Twist
            new Vector2(-1,-1), //new Vector2(2,1), // Triangle
            new Vector2(2,1), // Left Twist
            new Vector2(-1,-1)  //new Vector2(2.5f,2.5f),//Kub
        };

        public Shape(Shape shape)
        {
            sprites = new List<Sprite>();
            for (int i = 0; i < shape.sprites.Count; i++)
            {
                sprites.Add(new Sprite(shape.sprites[i].tex, shape.sprites[i].position));
                sprites[^1].AccessColor = Color.White * 0.4f;
            }
            origin = shape.origin;
            active = true;
        }

        public Shape(Texture2D texture, int _id, Random rng, int _shape)
        {
            sprites = new List<Sprite>();
            //sprites.Add(new Sprite(texture, new Vector2(0 + xOffset, 0 + yOffset)));
            //sprites.Add(new Sprite(texture, new Vector2(8 + xOffset, 0 + yOffset)));
            //sprites.Add(new Sprite(texture, new Vector2(8 + xOffset, 8 + yOffset)));
            //sprites.Add(new Sprite(texture, new Vector2(16 + xOffset, 8 + yOffset)));
            blocks = new Sprite[16];

            shape = _shape;
            for (int i = 0; i < AllShapes[shape].Length; i++)
            {
                sprites.Add(new Sprite(texture, AllShapes[shape][i].ToVector2()));
                blocks[AllShapes[shape][i].X + AllShapes[shape][i].Y * 4] = sprites[^1];
            }
            //sprites = AllShapes[rng.Next(AllShapes.Length)].ToList();
            for (int i = 0; i < sprites.Count; i++)
            {
                sprites[i].position *= 8;
                sprites[i].position += new Vector2(xOffset, yOffset);
            }
            origin = new Vector2();
            for (int i = 0; i < sprites.Count; i++)
            {
                origin += sprites[i].position;
            }
            origin /= sprites.Count;
            if (Origins[shape].X > -1)
            {
                origin = Origins[shape];
                origin *= 8;
                origin += new Vector2(xOffset, yOffset);
            }

            id = _id;
            active = true;
        }

        private int Rotate(int px, int py, int r)
        {
            switch (r % 4)
            {
                case 0: return py * 4 + px;         // 0 degrees
                case 1: return 12 + py - (px * 4);  // 90 degrees
                case 2: return 15 - (py * 4) - px;  // 180 degrees
                case 3: return 3 - py + (px * 4);   // 270 degrees
            }
            return 0;
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

        //public void rotateLeft()
        //{
        //    Vector2[] rotatedCoordinates = new Point[MAX_COORDINATES];

        //    for (int i = 0; i < MAX_COORDINATES; i++)
        //    {
        //        // Translates current coordinate to be relative to (0,0)
        //        Point translationCoordinate = new Vector2(coordinates[i].x - origin.x, coordinates[i].y - origin.y);

        //        // Java coordinates start at 0 and increase as a point moves down, so
        //        // multiply by -1 to reverse
        //        translationCoordinate.y *= -1;

        //        // Clone coordinates, so I can use translation coordinates
        //        // in upcoming calculation
        //        rotatedCoordinates[i] = (Vector2)translationCoordinate.clone();

        //        // May need to round results after rotation
        //        rotatedCoordinates[i].x = (int)Math.round(translationCoordinate.x * Math.cos(Math.PI / 2) - translationCoordinate.y * Math.sin(Math.PI / 2));
        //        rotatedCoordinates[i].y = (int)Math.round(translationCoordinate.x * Math.sin(Math.PI / 2) + translationCoordinate.y * Math.cos(Math.PI / 2));

        //        // Multiply y-coordinate by -1 again
        //        rotatedCoordinates[i].y *= -1;

        //        // Translate to get new coordinates relative to
        //        // original origin
        //        rotatedCoordinates[i].x += origin.x;
        //        rotatedCoordinates[i].y += origin.y;

        //        // Erase the old coordinates by making them black
        //        matrix.fillCell(coordinates[i].x, coordinates[i].y, Color.black);

        //    }
        //    // Set new coordinates to be drawn on screen
        //    setCoordinates(rotatedCoordinates.clone());
        //}

        public void RotateRight(List<Shape> colliding)
        {
            if (active)
            {
                Sprite[] tempBlocks = new Sprite[blocks.Length];
                for (int i = 0; i < blocks.Length; i++)
                {
                    tempBlocks[Rotate(i / 4, i % 4, 1)] = blocks[i];
                    Vector2 temp = new Vector2(i / 4, i % 4);
                }

                for (int i = 0; i < sprites.Count; i++)
                {
                    Vector2 temp = AdvancedMath.Rotate(sprites[i].position - origin, 90) + origin;
                    temp = new Vector2(AdvancedMath.GetNearestMultiple((int)Math.Round(temp.X - xOffset, MidpointRounding.AwayFromZero), 8) + xOffset, AdvancedMath.GetNearestMultiple((int)Math.Round(temp.Y - yOffset, MidpointRounding.AwayFromZero), 8) + yOffset);
                    if (temp.X < 17 || temp.X > 89 || temp.Y > 137)
                    {
                        return;
                    }
                    for (int a = 0; a < colliding.Count; a++)
                    {
                        if (colliding[a].sprites.Any(o => o.position == temp))
                        {
                            return;
                        }
                    }
                }
                for (int i = 0; i < sprites.Count; i++)
                {
                    sprites[i].position = AdvancedMath.Rotate(sprites[i].position - origin, 90) + origin;
                    sprites[i].position = new Vector2(AdvancedMath.GetNearestMultiple((int)Math.Round(sprites[i].position.X - xOffset, MidpointRounding.AwayFromZero), 8) + xOffset, AdvancedMath.GetNearestMultiple((int)Math.Round(sprites[i].position.Y - yOffset, MidpointRounding.AwayFromZero), 8) + yOffset);
                    sprites[i].UpdatePos(0);
                }
                if (Origins[shape].X < 0)
                {
                    origin = new Vector2();
                    for (int i = 0; i < sprites.Count; i++)
                    {
                        origin += sprites[i].position;
                    }
                    origin /= sprites.Count;
                }
            }
        }

        public void RotateLeft(List<Sprite> colliding)
        {
            if (active)
            {
                for (int i = 0; i < sprites.Count; i++)
                {
                    Vector2 temp = AdvancedMath.Rotate(sprites[i].position - origin, -90) + origin;
                    temp = new Vector2(AdvancedMath.GetNearestMultiple((int)Math.Round(temp.X - xOffset), 8) + xOffset, AdvancedMath.GetNearestMultiple((int)Math.Round(temp.Y - yOffset), 8) + yOffset);
                    if (colliding.Any(o => o.BasicIntersects(new Rectangle((int)temp.X, (int)temp.Y, sprites[i].rectangle.Width, sprites[i].rectangle.Height))))
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
                    Vector2 temp = AdvancedMath.Rotate(sprites[i].position - origin, -90) + origin;
                    temp = new Vector2(AdvancedMath.GetNearestMultiple((int)Math.Round(temp.X - xOffset, MidpointRounding.AwayFromZero), 8) + xOffset, AdvancedMath.GetNearestMultiple((int)Math.Round(temp.Y - yOffset, MidpointRounding.AwayFromZero), 8) + yOffset);
                    if (temp.X < 17 || temp.X > 89 || temp.Y > 137)
                    {
                        return;
                    }
                    for (int a = 0; a < colliding.Count; a++)
                    {
                        if (colliding[a].sprites.Any(o => o.position == temp))
                        {
                            return;
                        }
                    }
                }
                for (int i = 0; i < sprites.Count; i++)
                {
                    sprites[i].position = AdvancedMath.Rotate(sprites[i].position - origin, -90) + origin;
                    sprites[i].position = new Vector2(AdvancedMath.GetNearestMultiple((int)Math.Round(sprites[i].position.X - xOffset, MidpointRounding.AwayFromZero), 8) + xOffset, AdvancedMath.GetNearestMultiple((int)Math.Round(sprites[i].position.Y - yOffset, MidpointRounding.AwayFromZero), 8) + yOffset);
                    sprites[i].UpdatePos(0);
                }
                if (Origins[shape].X < 0)
                {
                    origin = new Vector2();
                    for (int i = 0; i < sprites.Count; i++)
                    {
                        origin += sprites[i].position;
                    }
                    origin /= sprites.Count;
                }
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
                        //if (colliding[a].sprites.Any(o => o.BasicIntersects(new Rectangle((int)sprites[i].position.X - sprites[i].rectangle.Width, (int)sprites[i].position.Y, sprites[i].rectangle.Width, sprites[i].rectangle.Height))))
                        if (colliding[a].sprites.Any(o => o.position == new Vector2(sprites[i].position.X - sprites[i].rectangle.Width, sprites[i].position.Y)))
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
                if (Origins[shape].X < 0)
                {
                    origin = new Vector2();
                    for (int i = 0; i < sprites.Count; i++)
                    {
                        origin += sprites[i].position;
                    }
                    origin /= sprites.Count;
                }
                else
                {
                    origin.X -= sprites[0].rectangle.Width;
                }
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
                        //if (colliding[a].sprites.Any(o => o.BasicIntersects(new Rectangle((int)sprites[i].position.X + sprites[i].rectangle.Width, (int)sprites[i].position.Y, sprites[i].rectangle.Width, sprites[i].rectangle.Height))))
                        if (colliding[a].sprites.Any(o => o.position == new Vector2(sprites[i].position.X + sprites[i].rectangle.Width, sprites[i].position.Y)))
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
                if (Origins[shape].X < 0)
                {
                    origin = new Vector2();
                    for (int i = 0; i < sprites.Count; i++)
                    {
                        origin += sprites[i].position;
                    }
                    origin /= sprites.Count;
                }
                else
                {
                    origin.X += sprites[0].rectangle.Width;
                }
            }
        }

        public void Update(List<Sprite> colliding)
        {
            if (active)
            {
                for (int i = 0; i < sprites.Count; i++)
                {
                    if (colliding.Any(o => o.BasicIntersects(new Rectangle(sprites[i].rectangle.X, sprites[i].rectangle.Y + sprites[i].rectangle.Height, sprites[i].rectangle.Width, sprites[i].rectangle.Height))) || sprites[i].position.Y + sprites[i].rectangle.Height > 137)
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
                        //if (colliding[a].sprites.Any(o => o.BasicIntersects(new Rectangle(sprites[i].rectangle.X, sprites[i].rectangle.Y + sprites[i].rectangle.Height, sprites[i].rectangle.Width, sprites[i].rectangle.Height))))
                        if (colliding[a].sprites.Any(o => o.position == new Vector2(sprites[i].position.X, sprites[i].position.Y + sprites[i].rectangle.Height)))
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
                if (Origins[shape].X < 0)
                {
                    origin = new Vector2();
                    for (int i = 0; i < sprites.Count; i++)
                    {
                        origin += sprites[i].position;
                    }
                    origin /= sprites.Count;
                }
                else
                {
                    origin.Y += sprites[0].rectangle.Height;
                }
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

        public void SetActive(bool isActive)
        {
            active = isActive;
        }
    }
}