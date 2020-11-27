using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
namespace WpfKa
{
    class Operations
    {
        /// <summary>
        /// Метод, производящий бинаризацию картинки по 1 из 3х компонент - R, G или B, а также разделяющий исходный набор пикселей изображения на строки.
        /// </summary>
        /// <param name="component"> Цветовая компонента, по которой будет происходить бинаризцаия.</param>
        /// <param name="width"> Ширина картинки.</param>
        /// <param name="heigth"> Высота картинки</param>
        public static List<AmountAndType>  Binarization(byte component, int width, 
            int heigth, Bitmap image, byte[,] picPixels)
        {
            //Провожу бинаризацию по определенному цвету, используя порогувую функцию. 
            //     { 0, если величина цветовая пикселя < 128   
            // F = {
            //     { 1, иначе
            if (component != 0)
            {
                List<AmountAndType> columns = new List<AmountAndType>();
                bool isBlack;
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
                return columns;
            }
            else
            {
                List<AmountAndType> columns = DivisionByColumns(heigth, width, image, picPixels);
                AdditionalColumnsCheck(ref columns);
                return columns;
            }
        }
        /// <summary>
        /// Метод для нахождения самой распространненной цветовой компоненты изображения.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <returns></returns>
        public static byte FindColorComponentOfTheImage(Bitmap image, int w, int h)
        {
            uint white = 0;
            uint green = 0;//компоненты цвета
            uint blue = 0;
            uint red = 0;
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
                switch (green > blue)//Нахожу самый распространенный цвет. 
                {                    
                    case true:
                        {
                            switch (red > green)
                            {
                                case true:
                                    return 1;
                                case false:
                                    return 2;
                            }
                            break;
                        }
                    case false:
                        {
                            switch (red > blue)
                            {
                                case true:
                                    return 1;
                                case false:
                                    return 3;
                            }
                            break;
                        }
                }
            }
            return 0;    
        }
        /// <summary>
        /// Дополнительная разрезка строки - удаление мусора, группировка хвостиков и основных частей букв.
        /// </summary>
        /// <param name="genStrOffset"> Тип отступа.</param>
        private static void SecondColumnsCut(uint genStrOffset, List<AmountAndType> columns)
        {
            int i = 0;
            if (columns[i].type != 0)
                i++;
            while (i < columns.Count)
            {
                if (columns[i].amount <= 0.3 * genStrOffset && i > 0 && i + 1 < columns.Count)
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
                    i += 2;
            }
        }
        /// <summary>
        /// Метод, вычисляющий самый распространенный отступ между строк на картинке.
        /// Рассматриваю 3 типа отступов: маленький(до 10пикселей), средний(10-20), большой(20-30).
        /// </summary>
        private static void AdditionalColumnsCheck(ref List<AmountAndType> columns)
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
                                        nCounter++;
                                        break;
                                    }
                                case false:

                                    {
                                        bCounter++;
                                        break;
                                    }
                            }
                            break;
                        }
                }
                i += 2;
            }
            switch (nCounter > bCounter)
            {
                case true:
                    {
                        switch (nCounter > sCounter)
                        {
                            case true:
                                {
                                    SecondColumnsCut(20, columns);
                                    break;
                                }
                            case false:
                                {
                                    SecondColumnsCut(10, columns);
                                    break;
                                }
                        }
                        break;
                    }
                case false:
                    {
                        switch (bCounter > sCounter)
                        {
                            case true:
                                {
                                    SecondColumnsCut(30, columns);
                                    break;
                                }
                            case false:
                                {
                                    SecondColumnsCut(10, columns);
                                    break;
                                }
                        }
                        break;
                    }
            }

        }
        /// <summary>
        /// Метод, разрезающий изображение с текстом на строки, если картинке не нужна бинаризация.
        /// </summary>
        /// <param name="heigth"> Высота картинки.</param>
        /// <param name="width"> Ширина картинки. </param>
        public static List<AmountAndType> DivisionByColumns(int heigth, int width, Bitmap image,byte[,] picPixels)
        {
            List<AmountAndType> columns = new List<AmountAndType>();
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
            return columns;
        }

    }
}
