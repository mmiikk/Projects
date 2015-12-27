using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fotobudka
{
    class Helper
    {
        public Helper()
        {
            
        }
        public int CentimeterToPixel(double Centimeter,bool X)
        {
            double pixelX = -1;
            double pixelY = -1;
            using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
            {
                pixelX = Centimeter * g.DpiX / 2.54d;
                pixelY = Centimeter * g.DpiY / 2.54d;
            }
            if(X)
                return (int)pixelX;
            else
                return (int)pixelY;
        }

       
    }
}
