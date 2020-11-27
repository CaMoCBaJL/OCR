using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfKa
{
    /// <summary>
    /// Класс для подсчета ширины буквы. Используется для уточнения разделения строк на символы.
    /// </summary>
    public class LetterWidthAndCount : IComparable
    {
        public byte width;
        public byte count;
        public LetterWidthAndCount(byte width, byte count)
        {
            this.width = width;
            this.count = count;
        }
        int IComparable.CompareTo(object obj)
        {
            LetterWidthAndCount l = (LetterWidthAndCount)obj;
            if (l.width != 0 && l.count != 0)
                return this.width.CompareTo(l.width);
            else
                throw new Exception("АЙ АЙАЙ НЕХОРОШО!");
        }

    }
}
