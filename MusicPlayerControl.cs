using System;
using System.IO;
using System.Windows.Forms;
using WMPLib;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Drawing;

namespace ClientKingMe
{
    public partial class MusicPlayerControl : UserControl
    {
        private WindowsMediaPlayer player;
        private bool somDesativado = false;
        private string[] arquivosMusica;
        private Timer timerAtualizacao;
        private const string PASTA_MUSICAS = "./Musicas";
        private bool modoDeFila = true; 

        public event EventHandler MusicaIniciada;
        public event EventHandler MusicaParada;

        public MusicPlayerControl()
        {
            InicializarComponentes();
            ConfigurarPlayer();
            ConfigurarTimer();
        }

        private void InicializarComponentes()
        {
            this.btnTocarPausar = new Button();
            this.btnSom = new Button();
            this.btnProxima = new Button();
            this.btnAnterior = new Button();
            this.btnEscolherMusica = new Button();
            this.btnAleatorio = new Button(); 
            this.lblMusicaAtual = new Label();
            this.barraVolume = new TrackBar();
            this.barraProgresso = new ProgressBar();
            this.painelBotoes = new Panel();

            this.painelBotoes.Height = 40;
            this.painelBotoes.Dock = DockStyle.Bottom;

            this.btnTocarPausar.Text = "▶";
            this.btnSom.Text = "🔊";
            this.btnProxima.Text = "⏭";
            this.btnAnterior.Text = "⏮";
            this.btnEscolherMusica.Text = "📁";
            this.btnAleatorio.Text = "🔀"; 

            this.lblMusicaAtual.Text = "Nenhuma música";
            this.lblMusicaAtual.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblMusicaAtual.AutoEllipsis = true;

            this.barraVolume.Minimum = 0;
            this.barraVolume.Maximum = 100;
            this.barraVolume.Value = 50;
            this.barraVolume.TickFrequency = 10;

            this.barraProgresso.Minimum = 0;
            this.barraProgresso.Maximum = 100;
            this.barraProgresso.Height = 10;

            this.btnTocarPausar.Click += BtnTocarPausar_Click;
            this.btnSom.Click += BtnSom_Click;
            this.btnProxima.Click += BtnProxima_Click;
            this.btnAnterior.Click += BtnAnterior_Click;
            this.btnEscolherMusica.Click += BtnEscolherMusica_Click;
            this.btnAleatorio.Click += BtnAleatorio_Click;
            this.barraVolume.Scroll += BarraVolume_Scroll;
            this.barraProgresso.MouseClick += BarraProgresso_MouseClick;

            this.painelBotoes.Controls.Add(btnAnterior);
            this.painelBotoes.Controls.Add(btnTocarPausar);
            this.painelBotoes.Controls.Add(btnProxima);
            this.painelBotoes.Controls.Add(btnSom);
            this.painelBotoes.Controls.Add(btnAleatorio);
            this.painelBotoes.Controls.Add(btnEscolherMusica);

            int larguraBotao = 40;
            int espacamento = 5;
            int totalBotoes = painelBotoes.Controls.Count;
            int larguraTotal = (larguraBotao + espacamento) * totalBotoes - espacamento;

            this.Width = 250;

            for (int i = 0; i < totalBotoes; i++)
            {
                painelBotoes.Controls[i].Location = new System.Drawing.Point(
                    i * (larguraBotao + espacamento), 0);
                painelBotoes.Controls[i].Size = new System.Drawing.Size(larguraBotao, 40);
            }

            this.Controls.Add(painelBotoes);
            this.Controls.Add(lblMusicaAtual);
            this.Controls.Add(barraProgresso);
            this.Controls.Add(barraVolume);

            this.lblMusicaAtual.Dock = DockStyle.Top;
            this.barraProgresso.Dock = DockStyle.Bottom;
            this.barraVolume.Dock = DockStyle.Bottom;

            this.Size = new System.Drawing.Size(250, 150);

            this.BackColor = System.Drawing.Color.FromArgb(241, 245, 249);
            this.lblMusicaAtual.ForeColor = System.Drawing.Color.FromArgb(57, 89, 156);
            this.lblMusicaAtual.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);

            foreach (Control ctrl in painelBotoes.Controls)
            {
                if (ctrl is Button btn)
                {
                    btn.BackColor = System.Drawing.Color.FromArgb(34, 197, 94);
                    btn.ForeColor = System.Drawing.Color.White;
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderSize = 0;
                    btn.Cursor = Cursors.Hand;
                }
            }
        }

        private void ConfigurarTimer()
        {
            timerAtualizacao = new Timer();
            timerAtualizacao.Interval = 500;
            timerAtualizacao.Tick += TimerAtualizacao_Tick;
            timerAtualizacao.Start();
        }

        private void TimerAtualizacao_Tick(object sender, EventArgs e)
        {
            AtualizarBarraProgresso();
        }

        private void AtualizarBarraProgresso()
        {
            if (player?.currentMedia != null && player.playState == WMPPlayState.wmppsPlaying)
            {
                try
                {
                    double posicaoAtual = player.controls.currentPosition;
                    double duracaoTotal = player.currentMedia.duration;

                    if (duracaoTotal > 0)
                    {
                        int porcentagem = (int)((posicaoAtual / duracaoTotal) * 100);
                        barraProgresso.Value = Math.Min(porcentagem, 100);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Erro ao atualizar progresso: {ex.Message}");
                }
            }
        }

        private void BarraProgresso_MouseClick(object sender, MouseEventArgs e)
        {
            if (player?.currentMedia != null)
            {
                double percentual = (double)e.X / barraProgresso.Width;
                double novaPosicao = percentual * player.currentMedia.duration;
                player.controls.currentPosition = novaPosicao;
            }
        }

        private void ConfigurarPlayer()
        {
            try
            {
                player = new WindowsMediaPlayer();

                if (!Directory.Exists(PASTA_MUSICAS))
                {
                    Directory.CreateDirectory(PASTA_MUSICAS);
                }

                arquivosMusica = Directory.GetFiles(PASTA_MUSICAS, "*.mp3");

                if (arquivosMusica.Length == 0)
                {
                    MessageBox.Show("Nenhuma música encontrada na pasta ./Musicas",
                        "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                CriarPlaylistAleatoria();

                player.settings.volume = barraVolume.Value;
                player.settings.autoStart = false;

                player.PlayStateChange += Player_PlayStateChange;
                player.MediaError += Player_MediaError;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao configurar player: {ex.Message}",
                    "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CriarPlaylistAleatoria()
        {
            try
            {
                List<string> listaAleatoria = arquivosMusica.ToList();
                Random random = new Random();
                listaAleatoria = listaAleatoria.OrderBy(x => random.Next()).ToList();

                var playlist = player.playlistCollection.newPlaylist("KingMePlaylist");
                foreach (string arquivo in listaAleatoria)
                {
                    IWMPMedia media = player.newMedia(arquivo);
                    playlist.appendItem(media);
                }

                player.currentPlaylist = playlist;
                player.settings.setMode("loop", true);
                player.settings.setMode("shuffle", true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao criar playlist: {ex.Message}");
            }
        }

        private void Player_MediaError(object pMediaObject)
        {
            this.Invoke((MethodInvoker)delegate {
                lblMusicaAtual.Text = "Erro na reprodução";
                player.controls.next();
            });
        }

        private void Player_PlayStateChange(int newState)
        {
            if (newState == (int)WMPPlayState.wmppsMediaEnded)
            {
                if (modoDeFila)
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        player.controls.next();
                    });
                }
            }
            else if (newState == (int)WMPPlayState.wmppsPlaying)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    btnTocarPausar.Text = "❚❚";
                    AtualizarNomeMusica();
                    MusicaIniciada?.Invoke(this, EventArgs.Empty);
                });
            }
            else if (newState == (int)WMPPlayState.wmppsPaused || newState == (int)WMPPlayState.wmppsStopped)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    btnTocarPausar.Text = "▶";
                    MusicaParada?.Invoke(this, EventArgs.Empty);
                });
            }
        }
        

        private void BtnTocarPausar_Click(object sender, EventArgs e)
        {
            if (player.playState == WMPPlayState.wmppsPaused ||
                player.playState == WMPPlayState.wmppsStopped)
            {
                player.controls.play();
            }
            else
            {
                player.controls.pause();
            }
        }

        private void BtnSom_Click(object sender, EventArgs e)
        {
            somDesativado = !somDesativado;
            player.settings.mute = somDesativado;
            btnSom.Text = somDesativado ? "🔇" : "🔊";
        }

        private void BtnProxima_Click(object sender, EventArgs e)
        {
            player.controls.next();
        }

        private void BtnAnterior_Click(object sender, EventArgs e)
        {
            player.controls.previous();
        }

        private void BtnAleatorio_Click(object sender, EventArgs e)
        {
            modoDeFila = !modoDeFila;
            player.settings.setMode("shuffle", modoDeFila);
            btnAleatorio.BackColor = modoDeFila ?
                System.Drawing.Color.FromArgb(57, 89, 156) :
                System.Drawing.Color.FromArgb(34, 197, 94);

            MessageBox.Show(modoDeFila ?
                "Modo aleatório ativado" :
                "Modo aleatório desativado",
                "Modo de Reprodução",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            if (modoDeFila)
            {
                CriarPlaylistAleatoria();
            }
        }

        private void BtnEscolherMusica_Click(object sender, EventArgs e)
        {
            using (var formSelecao = new FormSelecaoMusica(arquivosMusica))
            {
                if (formSelecao.ShowDialog() == DialogResult.OK)
                {
                    if (formSelecao.CaminhoMusicaSelecionada != null)
                    {
                        TocarMusica(formSelecao.CaminhoMusicaSelecionada);
                    }
                }
            }
        }

        private void TocarMusica(string caminho)
        {
            try
            {
                player.controls.stop();

                if (modoDeFila)
                {
                    IWMPMedia mediaAtual = player.newMedia(caminho);

                    var playlistAtual = player.currentPlaylist;

                    var playlistTemp = player.playlistCollection.newPlaylist("TempTrack");
                    playlistTemp.appendItem(mediaAtual);

                    for (int i = 0; i < playlistAtual.count; i++)
                    {
                        IWMPMedia item = playlistAtual.Item[i];
                        if (item.sourceURL != caminho)
                        {
                            playlistTemp.appendItem(item);
                        }
                    }

                    player.currentPlaylist = playlistTemp;
                }
                else
                {
                    var playlist = player.playlistCollection.newPlaylist("SelectedTrack");
                    IWMPMedia media = player.newMedia(caminho);
                    playlist.appendItem(media);
                    player.currentPlaylist = playlist;
                }

                player.controls.play();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao reproduzir música: {ex.Message}",
                    "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BarraVolume_Scroll(object sender, EventArgs e)
        {
            player.settings.volume = barraVolume.Value;
        }

        private void AtualizarNomeMusica()
        {
            try
            {
                if (player?.currentMedia != null)
                {
                    string nomeArquivo = Path.GetFileNameWithoutExtension(player.currentMedia.sourceURL);

                    this.Invoke((MethodInvoker)delegate {
                        lblMusicaAtual.Text = nomeArquivo ?? "Música Desconhecida";
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao atualizar nome da música: {ex.Message}");
                lblMusicaAtual.Text = "Erro ao carregar música";
            }
        }

        public void AtualizarTamanho(int largura, int altura)
        {
            this.Size = new System.Drawing.Size(largura, altura);

            int larguraBotao = 35;
            int espacamento = 5;
            int totalBotoes = painelBotoes.Controls.Count;

            if (this.Width < totalBotoes * (larguraBotao + espacamento))
            {
                larguraBotao = (this.Width / totalBotoes) - espacamento;
            }

            for (int i = 0; i < totalBotoes; i++)
            {
                painelBotoes.Controls[i].Size = new System.Drawing.Size(larguraBotao, 35);
                painelBotoes.Controls[i].Location = new System.Drawing.Point(
                    i * (larguraBotao + espacamento), 0);
            }
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            base.OnHandleDestroyed(e);

            timerAtualizacao?.Stop();
            timerAtualizacao?.Dispose();

            if (player != null)
            {
                player.PlayStateChange -= Player_PlayStateChange;
                player.MediaError -= Player_MediaError;
                player.controls.stop();
                player = null;
            }
        }

        private Button btnTocarPausar;
        private Button btnSom;
        private Button btnProxima;
        private Button btnAnterior;
        private Button btnEscolherMusica;
        private Button btnAleatorio;
        private Label lblMusicaAtual;
        private TrackBar barraVolume;
        private ProgressBar barraProgresso;
        private Panel painelBotoes;
    }

    public partial class FormSelecaoMusica : Form
    {
        public string CaminhoMusicaSelecionada { get; private set; }
        private string[] arquivosOriginais;

        public FormSelecaoMusica(string[] arquivosMusica)
        {
            InicializarComponentes();
            CarregarListaMusicas(arquivosMusica);
        }

        private void InicializarComponentes()
        {
            this.listaMusicas = new ListBox();
            this.botaoSelecionar = new Button();
            this.botaoCancelar = new Button();
            this.txtPesquisa = new TextBox();

            this.Text = "Selecionar Música";
            this.Size = new System.Drawing.Size(350, 450);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimizeBox = false;
            this.MaximizeBox = false;

            Color primaryColor = Color.FromArgb(57, 89, 156);
            Color secondaryColor = Color.FromArgb(241, 245, 249);
            Color accentColor = Color.FromArgb(34, 197, 94);

            this.BackColor = secondaryColor;

            this.txtPesquisa.Text = "Pesquisar música...";
            this.txtPesquisa.Dock = DockStyle.Top;
            this.txtPesquisa.TextChanged += TxtPesquisa_TextChanged;

            listaMusicas.Dock = DockStyle.Top;
            listaMusicas.Height = 320;
            listaMusicas.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            listaMusicas.BorderStyle = BorderStyle.FixedSingle;

            Panel panelBotoes = new Panel();
            panelBotoes.Height = 50;
            panelBotoes.Dock = DockStyle.Bottom;
            panelBotoes.BackColor = secondaryColor;

            botaoSelecionar.Text = "Selecionar";
            botaoCancelar.Text = "Cancelar";
            botaoSelecionar.Size = new System.Drawing.Size(100, 30);
            botaoCancelar.Size = new System.Drawing.Size(100, 30);
            botaoSelecionar.Location = new System.Drawing.Point(70, 10);
            botaoCancelar.Location = new System.Drawing.Point(180, 10);
            botaoSelecionar.DialogResult = DialogResult.OK;
            botaoCancelar.DialogResult = DialogResult.Cancel;

            botaoSelecionar.BackColor = accentColor;
            botaoSelecionar.ForeColor = Color.White;
            botaoSelecionar.FlatStyle = FlatStyle.Flat;
            botaoSelecionar.FlatAppearance.BorderSize = 0;
            botaoCancelar.FlatStyle = FlatStyle.Flat;
            botaoCancelar.BackColor = primaryColor;
            botaoCancelar.ForeColor = Color.White;
            botaoCancelar.FlatAppearance.BorderSize = 0;

            panelBotoes.Controls.Add(botaoSelecionar);
            panelBotoes.Controls.Add(botaoCancelar);

            this.Controls.Add(txtPesquisa);
            this.Controls.Add(listaMusicas);
            this.Controls.Add(panelBotoes);

            listaMusicas.DoubleClick += ListaMusicas_DuploClique;
            botaoSelecionar.Click += BotaoSelecionar_Click;
            this.AcceptButton = botaoSelecionar;
            this.CancelButton = botaoCancelar;
        }

        private void TxtPesquisa_TextChanged(object sender, EventArgs e)
        {
            string filtro = txtPesquisa.Text.ToLower();
            listaMusicas.BeginUpdate();
            listaMusicas.Items.Clear();

            foreach (string arquivo in arquivosOriginais)
            {
                string nomeArquivo = Path.GetFileNameWithoutExtension(arquivo);
                if (string.IsNullOrEmpty(filtro) || nomeArquivo.ToLower().Contains(filtro))
                {
                    listaMusicas.Items.Add(nomeArquivo);
                }
            }

            listaMusicas.EndUpdate();
        }

        private void CarregarListaMusicas(string[] arquivosMusica)
        {
            arquivosOriginais = arquivosMusica;
            listaMusicas.BeginUpdate();
            foreach (string arquivo in arquivosMusica)
            {
                listaMusicas.Items.Add(Path.GetFileNameWithoutExtension(arquivo));
            }
            listaMusicas.EndUpdate();

            if (listaMusicas.Items.Count > 0)
                listaMusicas.SelectedIndex = 0;
        }

        private void ListaMusicas_DuploClique(object sender, EventArgs e)
        {
            if (listaMusicas.SelectedItem != null)
            {
                SelecionarMusica();
            }
        }

        private void BotaoSelecionar_Click(object sender, EventArgs e)
        {
            if (listaMusicas.SelectedItem != null)
            {
                SelecionarMusica();
            }
        }

        private void SelecionarMusica()
        {
            string nomeArquivoSelecionado = listaMusicas.SelectedItem.ToString();

            string caminhoCompleto = arquivosOriginais
                .FirstOrDefault(a => Path.GetFileNameWithoutExtension(a) == nomeArquivoSelecionado);

            if (!string.IsNullOrEmpty(caminhoCompleto))
            {
                CaminhoMusicaSelecionada = caminhoCompleto;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private ListBox listaMusicas;
        private Button botaoSelecionar;
        private Button botaoCancelar;
        private TextBox txtPesquisa;
    }
}