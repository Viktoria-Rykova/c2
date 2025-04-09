using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace компилятор_2
{
    internal class SyntaxHighlighter
    {
        private readonly string[] keywordsBlue = { "public", "private", "protected", "class" };
        private readonly string[] keywordsPurple = { "if", "else", "elif" };

        private readonly Regex dictionaryRegex = new Regex(@"\b\w+\s*=\s*{", RegexOptions.Compiled); // Название словаря
        private readonly Regex singleQuotesRegex = new Regex(@"'[^']*'", RegexOptions.Compiled); // Текст в одинарных кавычках
        private readonly Regex bracesRegex = new Regex(@"[{}]", RegexOptions.Compiled); // Фигурные скобки

        public void AttachToRichTextBox(RichTextBox rtb)
        {
            rtb.TextChanged += (sender, e) => HighlightSyntax((RichTextBox)sender);
        }

        public void HighlightSyntax(RichTextBox rtb)
        {
            int selectionStart = rtb.SelectionStart;
            int selectionLength = rtb.SelectionLength;

            rtb.SuspendLayout();

            rtb.SelectAll();
            rtb.SelectionColor = Color.Black;
            rtb.SelectionFont = new Font(rtb.Font, FontStyle.Regular);

            // Подсветка ключевых слов
            ApplyKeywordHighlighting(rtb, keywordsBlue, Color.FromArgb(86, 156, 214)); // Мягкий синий
            ApplyKeywordHighlighting(rtb, keywordsPurple, Color.FromArgb(197, 134, 192)); // Нежно-фиолетовый

            // Подсветка регулярных выражений
            ApplyRegexHighlighting(rtb, dictionaryRegex, Color.FromArgb(92, 158, 205)); // Голубовато-серый
            ApplyRegexHighlighting(rtb, singleQuotesRegex, Color.FromArgb(209, 154, 102)); // Теплый оранжевый
            ApplyRegexHighlighting(rtb, bracesRegex, Color.FromArgb(229, 192, 123)); // Приглушенный желтый

            rtb.SelectionStart = selectionStart;
            rtb.SelectionLength = selectionLength;
            rtb.SelectionColor = Color.Black;
            rtb.SelectionFont = new Font(rtb.Font, FontStyle.Regular);

            rtb.ResumeLayout();
        }

        private void ApplyKeywordHighlighting(RichTextBox rtb, string[] keywords, Color color)
        {
            foreach (string keyword in keywords)
            {
                Regex regex = new Regex(@"\b" + Regex.Escape(keyword) + @"\b", RegexOptions.Compiled);
                ApplyRegexHighlighting(rtb, regex, color);
            }
        }

        private void ApplyRegexHighlighting(RichTextBox rtb, Regex regex, Color color)
        {
            MatchCollection matches = regex.Matches(rtb.Text);

            foreach (Match match in matches)
            {
                int start = match.Index;
                int length = match.Length;

                rtb.Select(start, length);
                rtb.SelectionColor = color;
                rtb.SelectionFont = new Font(rtb.Font, FontStyle.Bold);
            }

            rtb.SelectionStart = rtb.TextLength;
            rtb.SelectionLength = 0;
            rtb.SelectionColor = Color.Black;
            rtb.SelectionFont = new Font(rtb.Font, FontStyle.Regular);
        }
    }
}
