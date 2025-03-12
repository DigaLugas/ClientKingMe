using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientKingMe
{
    public class DesignerConfigurator
    {
        public Color primaryColor = Color.FromArgb(57, 89, 156);
        public Color secondaryColor = Color.FromArgb(241, 245, 249);
        public Color accentColor = Color.FromArgb(34, 197, 94);      

        
        public static void StyleComboBox(ComboBox comboBox, Color primaryColor, int tam)
        {
            comboBox.BackColor = Color.White;
            comboBox.ForeColor = primaryColor;
            comboBox.Font = new Font("Segoe UI", tam);
            comboBox.FlatStyle = FlatStyle.Flat;
            comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        public static void StyleTextBox(TextBox textBox, Color primaryColor, int tam)
        {
            textBox.BackColor = Color.White;
            textBox.ForeColor = primaryColor;
            textBox.Font = new Font("Segoe UI", tam);
            textBox.BorderStyle = BorderStyle.FixedSingle;
        }

        public static void StyleButton(Button button, Color primaryColor, Color accentColor, int tam)
        {
            button.BackColor = accentColor;
            button.ForeColor = Color.White;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = primaryColor;
            button.Font = new Font("Segoe UI", tam, FontStyle.Bold);
            button.Cursor = Cursors.Hand;
        }

        public static void  StyleLabel(Label label, Color primaryColor, float textSize)
        {
            label.ForeColor = primaryColor;
            label.Font = new Font("Segoe UI", textSize, FontStyle.Bold);
        }
    }
}
