using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfKa
{
    /// <summary>
    /// Класс для подсчета строк,в которыххотябы 1 пиксель закрашен, и пустых строк.
    /// </summary>
    public class AmountAndType
    {
        public ushort amount;
        public byte type;
        public AmountAndType(ushort amount, byte type)
        {
            this.amount = amount;
            this.type = type;
        }
    }
}
