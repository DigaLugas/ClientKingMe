using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientKingMe
{
    internal class Tabuleiro
    {
        Dictionary<int, int> map = new Dictionary<int, int>() {
            {0, 0},
            {1, 0},
            {2, 0},
            {3, 0},
            {4, 0},
            {5, 0},
        };

        private Dictionary<char, Image> imagensPersonagens = new Dictionary<char, Image>();

        private PictureBox imagemTabuleiro;

        private List<PictureBox> personagensNoTabuleiro = new List<PictureBox>();

        public Tabuleiro(PictureBox imagemTabuleiro)
        {
            this.imagemTabuleiro = imagemTabuleiro;
            CarregarImagensPersonagens();
        }

        private void CarregarImagensPersonagens()
        {
            try
            {

                string basePath = "./Images";
               
                imagensPersonagens['A'] = Image.FromFile( basePath + "/A.png");
                imagensPersonagens['B'] = Image.FromFile(basePath + "/B.png");
                imagensPersonagens['C'] = Image.FromFile(basePath + "/C.png");
                imagensPersonagens['D'] = Image.FromFile(basePath + "/D.png");
                imagensPersonagens['E'] = Image.FromFile(basePath + "/E.png");
                imagensPersonagens['G'] = Image.FromFile(basePath + "/G.png");
                imagensPersonagens['H'] = Image.FromFile(basePath + "/H.png");
                imagensPersonagens['K'] = Image.FromFile(basePath + "/K.png");
                imagensPersonagens['L'] = Image.FromFile(basePath + "/L.png");
                imagensPersonagens['M'] = Image.FromFile(basePath + "/M.png");
                imagensPersonagens['Q'] = Image.FromFile(basePath + "/Q.png");
                imagensPersonagens['R'] = Image.FromFile(basePath + "/R.png");
                imagensPersonagens['T'] = Image.FromFile(basePath + "/T.png");

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar imagens: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public bool posicionarPersonagem(char letra, int andar)
        {
            if (andar < 0 || andar > 5)
                return false;

            if (!imagensPersonagens.ContainsKey(letra))
                return false;

            PictureBox picPersonagem = new PictureBox();
            picPersonagem.Image = imagensPersonagens[letra];
            picPersonagem.SizeMode = PictureBoxSizeMode.Zoom;
            picPersonagem.BackColor = Color.Transparent;

            int larguraAndar = imagemTabuleiro.Width - 100; 
            int alturaAndar = (imagemTabuleiro.Height / 6) - 20; 


            int posX = 60 + (map[andar] * (larguraAndar / 4));

            int posY = imagemTabuleiro.Height - ((andar + 1) * alturaAndar);

            picPersonagem.Size = new Size(larguraAndar / 5, alturaAndar / 2);
            picPersonagem.Location = new Point(posX, posY);

            imagemTabuleiro.Controls.Add(picPersonagem);
            picPersonagem.BringToFront();

            map[andar]++;
            personagensNoTabuleiro.Add(picPersonagem);

            return true;
        }

        public void LimparTabuleiro()
        {
            foreach (var pic in personagensNoTabuleiro)
            {
                imagemTabuleiro.Controls.Remove(pic);
                pic.Dispose();
            }

            personagensNoTabuleiro.Clear();

            for (int i = 0; i < 6; i++)
            {
                map[i] = 0;
            }
        }

        public void ProcessarRetornoTabuleiro(string retorno)
        {
            LimparTabuleiro();

            string[] linhas = retorno.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string linha in linhas)
            {
                if (string.IsNullOrWhiteSpace(linha))
                    continue;

                string[] partes = linha.Split(',');

                if (partes.Length >= 2)
                {

                    int andar;
                    char letra = partes[1][0]; 
                    if (int.TryParse(partes[0], out andar))
                    {

                        posicionarPersonagem(letra, andar);
                    }
                }
            }
        }
    }
}