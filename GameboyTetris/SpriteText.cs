using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameboyTetris
{
    internal class SpriteText
    {
        public enum DrawMode
        {
            Normal,
            Middle,
            Underline,
            MiddleUnderline,
        }

        private DrawMode drawMode;
        private SpriteFont font;
        public string text;
        private Texture2D tex;
        public Vector2 position;

        public SpriteText(Texture2D _tex, Vector2 _position, DrawMode _drawMode, SpriteFont _font, string _text)
        //: base(_tex, _position)
        {
            drawMode = _drawMode;
            font = _font;
            text = _text;
            tex = _tex;
            position = _position;
        }

        //public SpriteText(Texture2D _tex, Vector2 _position, Vector2 _origin, DrawMode _drawMode, SpriteFont _font, string _text)
        ////: base(_tex, _position, _origin)
        //{
        //    drawMode = _drawMode;
        //    font = _font;
        //    text = _text;
        //    tex = _tex;
        //    position = _position;
        //}

        //public SpriteText(Texture2D _tex, GameWindow window, DrawMode _drawMode, SpriteFont _font, string _text)
        //   // : base(_tex, window)
        //{
        //    drawMode = _drawMode;
        //    font = _font;
        //    text = _text;
        //    tex = _tex;
        //    position = _position;
        //    //origin = new Vector2();
        //}

        public void Draw(SpriteBatch _spriteBatch)
        {
            //_spriteBatch.Draw(tex, position, null, Color.White, rotation, new Vector2(origin.X, origin.Y), playerScale, SpriteEffects.None, 1);
            if (drawMode == DrawMode.Middle || drawMode == DrawMode.MiddleUnderline)
            {
                _spriteBatch.DrawString(font, text, position - (font.MeasureString(text) / 2 * 0.25f), new Color(7, 24, 33), 0, new Vector2(), 0.25f, SpriteEffects.None, 0);
            }
            else
            {
                _spriteBatch.DrawString(font, text, position, new Color(7, 24, 33), 0, new Vector2(), 0.25f, SpriteEffects.None, 0);
            }
            if (drawMode == DrawMode.Underline || drawMode == DrawMode.MiddleUnderline)
            {
                if (drawMode == DrawMode.MiddleUnderline)
                {
                    _spriteBatch.Draw(tex, new Rectangle((int)Math.Round(position.X) - (int)Math.Round((font.MeasureString(text).X / 2 * 0.25f)), (int)Math.Round(position.Y + (font.MeasureString(text).Y / 2 * 0.25f) - 1), (int)Math.Round(font.MeasureString(text).X * 0.25f), 1), new Color(48, 104, 80));
                }
                else
                {
                    _spriteBatch.Draw(tex, new Rectangle((int)Math.Round(position.X), (int)Math.Round(position.Y + (font.MeasureString(text).Y / 2 * 0.25f) - 1), (int)Math.Round(font.MeasureString(text).X * 0.25f), 1), new Color(48, 104, 80));
                }
            }
        }

        public void Draw(SpriteBatch _spriteBatch, Color color)
        {
            //_spriteBatch.Draw(tex, position, null, color, rotation, new Vector2(origin.X * tex.Width, origin.Y * tex.Height), playerScale, SpriteEffects.None, 1);
            if (drawMode == DrawMode.Middle || drawMode == DrawMode.MiddleUnderline)
            {
                _spriteBatch.DrawString(font, text, position - (font.MeasureString(text) / 2 * 0.25f), color, 0, new Vector2(), 0.25f, SpriteEffects.None, 0);
            }
            else
            {
                _spriteBatch.DrawString(font, text, position, color, 0, new Vector2(), 0.25f, SpriteEffects.None, 0);
            }
            if (drawMode == DrawMode.Underline || drawMode == DrawMode.MiddleUnderline)
            {
                if (drawMode == DrawMode.MiddleUnderline)
                {
                    _spriteBatch.Draw(tex, new Rectangle((int)Math.Round(position.X) - (int)Math.Round((font.MeasureString(text).X / 2 * 0.25f)), (int)Math.Round(position.Y + (font.MeasureString(text).Y / 2 * 0.25f) - 1), (int)Math.Round(font.MeasureString(text).X * 0.25f), 1), new Color(48, 104, 80));
                }
                else
                {
                    _spriteBatch.Draw(tex, new Rectangle((int)Math.Round(position.X), (int)Math.Round(position.Y + (font.MeasureString(text).Y / 2 * 0.25f) - 1), (int)Math.Round(font.MeasureString(text).X * 0.25f), 1), new Color(48, 104, 80));
                }
            }
        }
    }
}