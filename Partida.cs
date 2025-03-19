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
        {'K', "Karin"},
        {'L', "Leonardo Takuno"},
        {'M', "Mario Toledo"},
        {'Q', "Quintas"},
        {'R', "Ranulfo"},
        {'T', "Toshio"},
    };

        public Dictionary<string, string> ValoresJogo { get; set; }
        private DesignerConfigurator designer;
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
            }
            var retornoMaluco = retorno.Split(',');
            if (retornoMaluco[0] == ValoresJogo["idJogador"])
            {

                label8.Text += $"ID: {ValoresJogo["idJogador"]}, sua vez {ValoresJogo["nomeJogador"]}";
                
            }
            else {
                var texto = Jogo.ListarJogadores(Convert.ToInt32(ValoresJogo["idPartida"]));
                var jogadores = texto.Split('\n');
                foreach (var jogador in jogadores)
                {
                    if (!string.IsNullOrWhiteSpace(jogador))
                    {
                        string[] detalhesJogador = jogador.Split(',');
                        if (detalhesJogador[0] == retornoMaluco[0])
                        {
                            label8.Text += $"ID: {detalhesJogador[0]}, vez do {detalhesJogador[1]}";

                        }
                    }
                }


            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var primeiraLetra = listBox1.SelectedItem.ToString().First();
            var retorno = Jogo.ColocarPersonagem(Convert.ToInt32(ValoresJogo["idJogador"]), ValoresJogo["senhaJogador"], comboBox1.SelectedIndex, Convert.ToString(primeiraLetra));
            if (retorno.Contains("ERRO"))
            {
                MessageBox.Show(retorno);
            }
            else
            {
                MessageBox.Show($"Movido {professores[primeiraLetra]}, para setor {comboBox1.SelectedItem}");
            }
        } 
        
    }
}
