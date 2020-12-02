using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfKa
{   
    /// <summary>
    /// Вспомогательный классЮ для преобразования данных о пиксельных строках.
    /// </summary>
    public class PixelStringHeigthAndFirstIndex
    {
        public int firstElem;
        public int heigth;
        /// <summary>
        /// <para>
        /// <paramref name="firstElem"/> С этого эелемента.                 
        /// </para>
        /// <paramref name="heigth"/> Столько элементов.
        /// </summary>
        /// <param name="firstElem"></param>
        /// <param name="heigth"></param>
        public PixelStringHeigthAndFirstIndex(int firstElem, int heigth)
        {
            this.firstElem = firstElem;
            this.heigth = heigth;
        }
    }
}
