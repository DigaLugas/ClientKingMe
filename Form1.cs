using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using KingMeServer;

namespace ClientKingMe
{
    public partial class Form1 : Form
    {
        private MusicPlayerControl musicPlayer;

        public Form1()
        {
            InitializeComponent();
            ApplyCustomStyling();
            atulizarComboBox();
            AddMusicPlayerControl();
        }

        private void AddMusicPlayerControl()
        {
            MusicPlayerControl musicPlayer = new MusicPlayerControl();

            musicPlayer.Location = new System.Drawing.Point(
                this.ClientSize.Width - 210, 
                this.ClientSize.Height - 550 
            );

            this.Controls.Add(musicPlayer);
        } 

        private void ApplyCustomStyling()
        {
            Color primaryColor = Color.FromArgb(57, 89, 156);      
            Color secondaryColor = Color.FromArgb(241, 245, 249); 
            Color accentColor = Color.FromArgb(34, 197, 94);      

            this.BackColor = secondaryColor;
            this.Font = new Font("Segoe UI", 9, FontStyle.Regular);
            this.Text = "KingMe - Gerenciador de Partidas";

            StyleComboBox(comboBoxPartidas, primaryColor);
            StyleTextBox(nomePartida, primaryColor);
            StyleTextBox(senhaPartida, primaryColor);
            StyleTextBox(detalhesPartida, primaryColor);
            StyleButton(criarPartida, primaryColor, accentColor);
            StyleButton(entrarPartida, primaryColor, accentColor);

            StyleLabel(label1, primaryColor, 18);
            StyleLabel(label2, primaryColor, 10);
            StyleLabel(label3, primaryColor, 10);
            StyleLabel(label4, primaryColor, 10);
            StyleLabel(label5, primaryColor, 10);
            StyleLabel(label6, primaryColor, 10);
            StyleLabel(label7, primaryColor, 18);
            StyleLabel(label8, primaryColor, 10);
            StyleLabel(label9, primaryColor, 10);
            StyleLabel(label11, primaryColor, 10);
            StyleLabel(label12, primaryColor, 18);




            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
        }

        private void StyleComboBox(ComboBox comboBox, Color primaryColor)
        {
            comboBox.BackColor = Color.White;
            comboBox.ForeColor = primaryColor;
            comboBox.Font = new Font("Segoe UI", 10);
            comboBox.FlatStyle = FlatStyle.Flat;
            comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        private void StyleTextBox(TextBox textBox, Color primaryColor)
        {
            textBox.BackColor = Color.White;
            textBox.ForeColor = primaryColor;
            textBox.Font = new Font("Segoe UI", 10);
            textBox.BorderStyle = BorderStyle.FixedSingle;
        }

        private void StyleButton(Button button, Color primaryColor, Color accentColor)
        {
            button.BackColor = accentColor;
            button.ForeColor = Color.White;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = primaryColor;
            button.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            button.Cursor = Cursors.Hand;
        }

        private void StyleLabel(Label label, Color primaryColor, float textSize)
        {
            label.ForeColor = primaryColor;
            label.Font = new Font("Segoe UI", textSize, FontStyle.Bold);
        }

        private void button1_Click(object sender, EventArgs e)
        {
      
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {

        }

        private void comboBoxPartidas_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxPartidas.SelectedIndex >= 0)
            {

                detalhesPartida.Text = "";

  
                string linha = comboBoxPartidas.SelectedItem.ToString();
                string[] detalhes = linha.Split('|');
                string id = detalhes[0].Split(':')[1].Trim();
                detalhesPartida.Text += "--- DETALHES DA PARTIDA ---" + Environment.NewLine;
                detalhesPartida.Text += linha + Environment.NewLine;
                detalhesPartida.Text += Environment.NewLine;

                label11.Text = $"Nome da partida:\n{detalhes[1].Split(':')[1].Trim()}\nId: {id}";
    
                string retorno = Jogo.ListarJogadores(Convert.ToInt32(id));
                string[] jogadores = retorno.Split('\n');

                detalhesPartida.Text += "--- JOGADORES ---" + Environment.NewLine;
                foreach (var jogador in jogadores)
                {
                    if (!string.IsNullOrWhiteSpace(jogador))
                    {
                        string[] detalhesJogador = jogador.Split(',');
                  
                        string linhaFormatada = $"ID: {detalhesJogador[0]} | Nome: {detalhesJogador[1]} | Pontuação: {detalhesJogador[2]}";
                        detalhesPartida.Text += linhaFormatada + Environment.NewLine;
                    }
                }
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
           string retorno = Jogo.CriarPartida(nomePartida.Text, senhaPartida.Text, Constants.NomeDoGrupo);
            if (retorno.Contains("ERRO"))
            {
                MessageBox.Show(retorno);
                return;
            }
            atulizarComboBox();
        }
        private void atulizarComboBox()
        {
            comboBoxPartidas.Items.Clear();

            string retorno = Jogo.ListarPartidas("T");

            string[] partidas = retorno.Split('\n');

            foreach (string partida in partidas)
            {
                if (!string.IsNullOrWhiteSpace(partida))
                {
                    string[] detalhes = partida.Split(',');

                    string linhaFormatada = $"ID: {detalhes[0]} | Nome: {detalhes[1]} | Data: {detalhes[2]} | Status: {detalhes[3]}";

                    comboBoxPartidas.Items.Add(linhaFormatada);
                }
            }
            if (comboBoxPartidas.Items.Count > 0)
            {
                comboBoxPartidas.SelectedIndex = 0;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int id = Convert.ToInt32(label11.Text.Split(':')[2].Trim());
            string retorno = Jogo.Entrar(id, nomeJogador.Text, senhaPartidaEntrar.Text);
            if (retorno.Contains("ERRO"))
            {
                MessageBox.Show(retorno);
                return;
            }

            //passando dados para o form2
            Dictionary<string, string>  valoresJogo = new Dictionary<string, string>();
            valoresJogo.Add("idPartida", Convert.ToString(id));
            valoresJogo.Add("nomePartida", label11.Text);               // tem que mudar isso aq
            valoresJogo.Add("idJogador", retorno.Split(',')[0]);
            valoresJogo.Add("nomeJogador", nomeJogador.Text);
            valoresJogo.Add("senhaJogador", retorno.Split(',')[1]);

            Form2 form2 = new Form2(valoresJogo);
            form2.Show();
            form2.FormClosed += (s, args) => Application.Exit();
            form2.ValoresJogo = valoresJogo;
            this.Hide();
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void label12_Click(object sender, EventArgs e)
        {

        }

        private void label11_Click(object sender, EventArgs e)
        {

        }
    }
}
