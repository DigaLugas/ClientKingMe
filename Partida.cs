using KingMeServer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientKingMe
{
    public partial class Partida : Form
    {
        IDictionary<char, string> professores = new Dictionary<char, string>()
        {
            {'A', "Adilson Konrad"},
            {'B', "Beatriz Paiva"},
            {'C', "Claro"},
            {'D', "Douglas Baquiao"},
            {'E', "Eduardo Takeo"},
            {'G', "Guilherme Rey"},
            {'H', "Heredia"},
            {'J', "Joker"},  // Added Joker character since it's in the verificarVez response
            {'K', "Karin"},
            {'L', "Leonardo Takuno"},
            {'M', "Mario Toledo"},
            {'P', "Pedreiro"}, // Added Pedreiro character since it's in the verificarVez response (P)
            {'Q', "Quintas"},
            {'R', "Ranulfo"},
            {'T', "Toshio"},
        };

        public Dictionary<string, string> ValoresJogo { get; set; }
        private DesignerConfigurator designer;
        private Tabuleiro tabuleiro; // Add Tabuleiro reference

        public Partida(Dictionary<string, string> valoresJogo)
        {
            this.designer = new DesignerConfigurator();
            InitializeComponent();
            ApplyCustomStyling();
            label7.Text = "";
            this.ValoresJogo = valoresJogo;
            label8.Text = "";
            label3.Text = ValoresJogo["nomePartida"];
            label4.Text = $"Id: {ValoresJogo["idJogador"]}";
            label5.Text = $"Nome: {ValoresJogo["nomeJogador"]}";
            label6.Text = $"Senha: {ValoresJogo["senhaJogador"]}";

            // Initialize the Tabuleiro with the pictureBox1 (assuming pictureBox1 is the game board control)
            tabuleiro = new Tabuleiro(pictureBox1);
        }

        private void ApplyCustomStyling()
        {
            DesignerConfigurator.StyleLabel(label1, designer.primaryColor, 13);
            DesignerConfigurator.StyleLabel(label2, designer.primaryColor, 13);
            DesignerConfigurator.StyleLabel(label3, designer.primaryColor, 10);
            DesignerConfigurator.StyleLabel(label4, designer.primaryColor, 10);
            DesignerConfigurator.StyleLabel(label5, designer.primaryColor, 10);
            DesignerConfigurator.StyleLabel(label6, designer.primaryColor, 10);
            DesignerConfigurator.StyleLabel(label8, designer.primaryColor, 10);
            DesignerConfigurator.StyleButton(button1, designer.primaryColor, designer.accentColor, 10);
            DesignerConfigurator.StyleButton(button2, designer.primaryColor, designer.accentColor, 10);
        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string retorno = Jogo.Iniciar(Convert.ToInt32(ValoresJogo["idJogador"]), ValoresJogo["senhaJogador"]);
            if (retorno.Contains("ERRO"))
            {
                MessageBox.Show(retorno);
                return;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string retorno = Jogo.ListarCartas(Convert.ToInt32(ValoresJogo["idJogador"]), ValoresJogo["senhaJogador"]);
            if (retorno.Contains("ERRO"))
            {
                MessageBox.Show(retorno);
                return;
            }

            foreach (char c in retorno.ToCharArray())
            {
                label7.Text += professores.ContainsKey(c) ? professores[c] + "\n" : "";
            }
        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            label8.Text = "";
            var retorno = Jogo.VerificarVez(Convert.ToInt32(ValoresJogo["idPartida"]));
            if (retorno.Contains("ERRO"))
            {
                MessageBox.Show(retorno);
                return;
            }

            // Process the game board state and show character positions
            tabuleiro.ProcessarRetornoTabuleiro(retorno);

            // Handle turn information
            var linhas = retorno.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (linhas.Length > 0)
            {
                var primeiraLinha = linhas[0].Split(',');

                if (primeiraLinha.Length >= 2 && primeiraLinha[0] == ValoresJogo["idJogador"])
                {
                    label8.Text = $"ID: {ValoresJogo["idJogador"]}, sua vez {ValoresJogo["nomeJogador"]}";
                }
                else
                {
                    var texto = Jogo.ListarJogadores(Convert.ToInt32(ValoresJogo["idPartida"]));
                    var jogadores = texto.Split('\n');
                    foreach (var jogador in jogadores)
                    {
                        if (!string.IsNullOrWhiteSpace(jogador))
                        {
                            string[] detalhesJogador = jogador.Split(',');
                            if (detalhesJogador.Length >= 2 && detalhesJogador[0] == primeiraLinha[0])
                            {
                                label8.Text = $"ID: {detalhesJogador[0]}, vez do {detalhesJogador[1]}";
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem == null || comboBox1.SelectedIndex == -1)
            {
                MessageBox.Show("Selecione um personagem e um andar");
                return;
            }

            var primeiraLetra = listBox1.SelectedItem.ToString().First();
            var retorno = Jogo.ColocarPersonagem(Convert.ToInt32(ValoresJogo["idJogador"]), ValoresJogo["senhaJogador"], comboBox1.SelectedIndex, Convert.ToString(primeiraLetra));
            if (retorno.Contains("ERRO"))
            {
                MessageBox.Show(retorno);
            }
            else
            {
                MessageBox.Show($"Movido {professores[primeiraLetra]}, para setor {comboBox1.SelectedItem}");

                // Update the board after a successful move
                button4_Click(sender, e);
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click_1(object sender, EventArgs e)
        {

        }
    }
}