using System;
using System.Drawing;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;

namespace компилятор_2
{
    internal static class Setting
    {
        // Применение размера шрифта ко всем элементам управления
        public static void ApplyFontSizeToControls(Control.ControlCollection controls)
        {
            foreach (Control control in controls)
            {
                if (control is RichTextBox || control is TextBox)
                {
                    control.Font = new Font(control.Font.FontFamily, Properties.Settings.Default.FontSize);
                }

                if (control.HasChildren)
                {
                    ApplyFontSizeToControls(control.Controls);
                }
            }
        }

        // Установка языка приложения
        public static void ApplyLanguage()
        {
            string culture = Properties.Settings.Default.AppLanguage;
            if (string.IsNullOrEmpty(culture))
                culture = "ru-RU";

            CultureInfo newCulture = new CultureInfo("ru");
            Thread.CurrentThread.CurrentCulture = newCulture;
            Thread.CurrentThread.CurrentUICulture = newCulture;
        }
    }
}
