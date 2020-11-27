using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using WindowsInput.Native;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Threading;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Windows.Markup.Localizer;

namespace WpfKa
{
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetCursorPos(out System.Drawing.Point lpPoint);
        /// <summary>
        /// Структура для подсчета ширины буквы. Используется для уточнения разделения строк на символы.
        /// </summary>
        public struct LetterWidthAndCount: IComparable
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
        /// <summary>
        /// Структура для подсчета строк,в которыххотябы 1 пиксель закрашен, и пустых строк.
        /// </summary>
        public struct AmountAndType
        {
            public ushort amount;
            public byte type;
            public AmountAndType(ushort amount, byte type)
            {
                this.amount = amount;
                this.type = type;
            }
        }
        /// <summary>
        /// Структура для хранения данных о массиве пикселей - сам массив, высота и ширина.
        /// /// </summary>
        public struct PixelArrayAndParams
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
        /// <summary>
        /// Метод, разрезающий изображение с текстом на строки, если картинке не нужна бинаризация.
        /// </summary>
        /// <param name="heigth"> Высота картинки.</param>
        /// <param name="width"> Ширина картинки. </param>
        public void DivisionByColumns(int heigth, int width)
        {
            bool isBlack = false;
            bool previsBlack = false;
            ushort counter = 0;
            for (int i = 0; i < heigth; i++)
            {
                isBlack = false;
                for (int j = 0; j < width; j++)
                {
                    System.Drawing.Color curPixel = image.GetPixel(j, i);
                    if (curPixel.R < 128 || curPixel.G < 128 || curPixel.B < 128)
                    {
                        picPixels[i, j] = 1;
                        isBlack = true;
                    }
                    else
                        picPixels[i, j] = 0;
                }
                if (i == 0) //Подсчет пустых строк.
                    switch (isBlack)
                    {
                        case true:
                            {
                                counter++;
                                previsBlack = true;
                                break;
                            }
                        case false:
                            {
                                counter++;
                                break;
                            }
                    }
                else if (previsBlack == isBlack) //Если предыдущая строка тоже содержит\не содержит единиц, увеличиваем счетчик.
                {                            //Иначе - пишем имеющееся количество, ставим счетчик в 1, т.к. новый элемент не совпадает с предыдущими.
                    counter++;               //После чего меняем значение предыдущего элемента.
                }
                else
                {
                    if (previsBlack)
                        columns.Add(new AmountAndType(counter, 1));
                    else
                        columns.Add(new AmountAndType(counter, 0));
                    previsBlack = isBlack;
                    counter = 1;
                }
            }
            if (previsBlack)
                columns.Add(new AmountAndType(counter, 1));
            else
                columns.Add(new AmountAndType(counter, 0));
            
        }
        /// <summary>
        /// Метод, производящий бинаризацию картинки по 1 из 3х компонент - R, G или B, а также разделяющий исходный набор пикселей изображения на строки.
        /// </summary>
        /// <param name="component"> Цветовая компонента, по которой будет происходить бинаризцаия.</param>
        /// <param name="width"> Ширина картинки.</param>
        /// <param name="heigth"> Высота картинки</param>
        public void Binarization(byte component, int width, int heigth)
        {
            //Провожу бинаризацию по определенному цвету, используя порогувую функцию. 
            //     { 0, если величина цветовая пикселя < 128   
            // F = {
            //     { 1, иначе   
            bool isBlack = false;
            bool previsBlack = false;
            ushort counter = 0;
            for (int j = 0; j < heigth; j++)
            {
                isBlack = false;
                for (int i = 0; i < width; i++)
                {

                    switch (component)
                    {
                        case 1:
                            {
                                if (image.GetPixel(i, j).R > 128)
                                {
                                    image.SetPixel(i, j, System.Drawing.Color.Black);
                                    isBlack = true;
                                    picPixels[j, i] = 1;
                                }
                                else
                                {
                                    image.SetPixel(i, j, System.Drawing.Color.White);
                                    picPixels[j, i] = 0;
                                }
                                break;
                            }
                        case 2:
                            {
                                if (image.GetPixel(i, j).G > 128)
                                {
                                    image.SetPixel(i, j, System.Drawing.Color.Black);
                                    isBlack = true;
                                    picPixels[j, i] = 1;
                                }
                                else
                                {
                                    image.SetPixel(i, j, System.Drawing.Color.White);
                                    picPixels[j, i] = 0;
                                }
                                break;
                            }
                        case 3:
                            {
                                if (image.GetPixel(i, j).B > 128)
                                {
                                    image.SetPixel(i, j, System.Drawing.Color.Black);
                                    isBlack = true;
                                    picPixels[j, i] = 1;
                                }
                                else
                                {
                                    image.SetPixel(i, j, System.Drawing.Color.White);
                                    picPixels[j, i] = 0;
                                }
                                break;
                            }
                    }
                }
                if (j == 0) //Подсчет пустых строк.
                    switch (isBlack)
                    {
                        case true:
                            {
                                counter++;
                                previsBlack = true;
                                break;
                            }
                        case false:
                            {
                                counter++;
                                break;
                            }
                    }
                else if (previsBlack == isBlack) //Если предыдущая строка тоже содержит\не содержит единиц, увеличиваем счетчик.
                {                            //Иначе - пишем имеющееся количество, ставим счетчик в 1, т.к. новый элемент не совпадает с предыдущими.
                    counter++;               //После чего меняем значение предыдущего элемента.
                }
                else
                {
                    if (previsBlack)
                        columns.Add(new AmountAndType(counter, 1));
                    else
                        columns.Add(new AmountAndType(counter, 0));
                    previsBlack = isBlack;
                    counter = 1;
                }
            }
            if (previsBlack)
                columns.Add(new AmountAndType(counter, 1));
            else
                columns.Add(new AmountAndType(counter, 0));
        }
        List<LetterWidthAndCount> generic = new List<LetterWidthAndCount>();
        List<LetterWidthAndCount> lW = new List<LetterWidthAndCount>();
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
            for (sbyte i = 0; i <c; i++)
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
        /// <summary>
        /// Метод транспонирует массив данных о букве.
        /// </summary>
        /// <param name="letter"></param>
        /// <param name="heigth"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        byte[,] Transpose(List<List<byte>> letter, int heigth, int width)
        {
            byte[,] result = new byte[heigth, width];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < heigth; j++)
                {
                    result[j, i] = letter[i][j];
                }
            }
            return result;
        }        
        int counter2 = 0;
        PixelArrayAndParams GettingLastLetter(List<int> lastColumns, PixelArrayAndParams letter, int heigth, int firstElem, int iter, 
            List<int> numsWhite)
        {
            byte[,] newLetter = new byte[heigth, lastColumns.Count + 1];
            byte counter = 0;
            foreach (int i in lastColumns)
            {
                for (int j = 0; j < heigth; j++)
                {
                    newLetter[j, counter] = letter.pixelArray[j, i];
                }
                counter++;
            }
            
            return new PixelArrayAndParams(newLetter, heigth, lastColumns.Count);
        }
        int counter4 = 0;//ненужный счетчик
        /// <summary>
        /// Метод, проводящий доп. обрезку символов с помощью эрозии.
        /// К каждому символу из списка проверяется операция эрозии. После этого, по обработанному изображению ищу пустые столбцы
        /// и режу по ним исходник. Работает хорошо.
        /// </summary>
        /// <param name="lettersN"> Все данные о полученных ранее символах.</param>
        /// <param name="mostRecentWidth"> Самая распространенная ширина.</param>
        void Erosion(List<PixelArrayAndParams> lettersN, byte mostRecentWidth)
        {
            //Список для хранения номеров пустых столбцов.
            List<int> numsOfWhiteColumns = new List<int>();
            int i1 = 0;
            for (int iter = 0; iter < lettersN.Count; iter++)
            {
                if (lettersN[iter].width < 1.5 * mostRecentWidth)
                    continue;
                else 
                {
                    bool chStructFig = true;
                    //массив, в который копирую всю информацию 
                    //о пикселях символа, поскольку при копировании из
                    //списка просиходит копирование по ссылке, значит
                    //при применении эрозии я потеряю исходники.
                    byte[,] curArray = new byte[lettersN[iter].heigth, lettersN[iter].width];
                    for (int i = 0; i < lettersN[iter].heigth; i++)
                        for (int j = 0; j < lettersN[iter].width; j++)
                            curArray[i, j] = lettersN[iter].pixelArray[i, j];
                    // Применяю эрозию, параллельно ищу пустые столбцы.
                    for (int i = 0; i < lettersN[iter].width; i++)
                    {
                        bool isWhite = true;
                        for (int j = 0; j < lettersN[iter].heigth; j++)// Фигура эрозии: (*) *
                                                                       //                 *
                        {
                            if (curArray[j, i] == 1)
                            {
                                if (j + 1 < lettersN[iter].heigth)
                                    if (curArray[j + 1, i] != 1)
                                    {
                                        curArray[j, i] = 2;
                                        chStructFig = false; //Если первая фигура не дала результата, пробуем вторую.
                                    }
                                if (chStructFig)
                                    if (i + 1 < lettersN[iter].width)
                                        if (curArray[j, i + 1] != 1)
                                            curArray[j, i] = 2;
                                if (curArray[j, i] == 1)
                                    isWhite = false;
                            }
                            chStructFig = true;
                        }
                        // Пoиск пустых столбцов.
                        if (lettersN[iter].width > 1.5 * mostRecentWidth)
                            if (isWhite)
                            {
                                if (i >= 0.5 * mostRecentWidth && i <= lettersN[iter].width - mostRecentWidth)
                                    numsOfWhiteColumns.Add(i);
                            }
                    }
                    //Вывод результата эрозии. Лишь для тестов.
                    System.Drawing.Bitmap b = new System.Drawing.Bitmap(lettersN[iter].width, lettersN[iter].heigth);
                    for (int i = 0; i < lettersN[iter].heigth; i++)
                        for (int j = 0; j < lettersN[iter].width; j++)
                            if (curArray[i, j] == 1)
                                b.SetPixel(j, i, System.Drawing.Color.Black);
                            else
                            {
                                b.SetPixel(j, i, System.Drawing.Color.White);
                                curArray[i, j] = 0;
                            }
                    using (FileStream ms = new FileStream("C:\\Users\\gorka\\OneDrive\\Рабочий стол\\OCR\\Erosion\\" +
                        counter2 + ".png", FileMode.Create))
                    using (Bitmap i2 = (Bitmap)b.Clone())
                    {
                        i2.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        counter2++;
                    }//Удалить до сих пор.
                    
                    //Пост-обработка найденных номеров пустых столбцов.    
                    i1 = 0;
                    if (iter != null)
                    {
                        iter++;
                        iter--;
                    }
                    if (numsOfWhiteColumns.Count > 0)
                    {
                        while (i1 < numsOfWhiteColumns.Count)
                        {
                            if (i1 + 1 < numsOfWhiteColumns.Count)
                                if (numsOfWhiteColumns[i1 + 1] - numsOfWhiteColumns[i1] < 0.35 * mostRecentWidth)
                                    numsOfWhiteColumns.RemoveAt(i1);
                                else
                            if (numsOfWhiteColumns[i1 + 1] - numsOfWhiteColumns[i1] < 0.6 * mostRecentWidth)
                                    numsOfWhiteColumns.RemoveAt(i1 + 1);
                                else
                                    i1++;
                            else
                                i1++;
                        }
                        //Список для букв в имеющемся символе.
                        //Используется в том случае, если в рассматриваемом
                        //символе оказалось более одной буквы.
                        List<PixelArrayAndParams> ppap = new List<PixelArrayAndParams>();
                        i1 = 0;
                        //Обрезка букв внутрrи исходника по найденным границам.
                        //Крайний элемент(rEdge) включается в букву.
                        for (int rEdge = 0; rEdge < numsOfWhiteColumns.Count; rEdge++)
                        {
                            byte[,] newLetter;
                            //Здесь довольно шляпный момент, нужно учесть несколько вещей:
                            //1. Если мы только начали, то все понятно: нужно определить границу слева
                            //и отрезать букву. Мб, что буква большая, и ее ширина превышает самое распространенное
                            //значение ширины. Это учтено в условии (2)
                            //2. Если у нас в 1 символ попало несколько букв и сейчас мы рассматриваем не первую
                            //область буквы, есть шанс, что следующая найденная буква будет следующей буквой в слове, 
                            //как при прочтении. В таком случае, не нужно вычислять левую границу для обрезки.
                            //Следует просто отрезать символ от пред. найденной границы до текущей.
                            //В ином случае, придется выяислять левую границу для символа и обрубать его по ней (3).
                            if (rEdge == 0)
                            {
                                if (numsOfWhiteColumns[rEdge] % mostRecentWidth < 0.4 * mostRecentWidth)//(2)
                                {
                                    newLetter = new byte[lettersN[iter].heigth, numsOfWhiteColumns[rEdge] - (numsOfWhiteColumns[rEdge] /
                                        mostRecentWidth - 1) * mostRecentWidth + 1];
                                    for (int i = (numsOfWhiteColumns[rEdge] / mostRecentWidth - 1) * mostRecentWidth;
                                        i <= numsOfWhiteColumns[rEdge]; i++)
                                    {
                                        for (int j = 0; j < lettersN[iter].heigth; j++)
                                        {
                                            newLetter[j, i] = lettersN[iter].pixelArray[j, i];
                                        }
                                    }
                                    ppap.Add(new PixelArrayAndParams(newLetter, lettersN[iter].heigth,
                                        numsOfWhiteColumns[rEdge] - (numsOfWhiteColumns[rEdge] /
                                        mostRecentWidth - 1) * mostRecentWidth + 1));
                                }
                                else
                                {
                                    newLetter = new byte[lettersN[iter].heigth, numsOfWhiteColumns[rEdge] + 1];
                                    for (int i = (numsOfWhiteColumns[rEdge] / mostRecentWidth) * mostRecentWidth;
                                        i <= numsOfWhiteColumns[rEdge]; i++)
                                    {
                                        for (int j = 0; j < lettersN[iter].heigth; j++)
                                        {
                                            newLetter[j, i] = lettersN[iter].pixelArray[j, i];
                                        }
                                    }
                                    ppap.Add(new PixelArrayAndParams(newLetter, lettersN[iter].heigth,
                                        numsOfWhiteColumns[rEdge] % mostRecentWidth + 1));
                                }
                            }
                            else
                            {
                                if (numsOfWhiteColumns[rEdge] - numsOfWhiteColumns[rEdge - 1] < 1.5 * mostRecentWidth)//(3)
                                {
                                    newLetter = new byte[lettersN[iter].heigth, numsOfWhiteColumns[rEdge] -
                                        numsOfWhiteColumns[rEdge - 1] + 1];
                                    byte counter = 0;
                                    for (int i = numsOfWhiteColumns[rEdge - 1] + 1;
                                        i <= numsOfWhiteColumns[rEdge]; i++)
                                    {
                                        for (int j = 0; j < lettersN[iter].heigth; j++)
                                        {
                                            newLetter[j, counter] = lettersN[iter].pixelArray[j, i];
                                        }
                                        counter++;
                                    }
                                    ppap.Add(new PixelArrayAndParams(newLetter, lettersN[iter].heigth,
                                        counter + 1));
                                }
                                else
                                {
                                    byte counter = 0;
                                    newLetter = new byte[lettersN[iter].heigth, numsOfWhiteColumns[rEdge] - (numsOfWhiteColumns[rEdge] /
                                        mostRecentWidth) * mostRecentWidth + 1];
                                    for (int i = numsOfWhiteColumns[rEdge] - (numsOfWhiteColumns[rEdge] /
                                        mostRecentWidth) * mostRecentWidth; i <= numsOfWhiteColumns[rEdge]; i++)
                                    {
                                        for (int j = 0; j < lettersN[iter].heigth; j++)
                                        {
                                            newLetter[j, counter] = lettersN[iter].pixelArray[j, i];
                                        }
                                        counter++;
                                    }
                                    ppap.Add(new PixelArrayAndParams(newLetter, lettersN[iter].heigth,
                                        counter + 1));
                                }
                            }
                        }
                        // Найдя все буквы внутри символа, необходимо отделить остаток.
                        // Это происходит след. образом: завожу список с номерами всех столбцов
                        //по ширине символа. Если я могу связать столбец с одной из найденных 
                        //границ - удаляю его из списка.
                        //Связывание столбца с границей происходит подобно обрезке символов:
                        //Те же условия для поиска промежутков с использованными столбцами, что и
                        //описанные ранее.
                        List<int> columnsNumbers = new List<int>();
                        columnsNumbers.AddRange(Enumerable.Range(0, lettersN[iter].width));
                        PixelArrayAndParams debug = lettersN[iter];
                        for (int rEdge = 0; rEdge < numsOfWhiteColumns.Count; rEdge++)
                        {
                            if (rEdge == 0)
                            {
                                if (numsOfWhiteColumns[rEdge] % mostRecentWidth < 0.4 * mostRecentWidth)
                                    for (int i = (numsOfWhiteColumns[rEdge] / mostRecentWidth - 1) *
                                    mostRecentWidth; i <= numsOfWhiteColumns[rEdge]; i++)
                                        columnsNumbers.Remove(i);
                                else
                                    for (int i = (numsOfWhiteColumns[rEdge] / mostRecentWidth) *
                                        mostRecentWidth; i <= numsOfWhiteColumns[rEdge]; i++)
                                        columnsNumbers.Remove(i);
                            }
                            else
                            {
                                if (numsOfWhiteColumns[rEdge] - numsOfWhiteColumns[rEdge - 1] < 1.4 * mostRecentWidth)
                                {
                                    for (int i = numsOfWhiteColumns[rEdge - 1] + 1; i <= numsOfWhiteColumns[rEdge]; i++)
                                        columnsNumbers.Remove(i);
                                }
                                else
                                {
                                    for (int i = (numsOfWhiteColumns[rEdge] / mostRecentWidth) *
                                    mostRecentWidth; i <= numsOfWhiteColumns[rEdge]; i++)
                                        columnsNumbers.Remove(i);
                                }
                            }
                        }
                        if (ppap.Count > 0)
                        {
                            i1 = ppap.Count;
                            while (i1 != 0)
                            {
                                lettersN.Insert(iter, ppap[i1 - 1]);
                                i1--;
                            }
                            iter += ppap.Count;
                        }
                        ppap.Clear();
                        numsOfWhiteColumns.Clear();
                        //После того, как убрал все лишние столбцы, обрезаю исходник.
                        // Вставка найденных букв, не вяляющихся крайней в список все символов.
                        lettersN[iter] = GettingLastLetter(columnsNumbers, lettersN[iter], lettersN[iter].heigth,
                            columnsNumbers[0], iter, numsOfWhiteColumns);
                    }
                }
            }
            i1 = 0;
            //Вывод результата преобразований. Только для теста?
            foreach (PixelArrayAndParams element in lettersN)
            {
                System.Drawing.Bitmap b1 = new System.Drawing.Bitmap(element.width, element.heigth);
                for (int i = 0; i < element.width; i++)
                {
                    for (int j = 0; j < element.heigth; j++)
                        switch (element.pixelArray[j, i])
                        {
                            case 1:
                                {
                                    b1.SetPixel(i, j, System.Drawing.Color.Black);
                                    break;
                                }
                            case 0:
                                {
                                    b1.SetPixel(i, j, System.Drawing.Color.White);
                                    break;
                                }
                        }
                }
                using (FileStream ms = new FileStream("C:\\Users\\gorka\\OneDrive\\Рабочий стол\\OCR\\LettersAfterErosionAnalysis\\" +
                    counter4 + ".png", FileMode.Create))
                using (Bitmap i2 = (Bitmap)b1.Clone())
                {
                    i2.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    counter4++;
                }

            }//Удалить до сих пор.
        }
        bool gc = false;
        byte mostRescentElem = 0;
        int counter1 = 0;//Счетчик для нумерации картинок
        /// <summary>
        /// Метод, разрезающий общий набор пикселей на строки и символы.
        /// Одно из больших частей, мякоток...
        /// </summary>
        /// <param name="l"> Верхняя точка строки.</param>
        /// <param name="r"> Нижняя точка строки.</param>
        /// <param name="width"> Ширина строки.</param>
        /// <returns></returns>
        List<PixelArrayAndParams> PixelsCopy(ushort l, ushort r, int width)
        {
            List<PixelArrayAndParams> letters = new List<PixelArrayAndParams>();
            List<List<byte>> letter = new List<List<byte>>();
            List<byte> row = new List<byte>();
            using (StreamWriter f_out = new StreamWriter("pixelsString.txt", false))
            for (int j = 0; j < width; j++)
            {
                bool isBlack = false;
                for (int i = l; i < r; i++)
                {
                    row.Add(picPixels[i, j]);
                        f_out.Write(picPixels[i, j] + " ");
                    if (picPixels[i, j] == 1)
                        isBlack = true;
                }
                    f_out.Write("\n");
                if (isBlack)
                    letter.Add(new List<byte>(row));
                else
                if (letter.Count != 0)
                {
                    letters.Add(new PixelArrayAndParams(Transpose(letter, r - l, letter.Count), row.Count, letter.Count));
                    bool isMatched = false;
                    for (int i = 0; i < lW.Count; i++)
                        if (letter.Count == lW[i].width)
                        {
                            LetterWidthAndCount v = lW[i];
                            v.count++;
                            lW[i]= v;
                            isMatched = true;
                        }
                    if (!isMatched)
                        lW.Add(new LetterWidthAndCount((byte)letter.Count, 1));
                    //Отрисовка получаемых букв - лишь для теста.
                    System.Drawing.Bitmap b = new System.Drawing.Bitmap(letter.Count, r - l);
                    for (int i = 0; i < r - l; i++)
                    {
                        for (int k = 0; k < letter.Count; k++)
                        {
                            switch (letter[k][i])
                            {
                                case 1:
                                    {
                                        b.SetPixel(k, i, System.Drawing.Color.Black);
                                        break;
                                    }
                                case 0:
                                    {
                                        b.SetPixel(k, i, System.Drawing.Color.White);
                                        break;
                                    }
                            }
                        }
                    }
                    using (FileStream ms = new FileStream("C:\\Users\\gorka\\OneDrive\\Рабочий стол\\OCR\\Letters\\" + 
                        counter1 + ".png", FileMode.Create))
                    using (Bitmap i1 = (Bitmap)b.Clone())
                    {
                        i1.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    }
                    //конец отрисовки. Удалить до сюда.
                    letter.Clear();
                    counter1++;
                }
                row.Clear();
            }
            lW.Sort();// По найденным ширинам символов объединяю их.
            if (lW.Count > 2)
                    GettingGenericWidths(2, 0);
                else
                    GettingGenericWidths(lW.Count - 1, 0);
            if (!gc)
            {
                mostRescentElem = MostRescentGenricElem(); //Нахожу самую распространенную ширину из объединенных.
                gc = true;
            }
            lW.Clear();
            generic.Clear();//Очищаю глобальные списки, использованные для поиск mostRecentWidth.
            Erosion(letters, mostRescentElem);//Следующая мякотка программы...
            return letters;
        }
        /// <summary>
        /// Эмуляция нажатия кнопки Prt Scr. Скриншот помещается по адресу "D:/1.jpeg", после чего область около курсора обрезается до прямоугольника. 
        /// </summary>
        /// <param name="mcords"> 
        /// Координаты курсора относительно экрана. 
        /// </param>
        public void PrintScreen(System.Drawing.Point mcords)
        {
            Bitmap printscreen = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            Graphics graphics = Graphics.FromImage(printscreen as System.Drawing.Image);
            graphics.CopyFromScreen(0, 0, 0, 0, printscreen.Size);
            Bitmap newScrn = printscreen.Clone(new System.Drawing.Rectangle((int)mcords.X - 150, (int)mcords.Y - 50, 300, 100), System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            newScrn.Save("D:/1.jpeg", System.Drawing.Imaging.ImageFormat.Jpeg);
        }
        public MainWindow()
        {
            InitializeComponent();
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            //timer.Start(); //ОБЯЗАТЕЛЬНО ВКЛЮЧИТЬ!!!!
        }
        System.Drawing.Point mousePos;
        System.Drawing.Bitmap image;
        /// <summary>
        /// Таймер, проверяющий, поменялось ли расположение мыши на экране в течение секунды.
        /// </summary>
        /// <param name="sender"> Формальность.</param>
        /// <param name="e"> Формальность.....</param>
        void timer_Tick(object sender, EventArgs e)
        {
            System.Drawing.Point newMousePoint = new System.Drawing.Point(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
            GetCursorPos(out newMousePoint);
            if (mousePos == newMousePoint)
            {
                PrintScreen(mousePos);
                System.Windows.MessageBox.Show("AYE");
            }
            else
                mousePos = newMousePoint;
        }
        uint white = 0;
        uint green = 0;//компоненты цвета
        uint blue = 0;
        uint red = 0;
        List<AmountAndType> columns;//массивы для определния ширины символа и его высоты
        byte[,] picPixels;
        List<PixelArrayAndParams> letters = new List<PixelArrayAndParams>();
        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);
        /// <summary>
        /// Дополнительная разрезка строки - удаление мусора, группировка хвостиков и основных частей букв.
        /// </summary>
        /// <param name="genStrOffset"> Тип отступа.</param>
        void SecondColumnsCut(uint genStrOffset)
        {
            int i = 0;
            if (columns[i].type != 0)
                i++;
            while (i < columns.Count)
            {
                if(columns[i].amount <= 0.3 * genStrOffset && i > 0 && i + 1 < columns.Count) 
                {
                    if (columns[i - 1].amount <= 0.3 * genStrOffset)
                    {
                        //Прибавляем к следующей последовательности строк,содержащих 1: i, i-1 строки.
                        AmountAndType a = columns[i + 1];
                        a.amount += (ushort)(columns[i].amount + columns[i - 1].amount);
                        columns[i + 1] = a;
                        columns.RemoveAt(i);
                        columns.RemoveAt(i - 1);
                    }
                    else
                        i++;
                }
                else 
                    i+=2;
            }
        }
        /// <summary>
        /// Метод, вычисляющий самый распространенный отступ между строк на картинке.
        /// Рассматриваю 3 типа отступов: маленький(до 10пикселей), средний(10-20), большой(20-30).
        /// </summary>
        void AdditionalColumnsCheck()
        {
            uint sCounter = 0;
            uint nCounter = 0;
            uint bCounter = 0;
            int i = 0;
            if (columns[i].type != 0)
                i++;
            while (i < columns.Count)
            {
                switch (columns[i].amount < 10)
                {
                    case true:
                        {
                            sCounter++;
                            break;
                        }
                    case false:
                        {
                            switch (columns[i].amount < 20)
                            {
                                case true:
                                    {
                                        nCounter ++;
                                        break;
                                    }
                                case false:

                                    {
                                        bCounter ++;
                                        break;
                                    }
                            }
                            break;
                        }
                }
                i += 2;
            }
            switch(nCounter > bCounter)
            {
                case true:
                    {
                        switch (nCounter > sCounter)
                        {
                            case true:
                                {
                                    SecondColumnsCut(20);
                                    break;
                                }
                            case false:
                                {
                                    SecondColumnsCut(10);
                                    break;
                                }
                        }
                        break;
                    }
                case false:
                    {
                        switch(bCounter > sCounter)
                        {
                            case true:
                                {
                                    SecondColumnsCut(30);
                                    break;
                                }
                            case false:
                                {
                                    SecondColumnsCut(10);
                                    break;
                                }
                        }
                        break;
                    }
            }

        }
        /// <summary>
        /// При нажатии на кнопку выбирается несколько фотографий, после чего они по порогу переходят в ч\б.
        /// Тестовый пример, на котором разработал механику бинаризцаии ихображения.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fD = new OpenFileDialog();//открываю файл для выбора нескольких фото 
            fD.Multiselect = true;
            fD.ShowDialog();
            Stopwatch s = new Stopwatch();//таймер для замеров времени
            foreach (string elem in fD.FileNames)//далее действия происходят для каждого из них
            {
                DirectoryInfo d = new DirectoryInfo("C:\\Users\\gorka\\OneDrive\\Рабочий стол\\OCR\\Letters");
                if (d.Exists)
                {
                    d.Delete(true);
                    d.Create(); 
                }
                else
                    d.Create();
                d = new DirectoryInfo("C:\\Users\\gorka\\OneDrive\\Рабочий стол\\OCR\\LettersD\\");
                if (d.Exists)
                {
                    d.Delete(true);
                    d.Create();
                }
                else
                    d.Create();

                d = new DirectoryInfo("C:\\Users\\gorka\\OneDrive\\Рабочий стол\\OCR\\Dilatation\\");
                if (d.Exists)
                {
                    d.Delete(true);
                    d.Create();
                }
                else
                    d.Create();

                d = new DirectoryInfo("C:\\Users\\gorka\\OneDrive\\Рабочий стол\\OCR\\Erosion\\");
                if (d.Exists)
                {
                    d.Delete(true);
                    d.Create();
                }
                else
                    d.Create();
                DirectoryInfo dd = new DirectoryInfo("C:\\Users\\gorka\\OneDrive\\Рабочий стол\\OCR\\LettersAfterErosionAnalysis\\");
                if (dd.Exists)
                {
                    dd.Delete(true);
                    dd.Create();
                }
                else
                    dd.Create();

                image = new System.Drawing.Bitmap(elem);
                int w = image.Width;
                int h = image.Height;
                columns = new List<AmountAndType>();
                picPixels = new byte[h, w];
                s.Start();//старт таймера
                for (int iter1 = 0; iter1 < w; iter1++)     //В первом цикле определяю, каких пикселей больше - красных, зеленых или синих/
                    for (int iter2 = 0; iter2 < h; iter2++)
                    {
                        System.Drawing.Color curColor = image.GetPixel(iter1, iter2);
                        if (curColor.R > 240 & curColor.G > 240 & curColor.B > 240)
                            white++;
                        else if (curColor.R > 128)
                            red++;
                        else if (curColor.G > 128)
                            green++;
                        else if (curColor.B > 128)
                            blue++;
                    }
                if (white < green + blue || white < green + red || white < blue + red)
                {
                    switch (green > blue)//Нахожу самый распространенный цвет, после чего произвожу бинаризацию и обрезку строк
                    {                    //по этой цветовой компоненте, если изображение надо бинаризовывать.
                        case true:
                            {
                                switch (red > green)
                                {
                                    case true:
                                        Binarization(1, w, h);
                                        break;
                                    case false:
                                        Binarization(2, w, h);
                                        break;
                                }
                                break;
                            }
                        case false:
                            {
                                switch (red > blue)
                                {
                                    case true:
                                        Binarization(1, w, h);
                                        break;
                                    case false:
                                        Binarization(3, w, h);
                                        break;
                                }
                                break;
                            }
                    }
                }
                else
                    DivisionByColumns(h, w); //Если в изображении уже преобладают ч\б, то сразу режу его на строки.
                AdditionalColumnsCheck();//Дополнительная проверка разрезки на строки.
                List<PixelArrayAndParams> imageClassifiedByPixelLetters = new List<PixelArrayAndParams>();
                ushort counter = 0;
                foreach (AmountAndType element in columns)
                {
                    switch (element.type)
                    {
                        case 1:
                            {
                                imageClassifiedByPixelLetters.AddRange(PixelsCopy(counter, (ushort)(counter + element.amount), w));
                                counter += element.amount;
                                break;
                            }
                        case 0:
                            {
                                counter += element.amount;
                                break;
                            }
                    }
                }
                s.Stop();//остановка таймера
                string[] sKU = elem.Split('\\');//результаты кидаю по новому пути( этот кусок просто для тестов)
                sKU[sKU.Length - 2] = "TestResults";
                StringBuilder newPath = new StringBuilder();
                for (int iter = 0; iter < sKU.Length; iter++)
                {
                    newPath.Append(sKU[iter]);
                    if (iter != sKU.Length - 1)
                        newPath.Append("\\\\");
                }

                //Чисто для теста, начало:
                //запись файлов необходимо проводить именно таким образом, чтобы все корректно сохранялось на диске (запись нужна для промежуточного тестирования).
                using (FileStream ms = new FileStream(newPath.ToString(), FileMode.Create))
                using (Bitmap i1 = (Bitmap)image.Clone())
                {
                    i1.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                }
                System.Windows.MessageBox.Show(s.ElapsedMilliseconds.ToString());//Вывод времени, затраченного на бинаризацию и сброс таймера.
                s.Reset();
                //Чистодля теста, конец. Можно удалять при релизе.

                red = 0;
                blue = 0;
                green = 0;
            }
        }
        
        bool status = false;
        //перетаскивание картинки. Как на форме)
        private void Image1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            status = true;
        }

        private void Image1_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (status)
            {
                System.Windows.Controls.Image i1 = (System.Windows.Controls.Image)sender;
                i1.Margin = new Thickness(e.GetPosition(this).X - 20, e.GetPosition(this).Y - 20, 0, 0);
            }
        }
        private void Image1_MouseUp(object sender, MouseButtonEventArgs e)
        {
            status = false;
        }
    }
}
