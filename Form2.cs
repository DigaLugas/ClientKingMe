using KingMeServer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientKingMe
{
    public partial class Form2: Form
    {
        public Dictionary<string, string> ValoresJogo { get; set; }

        public Form2(Dictionary<string, string> valoresJogo)
        {
            InitializeComponent();
            this.ValoresJogo = valoresJogo;
            label3.Text = ValoresJogo["nomePartida"];
            label4.Text = $"Id: {ValoresJogo["idJogador"]}";
            label5.Text = $"Nome: {ValoresJogo["nomeJogador"]}";
            label6.Text = $"Senha: {ValoresJogo["senhaJogador"]}";
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

            textBox1.Text = retorno;
        }
    }
}
