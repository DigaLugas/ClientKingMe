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
    public partial class Partida: Form
    {
        public Dictionary<string, string> ValoresJogo { get; set; }
        private DesignerConfigurator designer;
        public Partida(Dictionary<string, string> valoresJogo)
        {
            this.designer = new DesignerConfigurator();
            InitializeComponent();
            ApplyCustomStyling();
            this.ValoresJogo = valoresJogo;
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
            MessageBox.Show("Partida Criada com Sucesso!");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string retorno = Jogo.ListarCartas(Convert.ToInt32(ValoresJogo["idJogador"]), ValoresJogo["senhaJogador"]);
            if (retorno.Contains("ERRO"))
            {
                MessageBox.Show(retorno);
                return;
            }

            textBox1.Text = retorno;
            
        }
    }
}
