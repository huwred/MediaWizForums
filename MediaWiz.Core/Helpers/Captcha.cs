using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Maui.Graphics.Skia;

namespace MediaWiz.Forums.Helpers
{
    public class Captcha
    {
        private ICanvas canvas;
        private string answer;
        private readonly string question;
        private readonly Random random;
        private readonly SkiaBitmapExportContext bmp;

        ///<summary>Init the captcha class
        ///<example>For example, we can instantiate this class like this:
        ///<code>
        ///Captcha cptch = new CaptchaNoJS.Captcha(350, 50, 15);
        ///</code>
        ///</example>
        ///</summary>

        public Captcha(int pWidth, int pHeight, int pLength)
        {
            bmp = new(pWidth, pHeight, 1.0f);
            canvas = bmp.Canvas;
            random = new Random();

            question = GetRandomString(pLength);
        }

        /// <summary>  
        /// return captcha as base64 string
        /// <example>For example, we can use it in .cshtml razor view like this:
        /// <code>
        ///   img src="data:image/png;base64, @ViewData["captcha"]
        /// </code>
        /// <see href="https://goonlinetools.com/html-viewer/#jaii4893duk2b3ztkcem">see the result</see>
        /// </example>
        /// </summary>  
        public string GenerateAsB64(CaptchaType captchaType = CaptchaType.Line)
        {
            MemoryStream result = new MemoryStream();
            canvas.FillColor = GetRandomColor();

            switch (captchaType)
            {
                case CaptchaType.Line:
                    AddLines(30);
                    break;
                case CaptchaType.Circle:
                    AddCircles(30);
                    break;
                case CaptchaType.Random:

                    if (random.Next(0, 100) > 50)
                    {
                        AddLines(30);
                    }
                    else
                    {
                        AddCircles(30);
                    }
                    break;
            }

            canvas = AddStringToImg(canvas);
            bmp.WriteToStream(result);
            return Convert.ToBase64String(result.ToArray());
        }

        private void AddLines(int nb)
        {
            for (int i = 0; i < nb; i++)
            {
                canvas.StrokeColor = GetRandomColor();
                canvas.StrokeSize = random.Next(2, 10);
                canvas.DrawLine(random.Next(bmp.Width), random.Next(bmp.Height), random.Next(bmp.Width), random.Next(bmp.Height));
            }
        }
        private void AddCircles(int nb)
        {
            for (int i = 0; i < nb; i++)
            {
                canvas.StrokeColor = GetRandomColor();
                canvas.StrokeSize = random.Next(2, 10);
                canvas.DrawCircle(new PointF(random.Next(bmp.Width), random.Next(bmp.Height)), random.Next(2, 20));
            }
        }

        /// <summary>  
        /// return the correct answer
        /// </summary>

        public String GetAnswer()
        {
            return answer;
        }

        private ICanvas AddStringToImg(ICanvas pCanvas)
        {
            //            gfx.DrawString(captcha, new Font("Tahoma", 28), Brushes.OrangeRed, 10, 10);
            PointF strLoc = new PointF(20f, 50f);
            pCanvas.StrokeColor = Colors.White;
            pCanvas.FontColor = Colors.OrangeRed;
            pCanvas.Font = new Font("Tahoma");
            pCanvas.FontSize = 32;
            pCanvas.DrawString(question.ToString(), strLoc.X, strLoc.Y,HorizontalAlignment.Left);

            return pCanvas;
        }
        private string GetRandomString(int length)
        {
            var rand = new Random((int)DateTime.Now.Ticks);
            var allowed = new List<string>() { "plus", "minus" /*,"multiply"*/ };
            int item = random.Next(allowed.Count);
            string randomBar = allowed[item];

            //generate new question
            int a = rand.Next(10, 99);
            int b = rand.Next(0, 9);
            string captcha;
            switch (randomBar)
            {
                case "plus":
                    answer = (a+b).ToString();
                    captcha = $"{a} + {b} = ?";
                    break;
                case "minus":
                    answer = (a-b).ToString();
                    captcha = $"{a} - {b} = ?";
                    break;
                case "multiply":
                    answer = (a*b).ToString();
                    captcha = $"{a} x {b} = ?";
                    break;
                default:
                    answer = (a+b).ToString();
                    captcha = $"{a} + {b} = ?";
                    break;
            }

            return new string(captcha);
        }

        private Color GetRandomColor()
        {
            return new Color(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255),random.Next(0, 255));
        }

        public enum CaptchaType
        {
            Line,
            Circle,
            Random
        }
    }
}
