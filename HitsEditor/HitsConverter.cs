using System;
using System.Windows.Forms;
using System.IO;
using hitslib;

namespace MegaHits_Converter
{
    public partial class HitsConverter : Form
    {
        private OpenFileDialog fileDialog = new OpenFileDialog();
        private MegaHitsFFmpeg megaHits = new MegaHitsFFmpeg();
        private MegaHitsNAudio megaHitsNAudio = new MegaHitsNAudio();
        public HitsConverter()
        {
            InitializeComponent();
        }

        private void MainUI_Load(object sender, EventArgs e)
        {
            qualityComboBox.SelectedIndex = 2;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            fileDialog.Filter = "Audio FIles|*.mp3;*.wav;*.ogg;|All Files(*.*)|*.*";
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                filePathTextBox.Text = fileDialog.FileName;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!File.Exists(filePathTextBox.Text)) {
                MessageBox.Show("No such file!","Error");
                return;
            }
            switch (qualityComboBox.SelectedIndex)
            {
                case 0:
                    megaHits.audioToHits(filePathTextBox.Text, filePathTextBox.Text + ".hits", AudioQuality.Efficient);
                    break;
                case 1:
                    megaHits.audioToHits(filePathTextBox.Text, filePathTextBox.Text + ".hits", AudioQuality.Balance);
                    break;
                case 2:
                    megaHits.audioToHits(filePathTextBox.Text, filePathTextBox.Text + ".hits", AudioQuality.Standard);
                    break;
                case 3:
                    megaHits.audioToHits(filePathTextBox.Text, filePathTextBox.Text + ".hits", AudioQuality.HQ);
                    break;
                default:
                    break;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
        }
    }
}
