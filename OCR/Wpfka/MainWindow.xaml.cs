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
        byte[,] picPixels;
        int ccounter = 0;
        List<PixelArrayAndParams> letters = new List<PixelArrayAndParams>();
        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);
        /// <summary>
        /// <para>ГЛАВНЫЙ МЕТОД ПРОЕКТА.</para>
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
                image = new System.Drawing.Bitmap(elem);
                int w = image.Width;
                int h = image.Height;
                picPixels = new byte[h, w];
                s.Start();//старт таймера
                List<AmountAndType> columns = Operations.Binarization(Operations.FindColorComponentOfTheImage(
                    image, w, h), w, h, image, picPixels);
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
                System.Windows.MessageBox.Show(s.ElapsedMilliseconds.ToString());
            }
        }
        
        bool gc = false;
        byte mostRescentElem = 0;
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
            List<LetterWidthAndCount> lettersInfo = new List<LetterWidthAndCount>();//Типо lW для алгоритма.
            List<PixelArrayAndParams> letters = new List<PixelArrayAndParams>(); //
            List<List<byte>> letter = new List<List<byte>>();
            List<byte> row = new List<byte>();
            for (int j = 0; j < width; j++)
            {
                bool isBlack = false;
                for (int i = l; i < r; i++)
                {
                    row.Add(picPixels[i, j]);
                    if (picPixels[i, j] == 1)
                        isBlack = true;
                }
                if (isBlack)
                    letter.Add(new List<byte>(row));
                else
                if (letter.Count != 0)
                {
                    letters.Add(new PixelArrayAndParams(Operations.Transpose(letter, r - l, letter.Count), row.Count, letter.Count));
                    bool isMatched = false;
                    for (int i = 0; i < lettersInfo.Count; i++)
                        if (letter.Count == lettersInfo[i].width)
                        {
                            LetterWidthAndCount v = lettersInfo[i];
                            v.count++;
                            lettersInfo[i] = v;
                            isMatched = true;
                        }
                    if (!isMatched)
                        lettersInfo.Add(new LetterWidthAndCount((byte)letter.Count, 1));
                    letter.Clear();
                }
                row.Clear();
            }
            lettersInfo.Sort();// По найденным ширинам символов объединяю их.
            if (!gc)
            {
                mostRescentElem = new Algorithm().GetTheMostGenericElem(lettersInfo); //Нахожу самую распространенную ширину из объединенных.
                gc = true;
            }
            lettersInfo.Clear();
            new Erosion().UseErosion(ref letters, mostRescentElem);//Следующая мякотка программы...
                                                                   //Отрисовка получаемых букв - лишь для теста.
            foreach (PixelArrayAndParams ltr in letters)
            {
                //Отрисовка получаемых букв - лишь для теста.
                System.Drawing.Bitmap b = new System.Drawing.Bitmap(ltr.width, ltr.heigth);
                for (int i = 0; i < ltr.width; i++)
                {
                    for (int k = 0; k < ltr.heigth; k++)
                    {
                        switch (ltr.pixelArray[k, i])
                        {
                            case 1:
                                {
                                    b.SetPixel(i, k, System.Drawing.Color.Black);
                                    break;
                                }
                            case 0:
                                {
                                    b.SetPixel(i, k, System.Drawing.Color.White);
                                    break;
                                }
                        }
                    }
                }

                using (FileStream ms = new FileStream("C:\\Users\\gorka\\OneDrive\\Рабочий стол\\OCR\\Letters\\" +
                    ccounter + ".png", FileMode.Create))
                using (Bitmap i1 = (Bitmap)b.Clone())
                {
                    i1.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    ccounter++;
                }
            }//Удалить до сюда.
            return letters;
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
