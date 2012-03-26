using System.Drawing.Imaging;
using System.IO;
using System.Web.Mvc;
using gheat;

namespace EB.Crime.Map.Controllers
{
    public class HeatTileController : Controller
    {
        public ActionResult GetTile()
        {
            var foo = GHeat.GetTile(null, null, 1, 1, 1);
            MemoryStream ms = new MemoryStream();
            foo.Save(ms, ImageFormat.Png);
            return File(ms.ToArray(), "image/png");
        }
    }
}
