using System;
using System.Threading.Tasks;

using MediaWiz.Forums.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MediaWiz.Forums.ViewComponents
{
    public class CaptchaViewComponent : ViewComponent
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CaptchaViewComponent(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;

        }

        public async Task<IViewComponentResult> InvokeAsync(Guid UserId)
        {
            Captcha captcha = new Captcha(200, 80, 6);
            TempData["b64"] = captcha.GenerateAsB64(Captcha.CaptchaType.Circle);
            _httpContextAccessor.HttpContext.Session.SetString("Captcha", captcha.GetAnswer());

            return await Task.FromResult((IViewComponentResult)View("Captcha"));
        }
        //public async Task<IViewComponentResult> InvokeAsync(Guid UserId)
        //{
        //    var session = _httpContextAccessor.HttpContext.Session;
        //    string prefix = "";
        //    session.Remove("Captcha");
        //    var rand = new Random((int)DateTime.Now.Ticks);
        //    var allowed = new List<string>() { "plus", "minus" /*,"multiply"*/ };
        //    Random random = new Random();
        //    int item = random.Next(allowed.Count);
        //    string randomBar = allowed[item];

        //    try
        //    {
        //        //generate new question
        //        int a = rand.Next(10, 99);
        //        int b = rand.Next(0, 9);
        //        string captcha;
        //        switch (randomBar)
        //        {
        //            case "plus":
        //                session.SetString("Captcha" + prefix,(a+b).ToString());
        //                captcha = $"{a} + {b} = ?";
        //                break;
        //            case "minus":
        //                session.SetString("Captcha" + prefix,(a-b).ToString());
        //                captcha = $"{a} - {b} = ?";
        //                break;
        //            case "multiply":
        //                session.SetString("Captcha" + prefix,(a*b).ToString());
        //                captcha = $"{a} x {b} = ?";
        //                break;
        //            default:
        //                session.SetString("Captcha" + prefix,(a+b).ToString());
        //                captcha = $"{a} + {b} = ?";
        //                break;
        //        }

        //        using var mem = new MemoryStream();
        //        using var bmp = new Bitmap(240, 60);
        //        using (var gfx = Graphics.FromImage(bmp))
        //        {
        //            gfx.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
        //            gfx.SmoothingMode = SmoothingMode.AntiAlias;
        //            gfx.FillRectangle(Brushes.White, new Rectangle(0, 0, bmp.Width, bmp.Height));

        //            //add noise
        //            int i;
        //            var pen = new Pen(Color.LightYellow);
        //            for (i = 1; i < 10; i++)
        //            {
        //                pen.Color = Color.FromArgb(
        //                    (rand.Next(0, 255)),
        //                    (rand.Next(0, 255)),
        //                    (rand.Next(0, 255)));

        //                int r = rand.Next(0, (240 / 3));
        //                int x = rand.Next(0, 240);
        //                int y = rand.Next(0, 60);

        //                x -= r;
        //                y -= r;
        //                gfx.DrawEllipse(pen, x, y, r, r);
        //            }

        //            //add question
        //            gfx.DrawString(captcha, new Font("Tahoma", 28), Brushes.OrangeRed, 10, 10);

        //            //render as Jpeg
        //            bmp.Save(mem, System.Drawing.Imaging.ImageFormat.Jpeg);
        //            //img = File(mem.GetBuffer(), "image/Jpeg");

        //            byte[] imageBytes = mem.ToArray();
        //            // Convert byte[] to Base64 String
        //            string base64String = Convert.ToBase64String(imageBytes);
        //            return await Task.FromResult((IViewComponentResult)View("Captcha",Content("data:image/jpg;base64," + base64String)));
        //        }

        //    }
        //    catch (Exception)
        //    {
        //        return Content("");
        //        //throw new HttpException(404, "Captcha image Not found");
        //    }
            
        //}

    }
}