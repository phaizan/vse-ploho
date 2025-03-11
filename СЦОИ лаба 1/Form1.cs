using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace СЦОИ_лаба_1
{
    public partial class Form1 : Form
    {
        private class Layer
        {
            public Bitmap Image { get; set; }
            public float Opacity { get; set; } = 1.0f;
            public string BlendMode { get; set; } = "Нет";
        }

        private List<Layer> layers = new List<Layer>();
        private Bitmap canvas;
        private Graphics g;
        private int h, w;

        public Form1()
        {
            InitializeComponent();
            h = pictureBox1.Height;
            w = pictureBox1.Width;
            canvas = new Bitmap(w, h);
            g = Graphics.FromImage(canvas);
            pictureBox1.Image = canvas;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "Выберите изображения";
                openFileDialog.Filter = "Изображения|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
                openFileDialog.Multiselect = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    foreach (string file in openFileDialog.FileNames)
                    {
                        Bitmap img = new Bitmap(file);
                        var layer = new Layer { Image = img };
                        layers.Add(layer);
                        AddImageLayer(layer);
                    }
                    ApplyLayers();
                }
            }
        }

        private void ApplyLayers()
        {
            // Очищаем холст перед применением новых слоёв
            g.Clear(Color.Transparent);

            // Перебираем слои в обратном порядке (начиная с последнего добавленного)
            for (int i = layers.Count - 1; i >= 0; i--)
            {
                var layer = layers[i];

                using (ImageAttributes attr = new ImageAttributes())
                {
                    ColorMatrix matrix = new ColorMatrix { Matrix33 = layer.Opacity }; // Применяем новую прозрачность
                    attr.SetColorMatrix(matrix);

                    Rectangle destRect = new Rectangle(0, 0, w, h);

                    // Наложение слоёв с учётом режима
                    if (layer.BlendMode != "Нет")
                    {
                        canvas = BlendBitmaps(canvas, layer.Image, layer.BlendMode, layer.Opacity);
                        g = Graphics.FromImage(canvas);
                    }
                    else
                    {
                        g.DrawImage(layer.Image, destRect, 0, 0, w, h, GraphicsUnit.Pixel, attr);
                    }
                }
            }

            pictureBox1.Image = canvas; // Обновляем картинку
            pictureBox1.Refresh(); // Перерисовываем
        }

        private Bitmap BlendBitmaps(Bitmap baseImg, Bitmap overlayImg, string mode, float opacity)
        {
            int width = Math.Min(baseImg.Width, overlayImg.Width);
            int height = Math.Min(baseImg.Height, overlayImg.Height);

            Bitmap result = new Bitmap(baseImg);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color baseColor = baseImg.GetPixel(x, y);
                    Color overlayColor = overlayImg.GetPixel(x, y);

                    int r, g, b;

                    switch (mode)
                    {
                        case "Сумма":
                            r = Math.Min(baseColor.R + overlayColor.R, 255);
                            g = Math.Min(baseColor.G + overlayColor.G, 255);
                            b = Math.Min(baseColor.B + overlayColor.B, 255);
                            break;

                        case "Разность":
                            r = Math.Abs(baseColor.R - overlayColor.R);
                            g = Math.Abs(baseColor.G - overlayColor.G);
                            b = Math.Abs(baseColor.B - overlayColor.B);
                            break;

                        case "Умножение":
                            r = (baseColor.R * overlayColor.R) / 255;
                            g = (baseColor.G * overlayColor.G) / 255;
                            b = (baseColor.B * overlayColor.B) / 255;
                            break;

                        case "Среднее":
                            r = (baseColor.R + overlayColor.R) / 2;
                            g = (baseColor.G + overlayColor.G) / 2;
                            b = (baseColor.B + overlayColor.B) / 2;
                            break;

                        default:
                            r = baseColor.R;
                            g = baseColor.G;
                            b = baseColor.B;
                            break;
                    }

                    Color blendedColor = Color.FromArgb((int)(opacity * 255), r, g, b);
                    result.SetPixel(x, y, blendedColor);
                }
            }

            return result;
        }


        private void AddImageLayer(Layer layer)
        {
            // Создание панели для слоя
            Panel layerPanel = new Panel
            {
                Width = 500,
                Height = 600, // Увеличиваем высоту панели, чтобы разместить все элементы, включая кнопки
                BorderStyle = BorderStyle.FixedSingle,
            };

            PictureBox mini = new PictureBox
            {
                Image = layer.Image,
                SizeMode = PictureBoxSizeMode.Zoom,
                Width = 300,
                Height = 300,
            };

            ComboBox blendMode = new ComboBox
            {
                Width = 300
            };
            blendMode.Items.AddRange(new string[] { "Нет", "Сумма", "Разность", "Умножение", "Среднее" });
            blendMode.SelectedIndex = 0;
            blendMode.SelectedIndexChanged += (s, ev) =>
            {
                layer.BlendMode = blendMode.SelectedItem.ToString();
                ApplyLayers();
            };

            TrackBar opacityTrack = new TrackBar
            {
                Minimum = 0,
                Maximum = 100,
                Value = (int)(layer.Opacity * 100),
                TickFrequency = 10,
                Width = 200
            };
            opacityTrack.Scroll += (s, ev) =>
            {
                layer.Opacity = opacityTrack.Value / 100f;
                ApplyLayers();
            };

            // Кнопки перемещения слоёв
            Button moveUpButton = new Button
            {
                Text = "↑",
                Width = 70,
                Height = 70
            };
            moveUpButton.Click += (s, ev) =>
            {
                MoveLayerUp(layer);
                ApplyLayers();
            };

            Button moveDownButton = new Button
            {
                Text = "↓",
                Width = 70,
                Height = 70
            };
            moveDownButton.Click += (s, ev) =>
            {
                MoveLayerDown(layer);
                ApplyLayers();
            };

            // Добавляем элементы в панель
            layerPanel.Controls.Add(mini);
            layerPanel.Controls.Add(blendMode);
            layerPanel.Controls.Add(opacityTrack);
            layerPanel.Controls.Add(moveUpButton);
            layerPanel.Controls.Add(moveDownButton);

            // Расположим элементы внутри панели
            mini.Top = 10;
            mini.Left = (layerPanel.Width - mini.Width) / 2;
            blendMode.Top = mini.Bottom + 10;
            blendMode.Left = (layerPanel.Width - blendMode.Width) / 2;
            opacityTrack.Top = blendMode.Bottom + 10;
            opacityTrack.Left = (layerPanel.Width - opacityTrack.Width) / 2;
            moveUpButton.Top = opacityTrack.Bottom + 10;
            moveUpButton.Left = (layerPanel.Width - moveUpButton.Width) / 2;
            moveDownButton.Top = moveUpButton.Bottom + 5;
            moveDownButton.Left = (layerPanel.Width - moveDownButton.Width) / 2;

            // Добавляем панель в FlowLayoutPanel
            flowLayoutPanel1.Controls.Add(layerPanel);
        }



        private void MoveLayerUp(Layer layer)
        {
            int index = layers.IndexOf(layer);
            if (index > 0) // Проверяем, что слой не первый
            {
                // Перемещаем слой вверх в списке
                layers.RemoveAt(index);
                layers.Insert(index - 1, layer);
                flowLayoutPanel1.Controls.Clear();
                foreach (var l in layers)
                {
                    AddImageLayer(l); // Перерисовываем все слои
                }
            }
        }

        // Метод для перемещения слоя вниз
        private void MoveLayerDown(Layer layer)
        {
            int index = layers.IndexOf(layer);
            if (index < layers.Count - 1) // Проверяем, что слой не последний
            {
                // Перемещаем слой вниз в списке
                layers.RemoveAt(index);
                layers.Insert(index + 1, layer);
                flowLayoutPanel1.Controls.Clear();
                foreach (var l in layers)
                {
                    AddImageLayer(l); // Перерисовываем все слои
                }
            }
        }
    }
}
