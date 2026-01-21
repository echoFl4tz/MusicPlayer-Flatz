using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace MusicPlayer_Flatz
{
    public partial class Form1 : Form
    {
        #region === VISUAL ===
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= 0x20000;
                return cp;
            }
        }
        #endregion

        #region === CAMPOS ===
        private readonly string saveFile = "musicas.txt";
        private readonly Random random = new Random();

        private bool isShuffle;
        private bool isPaused;

        private List<string> musicFiles = new List<string>();
        private string currentSong;
        #endregion

        #region === LOAD ===
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadSavedMusics();
            MakeCoverRound();

            // fundo padrão

        }
        #endregion

        #region === ORGANIZAÇÃO ===
        private void LoadSavedMusics()
        {
            if (!System.IO.File.Exists(saveFile)) return;

            musicFiles = System.IO.File.ReadAllLines(saveFile).ToList();
            foreach (string file in musicFiles)
                listBox1.Items.Add(Path.GetFileName(file));
        }

        private void MakeCoverRound()
        {
            GraphicsPath gp = new GraphicsPath();
            gp.AddEllipse(0, 0, picCover1.Width, picCover1.Height);
            picCover1.Region = new Region(gp);
        }

        private void PlaySelectedMusic()
        {
            if (listBox1.SelectedIndex < 0) return;

            currentSong = musicFiles[listBox1.SelectedIndex];
            LoadMusicCover(currentSong);

            musicPlayer.URL = currentSong;
            musicPlayer.Ctlcontrols.play();

            isPaused = false;
            timerPlayback.Enabled = true;
        }

        private void PlayByIndex(int index)
        {
            if (index < 0 || index >= musicFiles.Count) return;

            listBox1.SelectedIndex = index;
            PlaySelectedMusic();
        }
        #endregion

        #region === CAPA / ARTISTA / BLUR ===
        private void LoadMusicCover(string filePath)
        {
            lblMusic.Text = "Artista desconhecido";
            Image coverImage = Properties.Resources.default_cover;

            try
            {
                var tagFile = TagLib.File.Create(filePath);

                if (!string.IsNullOrEmpty(tagFile.Tag.FirstPerformer))
                    lblMusic.Text = tagFile.Tag.FirstPerformer;

                if (tagFile.Tag.Pictures.Length > 0)
                {
                    var bin = tagFile.Tag.Pictures[0].Data.Data;
                    using (MemoryStream ms = new MemoryStream(bin))
                        coverImage = Image.FromStream(ms);
                }
            }
            catch { }

            picCover1.Image = coverImage;

            // aplica blur no fundo do FORM


        }
        #endregion

        #region === BOTÕES ===
        private void btnLoad_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "MP3 Files | *.mp3"
            };

            if (ofd.ShowDialog() != DialogResult.OK) return;

            foreach (string file in ofd.FileNames)
            {
                musicFiles.Add(file);
                listBox1.Items.Add(Path.GetFileName(file));
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (isPaused)
            {
                musicPlayer.Ctlcontrols.play();
                isPaused = false;
            }
            else
            {
                PlaySelectedMusic();
            }
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            if (musicPlayer.playState == WMPLib.WMPPlayState.wmppsPlaying)
            {
                musicPlayer.Ctlcontrols.pause();
                isPaused = true;
            }
            else
            {
                musicPlayer.Ctlcontrols.play();
                isPaused = false;
            }
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (musicFiles.Count == 0) return;

            int next = isShuffle ? random.Next(musicFiles.Count)
                                 : listBox1.SelectedIndex + 1;

            PlayByIndex(next);
        }

        private void btnPrev_Click(object sender, EventArgs e)
        {
            if (musicFiles.Count == 0) return;

            int prev = listBox1.SelectedIndex - 1;
            if (prev < 0) prev = 0;

            PlayByIndex(prev);
        }

        private void btnShuffle_Click(object sender, EventArgs e)
        {
            isShuffle = !isShuffle;
            btnShuffle.BackColor = isShuffle ? Color.Peru : Color.SaddleBrown;
        }
        #endregion

        #region === PLAYER ===
        private void musicPlayer_PlayStateChange(object sender,
            AxWMPLib._WMPOCXEvents_PlayStateChangeEvent e)
        {
            if (e.newState == 8)
                btnNext.PerformClick();
        }

        private void timerPlayback_Tick(object sender, EventArgs e)
        {
            if (musicPlayer.currentMedia == null) return;

            label1.Text =
                $"{FormatTime(musicPlayer.Ctlcontrols.currentPosition)} / " +
                $"{FormatTime(musicPlayer.currentMedia.duration)}";
        }

        private void volumeBar_Scroll(object sender, ScrollEventArgs e)
        {
            musicPlayer.settings.volume = volumeBar.Value;
        }
        #endregion

        #region === UTIL ===
        private string FormatTime(double seconds)
        {
            return TimeSpan.FromSeconds(seconds).ToString(@"mm\:ss");
        }


        #endregion

        #region === FECHAR ===
        private void siticoneCirclePictureBox1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            System.IO.File.WriteAllLines(saveFile, musicFiles);
        }
        #endregion

        private void siticoneCirclePictureBox1_Click_1(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
