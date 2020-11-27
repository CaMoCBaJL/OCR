using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfKa
{
    /// <summary>
    /// Класс для хранения данных о массиве пикселей - сам массив, высота и ширина.
    /// /// </summary>
    public class PixelArrayAndParams
    {
        public byte[,] pixelArray;
        public int heigth;
        public int width;
        /// <summary>
        /// Массив пикселей, ВЫСОТА, ШИРИНА!.
        /// </summary>
        /// <param name="pixelArray"></param>
        /// <param name="heigth"></param>
        /// <param name="width"></param>
        public PixelArrayAndParams(byte[,] pixelArray, int heigth, int width)
        {
            this.pixelArray = pixelArray;
            this.heigth = heigth;
            this.width = width;
        }
        public PixelArrayAndParams(PixelArrayAndParams elem)
        {
            this.pixelArray = elem.pixelArray;
            this.heigth = elem.heigth;
            this.width = elem.width;
        }
    }
}

