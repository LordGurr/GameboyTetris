using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameboyTetris
{
    internal static class AdvancedMath
    {
        public static float Deg2Rad = ((float)Math.PI * 2) / 360;
        public static float Rad2Deg = 360 / ((float)Math.PI * 2);

        public static float AngleBetween(Vector2 a, Vector2 B)
        {
            float dotProd;
            float Ratio;
            dotProd = Vector2.Dot(a, B);
            Ratio = dotProd / a.Length();
            return (float)(Math.Acos(Ratio)) * Rad2Deg;
        }

        public static float Vector2Distance(Vector2 a, Vector2 b)
        {
            return Magnitude(a - b);
        }

        public static Vector2 AngleToVector(float angle)
        {
            return new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
        }

        public static float VectorToAngle(Vector2 vector)
        {
            return (float)Math.Atan2(vector.Y, vector.X);
        }

        public static Vector2 Rotate(Vector2 v, float degrees) //Stulen från unity
        {
            float sin = (float)Math.Sin(degrees * Deg2Rad);
            float cos = (float)Math.Cos(degrees * Deg2Rad);

            float tx = v.X;
            float ty = v.Y;
            v.X = (cos * tx) - (sin * ty);
            v.Y = (sin * tx) + (cos * ty);
            return v;
        }

        public static float sqrMagnitude(Vector2 temp) //Stulen från unity
        {
            return temp.X * temp.X + temp.Y * temp.Y;
        }

        public static float Magnitude(Vector2 temp) //Stulen från unity
        {
            return (float)Math.Sqrt(sqrMagnitude(temp));
        }

        /// <summary>
        /// Splits a texture into an array of smaller textures of the specified size.
        /// </summary>
        /// <param name="original">The texture to be split into smaller textures</param>
        /// <param name="partWidth">The width of each of the smaller textures that will be contained in the returned array.</param>
        /// <param name="partHeight">The height of each of the smaller textures that will be contained in the returned array.</param>
        public static Texture2D[] Split(Texture2D original, int partWidth, int partHeight, out int xCount, out int yCount)
        {
            yCount = original.Height / partHeight /*+ (partHeight % original.Height == 0 ? 0 : 1)*/;//The number of textures in each horizontal row
            xCount = original.Width / partWidth /*+ (partWidth % original.Width == 0 ? 0 : 1)*/;//The number of textures in each vertical column
            Texture2D[] r = new Texture2D[xCount * yCount];//Number of parts = (area of original) / (area of each part).
            int dataPerPart = partWidth * partHeight;//Number of pixels in each of the split parts

            //Get the pixel data from the original texture:
            Color[] originalData = new Color[original.Width * original.Height];
            original.GetData<Color>(originalData);

            int index = 0;
            for (int y = 0; y < yCount * partHeight; y += partHeight)
                for (int x = 0; x < xCount * partWidth; x += partWidth)
                {
                    //The texture at coordinate {x, y} from the top-left of the original texture
                    Texture2D part = new Texture2D(original.GraphicsDevice, partWidth, partHeight);
                    //The data for part
                    Color[] partData = new Color[dataPerPart];

                    //Fill the part data with colors from the original texture
                    for (int py = 0; py < partHeight; py++)
                        for (int px = 0; px < partWidth; px++)
                        {
                            int partIndex = px + py * partWidth;
                            //If a part goes outside of the source texture, then fill the overlapping part with Color.Transparent
                            if (y + py >= original.Height || x + px >= original.Width)
                                partData[partIndex] = Color.Transparent;
                            else
                                partData[partIndex] = originalData[(x + px) + (y + py) * original.Width];
                        }

                    //Fill the part with the extracted data
                    part.SetData<Color>(partData);
                    //Stick the part in the return array:
                    r[index++] = part;
                }
            //Return the array of parts.
            return r;
        }

        public static int GetNearestMultiple(int value, int factor)
        {
            return (int)Math.Round(
                              (value / (double)factor),
                              MidpointRounding.AwayFromZero
                          ) * factor;
        }

        public static Vector2 Normalize(Vector2 vector)
        {
            float mag = AdvancedMath.Magnitude(vector);
            //these intermediate variables force the intermediate result to be
            //of float precision. without this, the intermediate result can be of higher
            //precision, which changes behavior.
            float normalized_x = vector.X / mag;
            float normalized_y = vector.Y / mag;
            return new Vector2(normalized_x, normalized_y);
        }
    }
}