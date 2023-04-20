using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Text;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace GameboyTetris
{
    internal class InputBox
    {
        private Stopwatch timeSinceCursor;
        private Stopwatch timeSinceWritten;
        private float delayAfterWrittenCursor = 1f;
        private float cursorFlash = 0.5f;
        private float ratio;
        private float percentage = 0.98f;
        private SpriteFont font;
        private StringBuilder stringBuilder;
        private bool mulitpleLines = false;
        private string previousString = string.Empty;
        private int maxLength = int.MaxValue;
        private float scale = 0.261f;

        public Rectangle rectangle { protected set; get; }
        public Texture2D texture { protected set; get; }
        public string text { protected set; get; }
        public bool pressed { protected set; get; }
        public Color rectangleColor { protected set; get; }
        public Color textColor { protected set; get; }

        public InputBox(Rectangle _rectangle, Texture2D _texture, string _text, bool _mulitpleLines)
        //: base(_rectangle, _texture, _text)
        {
            text = _text;
            texture = _texture;
            rectangle = _rectangle;
            timeSinceCursor = new Stopwatch();
            timeSinceCursor.Start();
            timeSinceWritten = new Stopwatch();
            timeSinceWritten.Start();
            ratio = (float)_rectangle.Width / (float)_rectangle.Height;
            //rectangle = new Rectangle((int)(rectangle.X + rectangle.Width * (1 - percentage)), (int)Math.Round(rectangle.Y + rectangle.Height * ((1 - percentage) * ratio)), (int)(rectangle.Width * MathF.Pow(percentage, 2)), (int)(rectangle.Height * (MathF.Pow((percentage - 1) * ratio + 1, 2))));
            stringBuilder = new StringBuilder();
            stringBuilder.Append(text);
            mulitpleLines = _mulitpleLines;
            isSelected = false;
        }

        public InputBox(Rectangle _rectangle, Texture2D _texture, string _text, Color _rectangleColor, bool _mulitpleLines, int _maxLength)
        //: base(_rectangle, _texture, _text, _rectangleColor)
        {
            text = _text;
            texture = _texture;
            rectangle = _rectangle;
            rectangleColor = _rectangleColor;
            timeSinceCursor = new Stopwatch();
            timeSinceCursor.Start();
            timeSinceWritten = new Stopwatch();
            timeSinceWritten.Start();
            ratio = (float)_rectangle.Width / (float)_rectangle.Height;
            //rectangle = new Rectangle((int)(rectangle.X + rectangle.Width * (1 - percentage)), (int)Math.Round(rectangle.Y + rectangle.Height * ((1 - percentage) * ratio)), (int)(rectangle.Width * MathF.Pow(percentage, 2)), (int)(rectangle.Height * (MathF.Pow((percentage - 1) * ratio + 1, 2))));
            stringBuilder = new StringBuilder();
            stringBuilder.Append(text);
            mulitpleLines = _mulitpleLines;
            maxLength = _maxLength;
            isSelected = false;
        }

        public InputBox(Rectangle _rectangle, Texture2D _texture, string _text, Color _rectangleColor, Color _textColor, bool _mulitpleLines)
        //: base(_rectangle, _texture, _text, _rectangleColor, _textColor)
        {
            text = _text;
            texture = _texture;
            rectangle = _rectangle;
            rectangleColor = _rectangleColor;
            timeSinceCursor = new Stopwatch();
            timeSinceCursor.Start();
            timeSinceWritten = new Stopwatch();
            timeSinceWritten.Start();
            ratio = (float)_rectangle.Width / (float)_rectangle.Height;
            //rectangle = new Rectangle((int)(rectangle.X + rectangle.Width * (1 - percentage)), (int)Math.Round(rectangle.Y + rectangle.Height * ((1 - percentage) * ratio)), (int)(rectangle.Width * MathF.Pow(percentage, 2)), (int)(rectangle.Height * (MathF.Pow((percentage - 1) * ratio + 1, 2))));
            stringBuilder = new StringBuilder();
            stringBuilder.Append(text);
            mulitpleLines = _mulitpleLines;
            isSelected = false;
        }

        public bool isSelected { protected set; get; }

        public void Activate(GameWindow window)
        {
            isSelected = true;
            window.TextInput += TextInputHandler;
            window.KeyDown += KeyHandler;
            timeSinceCursor.Restart();
            showingCursor = true;
        }

        public void Deactivate(GameWindow window)
        {
            isSelected = false;
            window.TextInput -= TextInputHandler;
            window.KeyDown -= KeyHandler;
        }

        //public bool Selected(GameWindow window)
        //{
        //    if (Input.GetMouseButtonDown(0))
        //    {
        //        bool previosly = isSelected;
        //        isSelected = rectangle.Contains(Input.MousePos());
        //        if (previosly != isSelected)
        //        {
        //            if (isSelected)
        //            {
        //                window.TextInput += TextInputHandler;
        //                window.KeyDown += KeyHandler;
        //                timeSinceCursor.Restart();
        //                showingCursor = true;
        //            }
        //            else
        //            {
        //                window.TextInput -= TextInputHandler;
        //                window.KeyDown -= KeyHandler;
        //            }
        //        }
        //    }
        //    return isSelected;
        //}

        public void ClearText()
        {
            previousString = string.Empty;
            text = string.Empty;
            cursorPos = -1;
        }

        public void AddText(string input)
        {
            previousString = text;
            text += input;
            if (text.Length > maxLength)
            {
                text = text.Substring(0, maxLength);
                cursorPos = MathHelper.Clamp(cursorPos, -1, text.Length - 1);
            }
        }

        private int cursorPos = -1;

        public void AddText(char input)
        {
            if (input == '\u0016') // ctr V
            {
                previousString = text;
                string temp = System.Windows.Forms.Clipboard.GetText();
                if (font.MeasureString(text + temp).X > rectangle.Width && mulitpleLines)
                {
                    text += "\n";
                    stringBuilder.Append("\n");
                }
                //text += input;
                text = text.Insert(cursorPos + 1, temp.ToString());
                stringBuilder.Insert(cursorPos + 1, temp);
                cursorPos += temp.Length;
            }
            else if (input == '\u0003') //ctr C
            {
                if (text != null && text != string.Empty)
                {
                    System.Windows.Forms.Clipboard.SetText(text);
                }
            }
            else if (input == '\u0018')// ctr X
            {
                if (text != null && text != string.Empty)
                {
                    previousString = text;
                    System.Windows.Forms.Clipboard.SetText(text);
                    text = string.Empty;
                    cursorPos = -1;
                }
            }
            else if (input == '\u001a')// ctr Z
            {
                string temp = text;
                text = previousString;
                previousString = temp;
                cursorPos = MathHelper.Clamp(cursorPos, -1, text.Length - 1);
            }
            else if (input == '\b' || input == '\r') // \b == backspace. \r == remove
            {
                previousString = text;
                if (input == '\b')
                {
                    if (text.Length > 0 && cursorPos > -1)
                    {
                        text = text.Remove(cursorPos, 1);
                        stringBuilder.Remove(cursorPos, 1);
                        cursorPos--;
                    }
                }
                else if (input == '\r' && mulitpleLines)
                {
                    text = text.Insert(cursorPos + 1, "\n");
                    stringBuilder.Insert(cursorPos + 1, "\n");
                    cursorPos++;
                }
            }
            else
            {
                if (font.Characters.Contains(input))
                {
                    previousString = text;
                    Vector2 temp = font.MeasureString(text + input) * 1.2f;
                    temp.X += 5;
                    if (temp.X > rectangle.Width && mulitpleLines)
                    {
                        text = text.Insert(cursorPos + 1, "\n");
                        stringBuilder.Insert(cursorPos + 1, "\n");
                        cursorPos++;
                    }
                    //text += input;
                    text = text.Insert(cursorPos + 1, input.ToString());
                    stringBuilder.Insert(cursorPos + 1, input);
                    cursorPos++;
                }
            }
            text = text.Replace("\n", "");
            text = text.Replace("\r", "");
            if (text.Length > maxLength)
            {
                text = text.Substring(0, maxLength);
            }
            cursorPos = MathHelper.Clamp(cursorPos, -1, text.Length - 1);
        }

        private void TextInputHandler(object sender, TextInputEventArgs args)
        {
            var pressedKey = args.Key;
            var character = args.Character;
            AddText(character);
            timeSinceWritten.Restart();
            showingCursor = true;
            // do something with the character (and optionally the key)
            // ...
        }

        private void KeyHandler(object sender, InputKeyEventArgs args)
        {
            if (args.Key == Keys.Left)
            {
                cursorPos--;
                cursorPos = MathHelper.Clamp(cursorPos, -1, text.Length - 1);
            }
            if (args.Key == Keys.Right)
            {
                cursorPos++;
                cursorPos = MathHelper.Clamp(cursorPos, -1, text.Length - 1);
            }
            if (args.Key == Keys.Delete)
            {
                if (text.Length > 0 && cursorPos < text.Length - 1)
                {
                    text = text.Remove(cursorPos + 1, 1);
                    stringBuilder.Remove(cursorPos + 1, 1);
                }
            }
            if (args.Key == Keys.Up)
            {
                if (mulitpleLines)
                {
                    cursorPos -= AmountToMoveOnLineChange();
                    cursorPos = MathHelper.Clamp(cursorPos, -1, text.Length - 1);
                }
                else
                {
                    cursorPos = -1;
                }
            }
            if (args.Key == Keys.Down)
            {
                if (mulitpleLines)
                {
                    cursorPos += AmountToMoveOnLineChange();
                    cursorPos = MathHelper.Clamp(cursorPos, -1, text.Length - 1);
                }
                else
                {
                    cursorPos = text.Length - 1;
                }
            }
            timeSinceWritten.Restart();
            showingCursor = true;
        }

        private int AmountToMoveOnLineChange()
        {
            string[] lines = text.Split("\n");
            int lineWithCursor = -1;
            int cursor = cursorPos;
            //cursor -= lines.Length;
            int temp = -1;
            int cursorPosInLine = -1;
            for (int i = 0; i < lines.Length; i++)
            {
                if (i + 1 < lines.Length)
                {
                    lines[i] += "\n";
                }
            }
            for (int i = 0; i < lines.Length; i++)
            {
                //if (cursor >= lines[i].Length && i + 1 < lines.Length)
                //{
                //    cursor -= lines[i].Length;
                //}
                //else
                //{
                //    lineWithCursor = i;
                //    break;
                //}
                if (temp >= cursor || i + 1 >= lines.Length)
                {
                    //lineWithCursor = i + 1 < lines.Length ? i + 1 : i;
                    lineWithCursor = i;
                    //cursorPosInLine = temp - cursor + 1;
                    cursorPosInLine = cursor - (temp - lines[i].Length) + 1;
                    break;
                }
                temp += lines[i].Length;
            }
            for (int i = 0; i < lineWithCursor; i++)
            {
            }
            if (lineWithCursor > -1)
            {
                //int newCursoPos = 0;
                //for (int i = 0; i < lineWithCursor; i++)
                //{
                //    newCursoPos += lines[i].Length + 1;
                //}
                //cursorPos = newCursoPos;
                //cursorPos -= lines[lineWithCursor].Length + 1;
                return cursorPosInLine;
                return lines[lineWithCursor].Length;
            }
            return 0;
        }

        private bool showingCursor = false;

        public void Draw(SpriteBatch _spriteBatch, SpriteFont _font)
        {
            if (isSelected)
            {
                if (font != _font)
                {
                    font = _font;
                }
                if (timeSinceCursor.Elapsed.TotalSeconds > cursorFlash)
                {
                    timeSinceCursor.Restart();
                    if (timeSinceWritten.Elapsed.TotalSeconds > delayAfterWrittenCursor)
                    {
                        showingCursor = !showingCursor;
                    }
                    else
                    {
                        showingCursor = true;
                    }
                    //showingCursor = true;
                }
                if (showingCursor)
                {
                    string realText = text;
                    text = text.Insert(cursorPos + 1, "|");
                    //Vector2 size = font.MeasureString(text) * 1.2f;
                    //size.X += 5;
                    //if (size.X > rectangle.Width && cursorPos > -1 && font.MeasureString(text.Substring(0, cursorPos)).X * 1.2f + 10 > rectangle.Width)
                    //{
                    //    _spriteBatch.Draw(texture, rectangle, rectangleColor);
                    //    _spriteBatch.DrawString(font, text, new Vector2(rectangle.Right - (font.MeasureString(text.Substring(0, cursorPos)).X * 1.2f + 50), rectangle.Y), textColor, 0, new Vector2(/*0, size.Y / 2*/), scale, SpriteEffects.None, 1);
                    //}
                    //else
                    {
                        _spriteBatch.Draw(texture, rectangle, rectangleColor);
                        _spriteBatch.DrawString(font, text, new Vector2(rectangle.Left, rectangle.Top - 3), new Color(7, 24, 33), 0, new Vector2(), scale, SpriteEffects.None, 0);
                    }
                    text = realText;
                    return;
                }
                //else
                //{
                //    base.Draw(_spriteBatch, _font);
                //}
            }
            //if (font != null)
            //{
            //    Vector2 size = font.MeasureString(text) * 1.2f;
            //    size.X += 5;
            //    if (size.X > rectangle.Width && cursorPos > -1 && font.MeasureString(text.Substring(0, cursorPos)).X * 1.2f + 10 > rectangle.Width)
            //    {
            //        _spriteBatch.Draw(texture, rectangle, rectangleColor);
            //        //_spriteBatch.DrawString(font, text, new Vector2(rectangle.Right - ((float)cursorPos / (float)text.Length) * size.X, rectangle.Y), textColor, 0, new Vector2(/*0, size.Y / 2*/), 1.2f, SpriteEffects.None, 1);
            //        _spriteBatch.DrawString(font, text, new Vector2(rectangle.Right - (font.MeasureString(text.Substring(0, cursorPos)).X * 1.2f + 50), rectangle.Y), textColor, 0, new Vector2(/*0, size.Y / 2*/), scale, SpriteEffects.None, 1);
            //        return;
            //    }
            //}
            //else
            //{
            if (font != _font)
            {
                font = _font;
            }
            _spriteBatch.Draw(texture, rectangle, rectangleColor);
            _spriteBatch.DrawString(font, text, new Vector2(rectangle.Left, rectangle.Top - 3), new Color(7, 24, 33), 0, new Vector2(), scale, SpriteEffects.None, 0);

            //base.Draw(_spriteBatch, _font);
            //}
        }
    }
}