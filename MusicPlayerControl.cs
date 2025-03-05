using System;
using System.IO;
using System.Windows.Forms;
using WMPLib;

namespace ClientKingMe
{
    public partial class MusicPlayerControl : UserControl
    {
        private WindowsMediaPlayer wplayer;
        private bool isMuted = false;
        private string[] musicFiles;

        public MusicPlayerControl()
        {
            InitializeComponent();
            ConfigurarReproducaoMusical();
        }

        private void InitializeComponent()
        {
            this.btnPlayPause = new Button();
            this.btnMuteUnmute = new Button();
            this.btnProximaMusica = new Button();
            this.btnSelecionarMusica = new Button();
            this.lblCurrentSong = new Label();
            this.trackBarVolume = new TrackBar();
            this.panel1 = new Panel();

            this.panel1.Height = 40;
            this.panel1.Dock = DockStyle.Bottom;

            this.btnPlayPause.Text = "▶";
            this.btnMuteUnmute.Text = "🔊";
            this.btnProximaMusica.Text = "⏭";
            this.btnSelecionarMusica.Text = "📁";
            this.lblCurrentSong.Text = "Nenhuma música";

            this.btnPlayPause.Click += btnPlayPause_Click;
            this.btnMuteUnmute.Click += btnMuteUnmute_Click;
            this.btnProximaMusica.Click += btnProximaMusica_Click;
            this.btnSelecionarMusica.Click += BtnSelecionarMusica_Click;
            this.trackBarVolume.Scroll += TrackBarVolume_Scroll;

            this.panel1.Controls.Add(btnPlayPause);
            this.panel1.Controls.Add(btnMuteUnmute);
            this.panel1.Controls.Add(btnProximaMusica);
            this.panel1.Controls.Add(btnSelecionarMusica);
            int buttonWidth = 40;
            int spacing = 5;
            btnPlayPause.Location = new System.Drawing.Point(0, 0);
            btnMuteUnmute.Location = new System.Drawing.Point(buttonWidth + spacing, 0);
            btnProximaMusica.Location = new System.Drawing.Point((buttonWidth + spacing) * 2, 0);
            btnSelecionarMusica.Location = new System.Drawing.Point((buttonWidth + spacing) * 3, 0);
            foreach (Button btn in panel1.Controls)
            {
                btn.Size = new System.Drawing.Size(buttonWidth, 40);
            }

            this.Controls.Add(panel1);
            this.Controls.Add(lblCurrentSong);
            this.Controls.Add(trackBarVolume);
            lblCurrentSong.Dock = DockStyle.Top;
            trackBarVolume.Dock = DockStyle.Bottom;
            this.Size = new System.Drawing.Size(200, 150);
        }

        private void ConfigurarReproducaoMusical()
        {
            try
            {
                wplayer = new WindowsMediaPlayer();

                string musicFolder = "./Musicas";

                musicFiles = Directory.GetFiles(musicFolder, "*.mp3");

                if (musicFiles.Length == 0)
                {
                    MessageBox.Show("Nenhuma música encontrada na pasta ./Musicas");
                    return;
                }
                var playlist = wplayer.playlistCollection.newPlaylist("KingMePlaylist");
                foreach (string musicFile in musicFiles)
                {
                    IWMPMedia media = wplayer.newMedia(musicFile);
                    playlist.appendItem(media);
                }
                wplayer.currentPlaylist = playlist;
                wplayer.settings.setMode("loop", true);
                wplayer.settings.autoStart = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao configurar reprodução musical: {ex.Message}");
            }
        }

        private void btnPlayPause_Click(object sender, EventArgs e)
        {
            if (wplayer.playState == WMPPlayState.wmppsPaused ||
                wplayer.playState == WMPPlayState.wmppsStopped)
            {
                wplayer.controls.play();
                btnPlayPause.Text = "❚❚"; 
            }
            else
            {
                wplayer.controls.pause();
                btnPlayPause.Text = "▶"; 
            }
            UpdateCurrentSongName();
        }

        private void btnMuteUnmute_Click(object sender, EventArgs e)
        {
            isMuted = !isMuted;
            wplayer.settings.mute = isMuted;
            btnMuteUnmute.Text = isMuted ? "🔇" : "🔊";
        }

        private void btnProximaMusica_Click(object sender, EventArgs e)
        {
            wplayer.controls.next();
            UpdateCurrentSongName();
        }

        private void BtnSelecionarMusica_Click(object sender, EventArgs e)
        {
            using (var formSelecao = new FormSelecaoMusica(musicFiles))
            {
                if (formSelecao.ShowDialog() == DialogResult.OK)
                {
                    if (formSelecao.SelectedMusicPath != null)
                    {
                        PlaySelectedMusic(formSelecao.SelectedMusicPath);
                    }
                }
            }
        }

        private void PlaySelectedMusic(string musicPath)
        {
            try
            {
                wplayer.controls.stop();

                var playlist = wplayer.playlistCollection.newPlaylist("SelectedTrack");
                IWMPMedia media = wplayer.newMedia(musicPath);
                playlist.appendItem(media);

                wplayer.currentPlaylist = playlist;
                wplayer.controls.play();

                UpdateCurrentSongName();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao reproduzir música: {ex.Message}");
            }
        }

        private void TrackBarVolume_Scroll(object sender, EventArgs e)
        {
            wplayer.settings.volume = trackBarVolume.Value;
        }

        private void UpdateCurrentSongName()
        {
            try
            {
                if (wplayer?.currentMedia != null)
                {
                    string fileName = Path.GetFileNameWithoutExtension(wplayer.currentMedia.sourceURL);

                    //Invoke é necessário para atualizar a UI de um thread diferente
                    this.Invoke((MethodInvoker)delegate {
                        lblCurrentSong.Text = fileName ?? "Música Desconhecida";
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao atualizar nome da música: {ex.Message}");
                lblCurrentSong.Text = "Erro ao carregar música";
            }
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            base.OnHandleDestroyed(e);
            if (wplayer != null)
            {
                wplayer.controls.stop();
                wplayer = null;
            }
        }

        private Button btnPlayPause;
        private Button btnMuteUnmute;
        private Button btnProximaMusica;
        private Button btnSelecionarMusica;
        private Label lblCurrentSong;
        private TrackBar trackBarVolume;
        private Panel panel1;
    }

    public partial class FormSelecaoMusica : Form
    {
        public string SelectedMusicPath { get; private set; }

        public FormSelecaoMusica(string[] musicFiles)
        {
            InitializeComponent();
            PopularListaMusicas(musicFiles);
        }

        private void InitializeComponent()
        {
            this.listBoxMusicas = new ListBox();
            this.btnSelecionar = new Button();
            this.btnCancelar = new Button();

            this.Text = "Selecionar Música";
            this.Size = new System.Drawing.Size(300, 400);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;

            listBoxMusicas.Dock = DockStyle.Top;
            listBoxMusicas.Height = 300;

            btnSelecionar.Text = "Selecionar";
            btnCancelar.Text = "Cancelar";
            btnSelecionar.DialogResult = DialogResult.OK;
            btnCancelar.DialogResult = DialogResult.Cancel;

            btnSelecionar.Location = new System.Drawing.Point(50, 320);
            btnCancelar.Location = new System.Drawing.Point(150, 320);

            this.Controls.Add(listBoxMusicas);
            this.Controls.Add(btnSelecionar);
            this.Controls.Add(btnCancelar);

            listBoxMusicas.DoubleClick += ListBoxMusicas_DoubleClick;
            btnSelecionar.Click += BtnSelecionar_Click;
        }

        private void PopularListaMusicas(string[] musicFiles)
        {
            foreach (string musicFile in musicFiles)
            {
                listBoxMusicas.Items.Add(Path.GetFileNameWithoutExtension(musicFile));
            }
        }

        private void ListBoxMusicas_DoubleClick(object sender, EventArgs e)
        {
            if (listBoxMusicas.SelectedItem != null)
            {
                SelecionarMusica();
            }
        }

        private void BtnSelecionar_Click(object sender, EventArgs e)
        {
            if (listBoxMusicas.SelectedItem != null)
            {
                SelecionarMusica();
            }
        }

        private void SelecionarMusica()
        {
            string selectedFileName = listBoxMusicas.SelectedItem.ToString() + ".mp3";
            SelectedMusicPath = Path.Combine("./Musicas", selectedFileName);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private ListBox listBoxMusicas;
        private Button btnSelecionar;
        private Button btnCancelar;
    }
}