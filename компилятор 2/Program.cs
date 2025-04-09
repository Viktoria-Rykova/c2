using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace компилятор_2
{
    public static class AppSettings
    {
        private static void ApplyResourcesToMenuStrip(MenuStrip menuStrip, ComponentResourceManager resources)
        {
            foreach (ToolStripItem item in menuStrip.Items)
            {
                resources.ApplyResources(item, item.Name);
                if (item is ToolStripMenuItem menuItem)
                {
                    ApplyResourcesToToolStripMenuItem(menuItem, resources);
                }
            }
        }

        private static void ApplyResourcesToToolStripMenuItem(ToolStripMenuItem menuItem, ComponentResourceManager resources)
        {
            foreach (ToolStripItem subItem in menuItem.DropDownItems)
            {
                resources.ApplyResources(subItem, subItem.Name);
                if (subItem is ToolStripMenuItem subMenuItem)
                {
                    ApplyResourcesToToolStripMenuItem(subMenuItem, resources);
                }
            }
        }

        private static void ApplyResourcesToStatusStrip(StatusStrip statusStrip, ComponentResourceManager resources)
        {
            foreach (ToolStripItem item in statusStrip.Items)
            {
                resources.ApplyResources(item, item.Name);
            }
        }

        public static void ApplyFontSizeToControls(Control.ControlCollection controls)
        {
            foreach (Control control in controls)
            {
                control.Font = new Font(control.Font.FontFamily, Properties.Settings.Default.FontSize);
                if (control.HasChildren)
                {
                    ApplyFontSizeToControls(control.Controls);
                }
            }
        }

        private static void ApplyResourcesToToolStrip(ToolStrip toolStrip, ComponentResourceManager resources)
        {
            foreach (ToolStripItem item in toolStrip.Items)
            {
                resources.ApplyResources(item, item.Name);
            }
        }

        // Метод обновления формы и всех элементов управления
        public static void UpdateFormLanguage(Form form)
        {
            ComponentResourceManager resources = new ComponentResourceManager(form.GetType());

            ApplyResourcesToControls(form, resources);

            form.Text = resources.GetString("$this.Text");

            // Обновляем локализацию для MenuStrip и ToolStrip
            foreach (Control control in form.Controls)
            {
                if (control is MenuStrip menuStrip)
                {
                    ApplyResourcesToMenuStrip(menuStrip, resources);
                }
                else if (control is ToolStrip toolStrip)
                {
                    ApplyResourcesToToolStrip(toolStrip, resources);
                }
            }

            // Обновляем DataGridView, если он есть на форме
            var dataGridView = form.Controls.OfType<DataGridView>().FirstOrDefault();
            if (dataGridView != null)
            {
                UpdateDataGridViewHeaders(dataGridView);
                dataGridView.Refresh();  // Перерисовываем DataGridView
            }

            form.AutoSize = false;
        }




        private static void ApplyResourcesToControls(Control parent, ComponentResourceManager resources)
        {
            foreach (Control control in parent.Controls)
                if (control is StatusStrip statusStrip)
                {
                    ApplyResourcesToStatusStrip(statusStrip, resources);
                }
                else
                {
                    resources.ApplyResources(control, control.Name);
                    if (control.HasChildren)
                    {
                        ApplyResourcesToControls(control, resources);
                    }
                }
        }

        // Метод обновления заголовков в DataGridView в зависимости от локализации
        public static void UpdateDataGridViewHeaders(DataGridView dataGridView)
        {
            if (Thread.CurrentThread.CurrentUICulture.Name == "ru-RU")
            {
                dataGridView.Columns["Code"].HeaderText = "Код";
                dataGridView.Columns["Type"].HeaderText = "Тип лексемы";
                dataGridView.Columns["Lexeme"].HeaderText = "Лексема";
                dataGridView.Columns["Position"].HeaderText = "Местоположение";
            }
            else // Для английского языка
            {
                dataGridView.Columns["Code"].HeaderText = "Code";
                dataGridView.Columns["Type"].HeaderText = "Lexeme Type";
                dataGridView.Columns["Lexeme"].HeaderText = "Lexeme";
                dataGridView.Columns["Position"].HeaderText = "Position";
            }
        }

        public static string ErrorMessage()
        {
            if (Thread.CurrentThread.CurrentUICulture == new CultureInfo("ru-RU"))
                return "Ошибка";
            else
                return "Error";
        }
    }

    internal static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            string appLanguage = Properties.Settings.Default.AppLanguage?.Trim() ?? "ru-RU";

            try
            {
                CultureInfo newCulture = new CultureInfo(appLanguage);
                Thread.CurrentThread.CurrentCulture = newCulture;
                Thread.CurrentThread.CurrentUICulture = newCulture;
            }
            catch (CultureNotFoundException)
            {
                CultureInfo defaultCulture = new CultureInfo("ru-RU");
                Thread.CurrentThread.CurrentCulture = defaultCulture;
                Thread.CurrentThread.CurrentUICulture = defaultCulture;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

    }
}
