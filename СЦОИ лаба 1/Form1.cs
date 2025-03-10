namespace СЦОИ_лаба_1
{
    public partial class Form1 : Form
    {
        private Bitmap image = null;
        public Form1()
        {
            InitializeComponent();
            image = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            pictureBox1.Image = image;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "Выберите изображения";
                openFileDialog.Filter = "Изображения|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
                openFileDialog.Multiselect = true; // Позволяет выбрать несколько файлов

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    foreach (string file in openFileDialog.FileNames)
                    {
                        PictureBox pictureBox = new PictureBox
                        {
                            Image = new Bitmap(file),
                            SizeMode = PictureBoxSizeMode.Zoom, // Уменьшает изображение пропорционально
                            Width = 150, // Ширина миниатюры
                            Height = 150, // Высота миниатюры
                            Margin = new Padding(5) // Отступы между картинками
                        };

                        flowLayoutPanel1.Controls.Add(pictureBox);
                    }
                }
            }
        }
    }
}
