using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PotatoShooter
{
    public partial class Start : Form
    {
        public Start()
        {
            InitializeComponent();
        }

        private void Start_Load(object sender, EventArgs e)
        {
            textBox1.Select();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Replace(" ", "").Length > 0)
            {
                string text = textBox1.Text;
                this.Close();
                Thread thread = new Thread(() =>
                {
                    Game1 game = new Game1();
                    game.playerName = text;
                    game.Run();
                });
                thread.Start();
                thread.Join();
            }
            else
            {
                MessageBox.Show("Text field is empty. You must enter your name to continue.");
            }
        }

        private void button1_MouseEnter(object sender, EventArgs e)
        {
            button1.BackColor = Color.Red;
            textBox1.ForeColor = Color.Red;
        }

        private void button1_MouseLeave(object sender, EventArgs e)
        {
            button1.BackColor = Color.FromArgb(255, 128, 128);
            textBox1.ForeColor = Color.FromArgb(255, 128, 128);
        }

        string lastChange;
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Text.Length == 20)
            {
                textBox1.Text = lastChange;
                textBox1.Select(textBox1.Text.Length, 0);
            }
            lastChange = textBox1.Text;
        }

        private void textBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button1.PerformClick();
            }
        }
    }
}
