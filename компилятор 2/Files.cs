using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace компилятор_2
{
    internal class Files
    {
        public Dictionary<RichTextBox, string> FilePaths { get; private set; }
        private Dictionary<RichTextBox, Stack<string>> undoHistory = new();
        private Dictionary<RichTextBox, Stack<string>> redoHistory = new();
        private HashSet<RichTextBox> trackedEditors = new();
        private SyntaxHighlighter syntaxHighlighter = new SyntaxHighlighter();
        private Dictionary<RichTextBox, bool> hasUnsavedChanges = new();


        public Files()
        {
            FilePaths = new Dictionary<RichTextBox, string>();
        }

        public void OpenFile(string filePath, TabControl tabControl)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    MessageBox.Show("Путь к файлу пуст!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!File.Exists(filePath))
                {
                    MessageBox.Show("Файл не найден!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                RichTextBox rtb = AddNewTab(tabControl, Path.GetFileName(filePath));
                if (rtb == null)
                {
                    MessageBox.Show("Не удалось создать вкладку!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                LoadFileContent(rtb, filePath);
                FilePaths[rtb] = filePath;

                InitializeUndoRedo(rtb);
                EnableTracking(rtb);

                syntaxHighlighter?.AttachToRichTextBox(rtb);
                syntaxHighlighter?.HighlightSyntax(rtb);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии файла: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadFileContent(RichTextBox rtb, string filePath)
        {
            if (Path.GetExtension(filePath).ToLower() == ".rtf")
                rtb.LoadFile(filePath, RichTextBoxStreamType.RichText);
            else
                rtb.Text = File.ReadAllText(filePath, Encoding.UTF8);
        }

        public void SaveFile(TabControl tabControl)
        {
            RichTextBox rtb = GetActiveRichTextBox(tabControl);
            if (rtb == null) return;

            if (FilePaths.TryGetValue(rtb, out string filePath) && File.Exists(filePath))
            {
                SaveToFile(rtb, filePath);
            }
            else
            {
                SaveFileAs(tabControl);
            }
        }

        public void SaveFileAs(TabControl tabControl)
        {
            RichTextBox rtb = GetActiveRichTextBox(tabControl);
            if (rtb == null) return;

            using SaveFileDialog saveFileDialog = new()
            {
                Filter = "Текстовый файл (*.txt)|*.txt|RTF файл (*.rtf)|*.rtf",
                Title = "Сохранить файл как"
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = saveFileDialog.FileName;
                SaveToFile(rtb, filePath);
                FilePaths[rtb] = filePath;
                tabControl.SelectedTab.Text = Path.GetFileName(filePath);
            }
        }

        private void SaveToFile(RichTextBox rtb, string filePath)
        {
            try
            {
                if (Path.GetExtension(filePath).ToLower() == ".rtf")
                    rtb.SaveFile(filePath, RichTextBoxStreamType.RichText);
                else
                    File.WriteAllText(filePath, rtb.Text, Encoding.UTF8);

                rtb.Modified = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении файла: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public RichTextBox AddNewTab(TabControl tabControl, string title = "Новый документ")
        {
            TabPage newTab = new(title);
            RichTextBox rtb = new()
            {
                Dock = DockStyle.Fill,
                Font = new System.Drawing.Font("Arial", 12),
                AcceptsTab = true,
                BorderStyle = BorderStyle.None
            };
            PictureBox lineNumbers = new()
            {
                Width = 40,
                Dock = DockStyle.Left,
                BackColor = Color.LightGray
            };


            newTab.Controls.Add(rtb);
            newTab.Controls.Add(lineNumbers);
            tabControl.TabPages.Add(newTab);
            tabControl.SelectedTab = newTab;

            rtb.TextChanged += (s, e) => lineNumbers.Invalidate();
            rtb.VScroll += (s, e) => lineNumbers.Invalidate();
            rtb.Resize += (s, e) => lineNumbers.Invalidate();
            lineNumbers.Paint += (s, e) => DrawLineNumbers(e.Graphics, rtb, lineNumbers);


            InitializeUndoRedo(rtb);
            EnableTracking(rtb);

            return rtb;
        }

        private void DrawLineNumbers(Graphics g, RichTextBox richTextBox, PictureBox lineNumberBox)
        {
            g.Clear(lineNumberBox.BackColor);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            int firstIndex = richTextBox.GetCharIndexFromPosition(new Point(0, 0));
            int firstLine = richTextBox.GetLineFromCharIndex(firstIndex);
            int totalLines = richTextBox.Lines.Length;

            int lineHeight = TextRenderer.MeasureText("0", richTextBox.Font).Height;
            int y = 0;

            string maxLineNumber = totalLines.ToString();
            int maxWidth = TextRenderer.MeasureText(maxLineNumber, richTextBox.Font).Width + 10;

            if (lineNumberBox.Width != maxWidth)
            {
                lineNumberBox.Width = maxWidth;
            }

            for (int i = firstLine; y < richTextBox.Height; i++)
            {
                y = (i - firstLine) * lineHeight;
                g.DrawString((i + 1).ToString(), richTextBox.Font, Brushes.Black, 5, y);
            }
        }

        private void InitializeUndoRedo(RichTextBox rtb)
        {
            undoHistory[rtb] = new Stack<string>();
            redoHistory[rtb] = new Stack<string>();
            undoHistory[rtb].Push(rtb.Text);
        }

        private void EnableTracking(RichTextBox rtb)
        {
            if (trackedEditors.Contains(rtb)) return;
            trackedEditors.Add(rtb);
            rtb.TextChanged += TrackChanges;
            hasUnsavedChanges[rtb] = false; 
            rtb.TextChanged += (sender, e) => hasUnsavedChanges[rtb] = true;
        }

        public void TrackChanges(object sender, EventArgs e)
        {
            if (sender is not RichTextBox rtb) return;

            if (undoHistory[rtb].Count == 0 || undoHistory[rtb].Peek() != rtb.Text)
            {
                undoHistory[rtb].Push(rtb.Text);
                redoHistory[rtb].Clear();
            }
        }

        public RichTextBox GetActiveRichTextBox(TabControl tabControl)
        {
            return tabControl.SelectedTab?.Controls[0] as RichTextBox;
        }

        public void UndoLastChange(RichTextBox rtb)
        {
            if (undoHistory.TryGetValue(rtb, out var undoStack) && undoStack.Count > 1)
            {
                if (!redoHistory.ContainsKey(rtb)) redoHistory[rtb] = new Stack<string>();

                rtb.TextChanged -= TrackChanges;
                redoHistory[rtb].Push(undoStack.Pop());
                rtb.Text = undoStack.Peek();
                rtb.TextChanged += TrackChanges;
            }
        }

        public void RedoLastChange(RichTextBox rtb)
        {
            if (redoHistory.TryGetValue(rtb, out var redoStack) && redoStack.Count > 0)
            {
                if (!undoHistory.ContainsKey(rtb)) undoHistory[rtb] = new Stack<string>();

                rtb.TextChanged -= TrackChanges;
                string nextState = redoStack.Pop();
                undoHistory[rtb].Push(nextState);
                rtb.Text = nextState;
                rtb.TextChanged += TrackChanges;
            }
        }

        public void DeleteSelectedText(RichTextBox rtb)
        {
            if (rtb == null || string.IsNullOrEmpty(rtb.SelectedText)) return;

            TrackChanges(rtb, EventArgs.Empty); 

            rtb.TextChanged -= TrackChanges;
            int selectionStart = rtb.SelectionStart;
            rtb.Text = rtb.Text.Remove(selectionStart, rtb.SelectionLength);
            rtb.SelectionStart = selectionStart;
            rtb.TextChanged += TrackChanges;
        }


        public void SelectAllText(RichTextBox rtb)
        {
            rtb?.SelectAll();
        }
    }
}
