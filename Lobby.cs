using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using KingMeServer;

namespace ClientKingMe
{
    public partial class GameLobbyForm : Form
    {
        private MusicPlayerControl musicPlayer;
        private readonly DesignerConfigurator designer;

        public GameLobbyForm()
        {
            this.designer = new DesignerConfigurator();
            InitializeComponent();
            ApplyCustomStyling();
            RefreshGamesList();
            AddMusicPlayerControl();
        }

        private void AddMusicPlayerControl()
        {
            musicPlayer = new MusicPlayerControl
            {
                Location = new Point(
                    this.ClientSize.Width - 210,
                    this.ClientSize.Height - 550
                )
            };

            this.Controls.Add(musicPlayer);
        }

        private void ApplyCustomStyling()
        {
            this.BackColor = designer.secondaryColor;
            this.Font = new Font("Segoe UI", 9, FontStyle.Regular);
            this.Text = "KingMe - Gerenciador de Partidas";

            DesignerConfigurator.StyleComboBox(comboBoxPartidas, designer.primaryColor, 10);
            DesignerConfigurator.StyleTextBox(detalhesPartida, designer.primaryColor, 10);

            DesignerConfigurator.StyleButton(criarPartida, designer.primaryColor, designer.accentColor, 10);
            DesignerConfigurator.StyleButton(entrarPartida, designer.primaryColor, designer.accentColor, 10);

            Label[] labelsToStyle = { label1, label2, label3, label4, label5, label6, label8, label9, label11 };
            foreach (Label label in labelsToStyle)
            {
                DesignerConfigurator.StyleLabel(label, designer.primaryColor, 10);
            }

            Label[] headerLabels = { label1, label7, label12 };
            foreach (Label header in headerLabels)
            {
                DesignerConfigurator.StyleLabel(header, designer.primaryColor, 18);
            }

            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
        }

        private void RefreshGamesList()
        {
            // Clear inputs
            senhaPartida.Text = "";
            nomePartida.Text = "";
            comboBoxPartidas.Items.Clear();

            string response = Jogo.ListarPartidas("T");
            string[] games = response.Split('\n');
            
            foreach (string game in games)
            {
                if (!string.IsNullOrWhiteSpace(game))
                {
                    string[] details = game.Split(',');
                    string formattedLine = $"ID: {details[0]} | Nome: {details[1]} | Data: {details[2]} | Status: {details[3]}";
                    comboBoxPartidas.Items.Add(formattedLine);
                }
            }

            if (comboBoxPartidas.Items.Count > 0)
            {
                comboBoxPartidas.SelectedIndex = 0;
            }
        }

        private void comboBoxPartidas_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxPartidas.SelectedIndex < 0)
                return;

            UpdateGameDetails();
        }

        private void UpdateGameDetails()
        {
            detalhesPartida.Text = "";

            string selectedGame = comboBoxPartidas.SelectedItem.ToString();
            string[] details = selectedGame.Split('|');
            string gameId = details[0].Split(':')[1].Trim();

            detalhesPartida.Text += "--- DETALHES DA PARTIDA ---" + Environment.NewLine;
            detalhesPartida.Text += selectedGame + Environment.NewLine;
            detalhesPartida.Text += Environment.NewLine;

            label11.Text = $"Nome da partida:\n{details[1].Split(':')[1].Trim()}\nId: {gameId}";

            string playersResponse = Jogo.ListarJogadores(Convert.ToInt32(gameId));
            string[] players = playersResponse.Split('\n');

            detalhesPartida.Text += "--- JOGADORES ---" + Environment.NewLine;
            foreach (var player in players)
            {
                if (!string.IsNullOrWhiteSpace(player))
                {
                    string[] playerDetails = player.Split(',');
                    string formattedLine = $"ID: {playerDetails[0]} | Nome: {playerDetails[1]} | Pontuação: {playerDetails[2]}";
                    detalhesPartida.Text += formattedLine + Environment.NewLine;
                }
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            string response = Jogo.CriarPartida(
                nomePartida.Text,
                senhaPartida.Text,
                ApplicationConstants.GroupName
            );

            if (ErrorHandler.HandleServerResponse(response))
                return;

            RefreshGamesList();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int gameId = Convert.ToInt32(label11.Text.Split(':')[2]);
            string response = Jogo.Entrar(gameId, nomeJogador.Text, senhaPartidaEntrar.Text);

            if (ErrorHandler.HandleServerResponse(response))
                return;

            Dictionary<string, string> gameSessionData = new Dictionary<string, string>
            {
                ["idPartida"] = Convert.ToString(gameId),
                ["nomePartida"] = label11.Text.Split(':')[1].Trim().Split('\n')[0],
                ["idJogador"] = response.Split(',')[0],
                ["nomeJogador"] = nomeJogador.Text,
                ["senhaJogador"] = response.Split(',')[1]
            };

            GameSessionForm gameSessionForm = new GameSessionForm(gameSessionData);
            gameSessionForm.Show();
            gameSessionForm.FormClosed += (s, args) => Application.Exit();
            gameSessionForm.GameSessionData = gameSessionData;
        }

        private void button1_Click(object sender, EventArgs e) { }
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e) { }
        private void label1_Click(object sender, EventArgs e) { }
        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e) { }
        private void label2_Click(object sender, EventArgs e) { }
        private void label3_Click(object sender, EventArgs e) { }
        private void label5_Click(object sender, EventArgs e) { }
        private void label6_Click(object sender, EventArgs e) { }
        private void Form1_Load(object sender, EventArgs e) { }
        private void label12_Click(object sender, EventArgs e) { }
        private void label11_Click(object sender, EventArgs e) { }
    }
}