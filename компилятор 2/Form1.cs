using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static компилятор_2.LexicalAnalyzer;

namespace компилятор_2
{
    public partial class Form1 : Form
    {
        private Files files = new Files();
        private Dictionary<RichTextBox, Stack<string>> undoHistory = new Dictionary<RichTextBox, Stack<string>>();
        private SyntaxHighlighter syntaxHighlighter = new SyntaxHighlighter();
        private System.Windows.Forms.Timer timer;
        private LexicalAnalyzer analyzer = new LexicalAnalyzer();


        public Form1()
        {
            InitializeComponent();
            ДобавитьFontSizeComboBox();
            Setting.ApplyFontSizeToControls(this.Controls);


            this.AllowDrop = true;
            this.DragEnter += Form1_DragEnter;
            this.DragDrop += Form1_DragDrop;

            timer = new System.Windows.Forms.Timer();
            timer.Interval = 1000; 
            timer.Tick += Timer_Tick;
            timer.Start();


        }

        private void InitializeDataGridView()
        {
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.O) // Ctrl + O (Открыть файл)
            {
                toolStripButton2.PerformClick();
            }
            else if (e.Control && e.KeyCode == Keys.S) // Ctrl + S (Сохранить файл)
            {
                toolStripButton3.PerformClick();
            }
            else if (e.Control && e.KeyCode == Keys.Z) // Ctrl + Z (Отмена действия)
            {
                RichTextBox rtb = files.GetActiveRichTextBox(tabControl1);
                if (rtb != null) files.UndoLastChange(rtb);
            }
            else if (e.Control && e.KeyCode == Keys.Y) // Ctrl + Y (Повтор действия)
            {
                RichTextBox rtb = files.GetActiveRichTextBox(tabControl1);
                if (rtb != null) files.RedoLastChange(rtb);
            }
            else if (e.Control && e.KeyCode == Keys.A) // Ctrl + A (Выделить всё)
            {
                RichTextBox rtb = files.GetActiveRichTextBox(tabControl1);
                if (rtb != null) rtb.SelectAll();
            }
            else if (e.Control && e.KeyCode == Keys.X) // Ctrl + X (Вырезать)
            {
                RichTextBox rtb = files.GetActiveRichTextBox(tabControl1);
                if (rtb != null) rtb.Cut();
            }
            else if (e.Control && e.KeyCode == Keys.C) // Ctrl + C (Копировать)
            {
                RichTextBox rtb = files.GetActiveRichTextBox(tabControl1);
                if (rtb != null) rtb.Copy();
            }
            else if (e.Control && e.KeyCode == Keys.V) // Ctrl + V (Вставить)
            {
                RichTextBox rtb = files.GetActiveRichTextBox(tabControl1);
                if (rtb != null) rtb.Paste();
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            toolStripStatusLabel2.Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] filePaths = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string filePath in filePaths)
                {
                    files.OpenFile(filePath, tabControl1);
                }
            }
        }
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            RichTextBox rtb = files.AddNewTab(tabControl1);
            files.TrackChanges(rtb, EventArgs.Empty); 
            syntaxHighlighter.AttachToRichTextBox(rtb);
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Текстовые файлы (*.txt;*.rtf)|*.txt;*.rtf|Все файлы (*.*)|*.*";
                openFileDialog.Title = "Открыть файл";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = openFileDialog.FileName;
                    files.OpenFile(filePath, tabControl1);
                }
            }
        }

        private void ДобавитьFontSizeComboBox()
        {
            ToolStripComboBox fontSizeComboBox = new ToolStripComboBox
            {
                Name = "fontSizeComboBox",
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 75
            };

            fontSizeComboBox.Items.AddRange(new object[] { "10", "12", "14", "16", "18", "20", "24", "28", "32" });
            fontSizeComboBox.SelectedItem = Properties.Settings.Default.FontSize.ToString();

            fontSizeComboBox.SelectedIndexChanged += (sender, e) =>
            {
                float newSize = float.Parse(fontSizeComboBox.SelectedItem.ToString());
                Properties.Settings.Default.FontSize = newSize;
                Properties.Settings.Default.Save();
                Setting.ApplyFontSizeToControls(this.Controls);
            };

            видToolStripMenuItem.DropDownItems.Add(fontSizeComboBox);
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            files.SaveFile(tabControl1);
        }
        public RichTextBox GetActiveRichTextBox()
        {
            if (tabControl1.SelectedTab != null && tabControl1.SelectedTab.Controls.Count > 0)
            {
                return tabControl1.SelectedTab.Controls[0] as RichTextBox;
            }
            return null;
        }
        private void UndoLastChange(RichTextBox rtb)
        {
            if (undoHistory.ContainsKey(rtb) && undoHistory[rtb].Count > 1)
            {
                undoHistory[rtb].Pop();
                rtb.Text = undoHistory[rtb].Peek();
            }
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            RichTextBox rtb = files.GetActiveRichTextBox(tabControl1);
            if (rtb != null)
            {
                files.UndoLastChange(rtb);
            }
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            RichTextBox rtb = files.GetActiveRichTextBox(tabControl1);
            if (rtb != null)
            {
                files.RedoLastChange(rtb); 
            }
        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            RichTextBox rtb = GetActiveRichTextBox();
            if (rtb != null && rtb.SelectedText.Length > 0)
            {
                Clipboard.SetText(rtb.SelectedText);
            }
        }

        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            RichTextBox rtb = GetActiveRichTextBox();
            if (rtb != null && rtb.SelectedText.Length > 0)
            {
                rtb.Cut();
            }
        }

        private void toolStripButton8_Click(object sender, EventArgs e)
        {
            RichTextBox rtb = GetActiveRichTextBox();
            if (rtb != null && Clipboard.ContainsText())
            {
                rtb.SelectedText = Clipboard.GetText();
            }
        }

        private void toolStripButton10_Click(object sender, EventArgs e)
        {
            Content content = new Content();
            content.Show();
        }

        private void toolStripButton11_Click(object sender, EventArgs e)
        {
            About about = new About();
            about.Show();
        }

        private void создатьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RichTextBox rtb = files.AddNewTab(tabControl1); 
            files.TrackChanges(rtb, EventArgs.Empty); 
            syntaxHighlighter.AttachToRichTextBox(rtb);
        }

        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Текстовые файлы (*.txt;*.rtf)|*.txt;*.rtf|Все файлы (*.*)|*.*";
                openFileDialog.Title = "Открыть файл";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = openFileDialog.FileName;
                    files.OpenFile(filePath, tabControl1);
                }
            }
        }

        private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            files.SaveFile(tabControl1);
        }

        private void сохранитьКакToolStripMenuItem_Click(object sender, EventArgs e)
        {
            files.SaveFileAs(tabControl1);
        }


        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void отменитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RichTextBox rtb = files.GetActiveRichTextBox(tabControl1);
            if (rtb != null)
            {
                files.UndoLastChange(rtb);
            }
        }

        private void повторитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RichTextBox rtb = files.GetActiveRichTextBox(tabControl1);
            if (rtb != null)
            {
                files.RedoLastChange(rtb); 
            }
        }

        private void вырезатьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RichTextBox rtb = GetActiveRichTextBox();
            if (rtb != null && rtb.SelectedText.Length > 0)
            {
                rtb.Cut();
            }
        }

        private void копироватьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RichTextBox rtb = GetActiveRichTextBox();
            if (rtb != null && rtb.SelectedText.Length > 0)
            {
                Clipboard.SetText(rtb.SelectedText);
            }
        }

        private void вставитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RichTextBox rtb = GetActiveRichTextBox();
            if (rtb != null && Clipboard.ContainsText())
            {
                rtb.SelectedText = Clipboard.GetText();
            }
        }

        private void удалитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RichTextBox rtb = files.GetActiveRichTextBox(tabControl1);
            if (rtb != null)
            {
                files.DeleteSelectedText(rtb);
            }
        }

        private void выделитьВсеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RichTextBox rtb = files.GetActiveRichTextBox(tabControl1);
            if (rtb != null)
            {
                files.SelectAllText(rtb);
            }
        }

        private void вызовСправкиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string language = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;
            string fileName = language == "ru" ? "справка на русском.html" : "справка на английском.html";

            // Открытие формы Content и передача имени файла для загрузки
            Content contentForm = new Content(fileName);
            contentForm.Show();
        }

        private void оПрограммеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            About about = new About();
            about.Show();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Setting.ApplyFontSizeToControls(this.Controls);
            AppSettings.UpdateFormLanguage(this);
            this.KeyPreview = true; 
            this.KeyDown += Form1_KeyDown;
            InitializeDataGridView();
        }

        private void локализацияToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }


        private void английскийЯзыкToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string newCulture = Thread.CurrentThread.CurrentUICulture.Name == "ru-RU" ? "en-US" : "ru-RU";

            Properties.Settings.Default.AppLanguage = newCulture;
            Properties.Settings.Default.Save();

            Thread.CurrentThread.CurrentCulture = new CultureInfo(newCulture);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(newCulture);

            foreach (Form form in Application.OpenForms)
            {
                AppSettings.UpdateFormLanguage(form);
            }
        }


        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
    if (!CheckUnsavedChanges())
            {
                return; 
            }

            DialogResult result = MessageBox.Show(
                "Сохранить изменения перед выходом?",
                "Выход",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                files.SaveFile(tabControl1); 
            }
            else if (result == DialogResult.Cancel)
            {
                e.Cancel = true; 
            }
        }
        private bool CheckUnsavedChanges()
        {
            foreach (TabPage tab in tabControl1.TabPages)
            {
                if (tab.Controls[0] is RichTextBox rtb && rtb.Modified)
                {
                    return true;
                }
            }
            return false;
        }

        private void toolStripButton9_Click(object sender, EventArgs e)
        {  // Получаем активную вкладку
            if (tabControl1.SelectedTab?.Controls.OfType<RichTextBox>().FirstOrDefault() is RichTextBox activeRtb)
            {
                try
                {
                    // Очищаем предыдущие подсветки
                    activeRtb.SelectAll();
                    activeRtb.SelectionBackColor = Color.White;

                    // Анализируем текст
                    LexicalAnalyzer analyzer = new LexicalAnalyzer();
                    var results = analyzer.AnalyzeText(activeRtb.Text);

                    // Подсвечиваем недопустимые символы
                    foreach (var result in results)
                    {
                        if (result.Code == -1)  // Ошибка - недопустимый символ
                        {
                            // Подсвечиваем ошибочный символ
                            activeRtb.Select(result.StartPos - 1, result.EndPos - result.StartPos + 1);
                            activeRtb.SelectionBackColor = result.HighlightColor;
                        }
                    }

                    // Выводим информацию в DataGridView
                    dataGridView1.Rows.Clear();
                    if (results.Any())
                    {
                        foreach (var result in results)
                        {
                            // Добавляем строки в DataGridView
                            dataGridView1.Rows.Add(result.Code, result.Type, result.Lexeme, $"{result.StartPos}-{result.EndPos}");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Анализ не дал результатов.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Произошла ошибка при анализе текста: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Нет активного текстового редактора!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void постановкаЗадачиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Content contentForm = new Content("Постановка задачи.html");
            contentForm.Show();
        }

        private void грамматикаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Content contentForm = new Content("Грамматика.html");
            contentForm.Show();
        }

        private void классификацияГрамматикиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Content contentForm = new Content("Классификация грамматики.html");
            contentForm.Show();
        }

        private void методАнализаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Content contentForm = new Content("Метод анализа.html");
            contentForm.Show();
        }

        private void тестовыйПримерToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Content contentForm = new Content("Тестовый пример.html");
            contentForm.Show();
        }

        private void списокЛитературыToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Content contentForm = new Content("Список литературы.html");
            contentForm.Show();
        }

        private void исходныйКодПрограммыToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Content contentForm = new Content("Исходный код программы.html");
            contentForm.Show();
        }

        private void файлToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }
}
