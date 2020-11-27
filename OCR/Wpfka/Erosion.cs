using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;

namespace WpfKa
{
    class Erosion
    {
        private PixelArrayAndParams GettingLastLetter(List<int> lastColumns, PixelArrayAndParams letter, int heigth, int firstElem, int iter,
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
        int counter2 = 0;
        int counter4 = 0;//ненужный счетчик
        /// <summary>
        /// Метод, проводящий доп. обрезку символов с помощью эрозии.
        /// К каждому символу из списка проверяется операция эрозии. После этого, по обработанному изображению ищу пустые столбцы
        /// и режу по ним исходник. Работает хорошо.
        /// </summary>
        /// <param name="lettersN"> Все данные о полученных ранее символах.</param>
        /// <param name="mostRecentWidth"> Самая распространенная ширина.</param>
        public void UseErosion(ref List<PixelArrayAndParams> lettersN, byte mostRecentWidth)
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
    }
}
