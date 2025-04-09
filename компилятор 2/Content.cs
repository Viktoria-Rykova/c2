using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace компилятор_2
{
    public partial class Content : Form
    {
        private string _fileName;
        public Content()
        {
            InitializeComponent();
        }
        public Content(string fileName)
        {
            InitializeComponent();
            _fileName = fileName;
            LoadHtmlPage();
        }
        private void LoadHtmlPage()
        {
            try
            {
                AppSettings.ApplyFontSizeToControls(this.Controls);

                // Путь к папке с ресурсами
                string resourcesFolder = Path.Combine(Application.StartupPath);
                string filePath = Path.Combine(resourcesFolder, _fileName);

                if (File.Exists(filePath))
                {
                    webBrowser1.Navigate(filePath);
                }
                else
                {
                    throw new Exception($"Файл {_fileName} не найден.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

        }

        private void Content_Load(object sender, EventArgs e)
        {

        }
    }
}
