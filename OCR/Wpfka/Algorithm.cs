using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfKa
{
    /// <summary>
    /// Класс, содержащий все методы, с помощью которых я реализовал GettingGenericWidths.
    /// </summary>
    class Algorithm
    {
        public byte GetTheMostGenericElem(List<LetterWidthAndCount> lettersInfo)
        {
            lW = lettersInfo;
                if (lW.Count > 2)
                    GettingGenericWidths(2, 0);
                else
                    GettingGenericWidths(lW.Count - 1, 0);

            return MostRescentGenricElem();
        }
        List<LetterWidthAndCount> generic = new List<LetterWidthAndCount>();
        List<LetterWidthAndCount> lW; 
        /// <summary>
        /// Метод используется для обобщения широт полученных букв.
        /// </summary>
        /// <param name="i"> Индекс буквы.</param>
        /// <param name="prevCounter"> Кол-во соседних элементов на предыдущем этапе.</param>
        void GettingGenericWidths(int i, byte prevCounter)
        {
            byte counter = 0;
            ushort lSum = 0;
            ushort rSum = 0;
            if (lW[i].count != 0)
            {
                if (i - 2 >= 0)
                {
                    for (int j = i - 2; j < i; j++)
                        if (Math.Abs(lW[j].width - lW[i].width) < 3 && lW[j].count != 0)
                        {
                            counter++;
                            lSum += lW[j].count;
                        }
                }
                else
                {
                    for (int j = 0; j < i; j++)
                        if (Math.Abs(lW[j].width - lW[i].width) < 3 && lW[j].count != 0)
                        {
                            counter++;
                            lSum += lW[j].count;
                        }
                }
                switch (counter)
                {
                    case 0://Нет элементов слева
                        {
                            if (i + 3 < lW.Count)
                            {
                                for (int j = i + 1; j < i + 3; j++)
                                    if (Math.Abs(lW[j].width - lW[i].width) < 3 && lW[j].count > 0)
                                    {
                                        counter++;
                                        rSum += lW[j].count;
                                    }
                            }
                            else
                            {
                                if (i != lW.Count)
                                    for (int j = i + 1; j < lW.Count; j++)
                                        if (Math.Abs(lW[j].width - lW[i].width) < 3 && lW[j].count > 0)
                                        {
                                            counter++;
                                            rSum += lW[j].count;
                                        }
                            }
                            switch (counter)
                            {
                                case 0: //0 слева и 0 справа
                                    {
                                        generic.Add(new LetterWidthAndCount(lW[i].width, lW[i].count));
                                        LetterWidthAndCount buf = lW[i];
                                        buf.count = 0;
                                        lW[i] = buf;
                                        if (i - 1 > -1)
                                            GettingGenericWidths(i - 1, 0);
                                        if (i + 1 < lW.Count)
                                            GettingGenericWidths(i + 1, 0);
                                        break;
                                    }
                                case 1: //0 слева и 1 справа
                                    {
                                        if (prevCounter == 0)
                                        {
                                            GettingGenericWidths(i + 1, 1);
                                            bool isUsedElemsExist = false;
                                            for (int j = i; i < i + 2; j++)
                                                if (lW[j].count == 0)
                                                {
                                                    isUsedElemsExist = true;
                                                    break;
                                                }
                                            if (!isUsedElemsExist)
                                            {
                                                generic.Add(new LetterWidthAndCount(lW[i].width, (byte)(lW[i].count + rSum)));
                                                for (int j = i; j < i + 2; j++)
                                                {
                                                    LetterWidthAndCount buf = lW[j];
                                                    buf.count = 0;
                                                    lW[j] = buf;
                                                }
                                                if (i + 2 < lW.Count)
                                                    GettingGenericWidths(i + 2, 0);
                                                if (i - 1 > -1)
                                                    GettingGenericWidths(i - 1, 0);
                                            }
                                            else
                                            {
                                                sbyte s = IsNotAllEmpty();
                                                if (s != -1)
                                                    GettingGenericWidths(IsNotAllEmpty(), prevCounter);
                                                else
                                                    return;
                                            }
                                        }
                                        else
                                        {
                                            if (counter < prevCounter)
                                                return;
                                            else
                                            {
                                                generic.Add(new LetterWidthAndCount(lW[i].width, (byte)(lW[i].count + rSum)));
                                                for (int j = i; j < i + 2; j++)
                                                {
                                                    LetterWidthAndCount buf = lW[j];
                                                    buf.count = 0;
                                                    lW[j] = buf;
                                                }
                                                if (i + 2 < lW.Count)
                                                    GettingGenericWidths(i + 2, 0);
                                                if (i - 1 > -1)
                                                    GettingGenericWidths(i - 1, 0);
                                            }
                                        }
                                        break;
                                    }
                                case 2: //0 слева и 2 справа
                                    {
                                        if (prevCounter == 0)
                                        {
                                            GettingGenericWidths(i + 2, 2);
                                            bool isUsedElemsExist = false;
                                            for (int j = i; i < i + 3; j++)
                                                if (lW[j].count == 0)
                                                {
                                                    isUsedElemsExist = true;
                                                    break;
                                                }
                                            if (!isUsedElemsExist)
                                            {
                                                generic.Add(new LetterWidthAndCount(lW[i].width, (byte)(lW[i].count + rSum)));
                                                for (int j = i; j < i + 3; j++)
                                                {
                                                    LetterWidthAndCount buf = lW[j];
                                                    buf.count = 0;
                                                    lW[j] = buf;
                                                }
                                                if (i + 3 < lW.Count)
                                                    GettingGenericWidths(i + 3, 0);
                                                if (i - 1 > -1)
                                                    GettingGenericWidths(i - 1, 0);
                                            }
                                            else
                                            {
                                                sbyte s = IsNotAllEmpty();
                                                if (s != -1)
                                                    GettingGenericWidths(IsNotAllEmpty(), prevCounter);
                                                else
                                                    return;
                                            }
                                        }
                                        else
                                        {
                                            if (counter < prevCounter)
                                                return;
                                            else
                                            {
                                                generic.Add(new LetterWidthAndCount(lW[i].width, (byte)(lW[i].count + rSum)));
                                                for (int j = i; j < i + 3; j++)
                                                {
                                                    LetterWidthAndCount buf = lW[j];
                                                    buf.count = 0;
                                                    lW[j] = buf;
                                                }
                                                if (i + 3 < lW.Count)
                                                    GettingGenericWidths(i + 3, 0);
                                                if (i - 1 > -1)
                                                    GettingGenericWidths(i - 1, 0);
                                            }
                                        }
                                        break;
                                    }
                            }
                            break;
                        }
                    case 1: //1 слева
                        {
                            if (i + 3 < lW.Count)
                            {
                                for (int j = i + 1; j < i + 3; j++)
                                    if (Math.Abs(lW[j].width - lW[i].width) < 3 && lW[j].count > 0)
                                    {
                                        counter++;
                                        rSum += lW[j].count;
                                    }
                            }
                            else
                            {
                                if (i != lW.Count)
                                    for (int j = i + 1; j < lW.Count; j++)
                                        if (Math.Abs(lW[j].width - lW[i].width) < 3 && lW[j].count > 0)
                                        {
                                            counter++;
                                            rSum += lW[j].count;
                                        }
                            }
                            switch (counter)
                            {
                                case 1:  //1 слева и 0 справа
                                    {
                                        if (prevCounter == 0)
                                        {
                                            GettingGenericWidths(i - 1, 1);
                                            bool isUsedElemsExist = false;
                                            for (int j = i - 1; i < i + 1; j++)
                                                if (lW[j].count == 0)
                                                {
                                                    isUsedElemsExist = true;
                                                    break;
                                                }
                                            if (!isUsedElemsExist)
                                            {
                                                generic.Add(new LetterWidthAndCount(lW[i].width, (byte)(lW[i].count + lSum)));
                                                for (int j = i - 1; j < i + 1; j++)
                                                {
                                                    LetterWidthAndCount buf = lW[j];
                                                    buf.count = 0;
                                                    lW[j] = buf;
                                                }
                                                if (i + 1 < lW.Count)
                                                    GettingGenericWidths(i + 1, 0);
                                                if (i - 2 > -1)
                                                    GettingGenericWidths(i - 2, 0);
                                            }
                                            else
                                            {
                                                sbyte s = IsNotAllEmpty();
                                                if (s != -1)
                                                    GettingGenericWidths(IsNotAllEmpty(), prevCounter);
                                                else
                                                    return;
                                            }
                                        }
                                        if (prevCounter != 0)
                                        {
                                            if (counter < prevCounter)
                                                return;
                                            else
                                            {
                                                generic.Add(new LetterWidthAndCount(lW[i].width, (byte)(lW[i].count + lSum)));
                                                for (int j = i - 1; j < i + 1; j++)
                                                {
                                                    LetterWidthAndCount buf = lW[j];
                                                    buf.count = 0;
                                                    lW[j] = buf;
                                                }
                                                if (i + 1 < lW.Count)
                                                    GettingGenericWidths(i + 1, 0);
                                                if (i - 2 > -1)
                                                    GettingGenericWidths(i - 2, 0);
                                            }
                                        }
                                        break;
                                    }
                                case 2:  //1 слева и 1 справа
                                    {
                                        if (prevCounter == 0)
                                        {
                                            GettingGenericWidths(i - 1, 2);
                                            GettingGenericWidths(i + 1, 2);
                                            bool isUsedElemsExist = false;
                                            for (int j = i - 1; i < i + 1; j++)
                                                if (lW[j].count == 0)
                                                {
                                                    isUsedElemsExist = true;
                                                    break;
                                                }
                                            if (!isUsedElemsExist)
                                            {
                                                generic.Add(new LetterWidthAndCount(lW[i].width, (byte)(lW[i].count + rSum + lSum)));
                                                for (int j = i - 1; j < i + 2; j++)
                                                {
                                                    LetterWidthAndCount buf = lW[j];
                                                    buf.count = 0;
                                                    lW[j] = buf;
                                                }
                                                if (i + 2 < lW.Count)
                                                    GettingGenericWidths(i + 2, 0);
                                                if (i - 2 > -1)
                                                    GettingGenericWidths(i - 2, 0);
                                            }
                                            else
                                            {
                                                sbyte s = IsNotAllEmpty();
                                                if (s != -1)
                                                    GettingGenericWidths(IsNotAllEmpty(), prevCounter);
                                                else
                                                    return;
                                            }
                                        }
                                        if (prevCounter != 0)
                                        {
                                            if (counter < prevCounter)
                                                return;
                                            else
                                            {
                                                generic.Add(new LetterWidthAndCount(lW[i].width, (byte)(lW[i].count + rSum + lSum)));
                                                for (int j = i - 1; j < i + 2; j++)
                                                {
                                                    LetterWidthAndCount buf = lW[j];
                                                    buf.count = 0;
                                                    lW[j] = buf;
                                                }
                                                if (i + 2 < lW.Count)
                                                    GettingGenericWidths(i + 2, 0);
                                                if (i - 2 > -1)
                                                    GettingGenericWidths(i - 2, 0);
                                            }
                                        }
                                        break;
                                    }
                                case 3:  //1 слева и 2 справа
                                    {
                                        if (prevCounter == 0)
                                        {
                                            GettingGenericWidths(i + 1, 3);
                                            GettingGenericWidths(i - 1, 3);
                                            bool isUsedElemsExist = false;
                                            for (int j = i - 1; j < i + 3; j++)
                                                if (lW[j].count == 0)
                                                {
                                                    isUsedElemsExist = true;
                                                    break;
                                                }
                                            if (!isUsedElemsExist)
                                            {
                                                generic.Add(new LetterWidthAndCount(lW[i].width, (byte)(lW[i].count + rSum + lSum)));
                                                for (int j = i - 1; j < i + 3; j++)
                                                {
                                                    LetterWidthAndCount buf = lW[j];
                                                    buf.count = 0;
                                                    lW[j] = buf;
                                                }
                                                if (i - 2 > -1)
                                                    GettingGenericWidths(i - 2, 0);
                                                if (i + 3 < lW.Count)
                                                    GettingGenericWidths(i + 3, 0);
                                            }
                                            else
                                            {
                                                sbyte s = IsNotAllEmpty();
                                                if (s != -1)
                                                    GettingGenericWidths(IsNotAllEmpty(), prevCounter);
                                                else
                                                    return;
                                            }
                                        }
                                        else
                                        {
                                            generic.Add(new LetterWidthAndCount(lW[i].width, (byte)(lW[i].count + rSum + lSum)));
                                            for (int j = i - 1; j < i + 3; j++)
                                            {
                                                LetterWidthAndCount buf = lW[j];
                                                buf.count = 0;
                                                lW[j] = buf;
                                            }
                                            if (i - 2 > -1)
                                                GettingGenericWidths(i - 2, 0);
                                            if (i + 3 < lW.Count)
                                                GettingGenericWidths(i + 3, 0);
                                        }
                                        break;
                                    }
                            }
                            break;
                        }
                    case 2: // 2 слева
                        {

                            if (i + 3 < lW.Count)
                            {
                                for (int j = i + 1; j < i + 3; j++)
                                    if (Math.Abs(lW[j].width - lW[i].width) < 3 && lW[j].count > 0)
                                    {
                                        counter++;
                                        rSum += lW[j].count;
                                    }
                            }
                            else
                            {
                                if (i != lW.Count)
                                    for (int j = i + 1; j < lW.Count; j++)
                                        if (Math.Abs(lW[j].width - lW[i].width) < 3 && lW[j].count > 0)
                                        {
                                            counter++;
                                            rSum += lW[j].count;
                                        }
                            }
                            switch (counter)
                            {
                                case 2: //2 слева и 0 справа
                                    {
                                        if (prevCounter == 0)
                                        {
                                            GettingGenericWidths(i - 2, 2);
                                            bool isUsedElemsExist = false;
                                            for (int j = i - 1; i < i + 1; j++)
                                                if (lW[j].count == 0)
                                                {
                                                    isUsedElemsExist = true;
                                                    break;
                                                }
                                            if (!isUsedElemsExist)
                                            {
                                                generic.Add(new LetterWidthAndCount(lW[i].width, (byte)(lW[i].count + lSum)));
                                                for (int j = i - 2; j < i + 1; j++)
                                                {
                                                    LetterWidthAndCount buf = lW[j];
                                                    buf.count = 0;
                                                    lW[j] = buf;
                                                }
                                                if (i + 1 < lW.Count)
                                                    GettingGenericWidths(i + 1, 0);
                                                if (i - 3 > -1)
                                                    GettingGenericWidths(i - 3, 0);
                                            }
                                            else
                                            {
                                                sbyte s = IsNotAllEmpty();
                                                if (s != -1)
                                                    GettingGenericWidths(IsNotAllEmpty(), prevCounter);
                                                else
                                                    return;
                                            }
                                        }
                                        else
                                        {
                                            if (counter < prevCounter)
                                                return;
                                            else
                                            {
                                                generic.Add(new LetterWidthAndCount(lW[i].width, (byte)(lW[i].count + lSum)));
                                                for (int j = i - 2; j < i + 1; j++)
                                                {
                                                    LetterWidthAndCount buf = lW[j];
                                                    buf.count = 0;
                                                    lW[j] = buf;
                                                }
                                                if (i + 1 < lW.Count)
                                                    GettingGenericWidths(i + 1, 0);
                                                if (i - 3 > -1)
                                                    GettingGenericWidths(i - 3, 0);
                                            }
                                        }
                                        break;

                                    }
                                case 3: //2 слева и 1 справа
                                    {
                                        if (prevCounter == 0)
                                        {
                                            GettingGenericWidths(i - 2, 3);
                                            GettingGenericWidths(i + 1, 3); bool isUsedElemsExist = false;
                                            for (int j = i - 1; i < i + 1; j++)
                                                if (lW[j].count == 0)
                                                {
                                                    isUsedElemsExist = true;
                                                    break;
                                                }
                                            if (!isUsedElemsExist)
                                            {
                                                generic.Add(new LetterWidthAndCount(lW[i].width, (byte)(lW[i].count + rSum + lSum)));
                                                for (int j = i - 2; j < i + 2; j++)
                                                {
                                                    LetterWidthAndCount buf = lW[j];
                                                    buf.count = 0;
                                                    lW[j] = buf;
                                                }
                                                if (i + 2 < lW.Count)
                                                    GettingGenericWidths(i + 2, 0);
                                                if (i - 3 > -1)
                                                    GettingGenericWidths(i - 3, 0);
                                            }
                                            else
                                            {
                                                sbyte s = IsNotAllEmpty();
                                                if (s != -1)
                                                    GettingGenericWidths(IsNotAllEmpty(), prevCounter);
                                                else
                                                    return;
                                            }
                                        }
                                        else
                                        {
                                            generic.Add(new LetterWidthAndCount(lW[i].width, (byte)(lW[i].count + rSum + lSum)));
                                            for (int j = i - 2; j < i + 2; j++)
                                            {
                                                LetterWidthAndCount buf = lW[j];
                                                buf.count = 0;
                                                lW[j] = buf;
                                            }
                                            if (i + 2 < lW.Count)
                                                GettingGenericWidths(i + 2, 0);
                                            if (i - 3 > -1)
                                                GettingGenericWidths(i - 3, 0);
                                        }
                                        break;
                                    }
                                case 4: //2 слева и 2 справа
                                    {
                                        generic.Add(new LetterWidthAndCount(lW[i].width, (byte)(lSum + rSum + lW[i].count)));
                                        for (int j = i - 2; j < i + 3; j++)
                                        {
                                            LetterWidthAndCount buf = lW[j];
                                            buf.count = 0;
                                            lW[j] = buf;
                                        }
                                        if (i + 3 < lW.Count)
                                            GettingGenericWidths(i + 3, 0);
                                        if (i - 3 > -1)
                                            GettingGenericWidths(i - 3, 0);
                                        break;
                                    }
                            }
                            break;
                        }
                }
            }
            else
            {
                sbyte curHalf = IsNotAllEmpty();
                if (curHalf != -1)
                    GettingGenericWidths(curHalf, 0);
            }
        }
        /// <summary>
        /// Функция возвращает индекс первого встреченного непросмотренного элемента.
        /// </summary>
        /// <returns> Индекс элемента i.</returns>
        sbyte IsNotAllEmpty()
        {
            sbyte c = (sbyte)lW.Count;
            for (sbyte i = 0; i < c; i++)
                if (lW[i].count != 0)
                    return i;
            return -1;
        }
        /// <summary>
        /// Метод ищет самый распространенный элемнет из обобщения.
        /// </summary>
        /// <returns></returns>
        byte MostRescentGenricElem()
        {
            byte mResult = 0;
            byte result = 0;
            foreach (LetterWidthAndCount elem in generic)
                if (elem.count > mResult)
                {
                    mResult = elem.count;
                    result = elem.width;
                }
            return result;
        }
    }
}
