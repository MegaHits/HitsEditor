using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.Text.Json;
using hitslib;
using System.Runtime.InteropServices;

namespace HitsEditor
{
    public partial class MainUI : Form
    {
        private string fileLoaded = "";
        private HitsProject project = new();
        private int romSpaceLeft = 0;
        private MegaHitsNAudio nAudio = new();
        private List<Byte> compiledROM = new();
        private HitsCompiler compiler = new();
        public MainUI()
        {
            InitializeComponent();
            project.PlayList = new List<PlaylistItem>();
            flashSizeComboBox.SelectedIndex = 0;
            powerOnLoopComboBox.SelectedIndex = 0;
            powerOnPlayComboBox.SelectedIndex = 0;
            antiNoiseDebounceComboBox.SelectedIndex = 0;
            sampleRateComboBox.SelectedIndex = 0;
            OKY1ComboBox.SelectedIndex = 0;
            OKY2ComboBox.SelectedIndex = 0;
        }

        private void tabPage2_Click(object sender, EventArgs e)
        {

        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            saveProject();
        }

        private void saveProject()
        {
            uiToProject();
            if (!File.Exists(fileLoaded))
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Title = "Save Project File",
                    Filter = "MegaHits Json|*.hson;*.json"
                };
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    fileLoaded = saveFileDialog.FileName;
                }
                else
                {
                    return;
                }
            }
            using var jDoc = JsonDocument.Parse(JsonSerializer.Serialize(project));
            File.WriteAllText(fileLoaded, JsonSerializer.Serialize(jDoc, new JsonSerializerOptions { WriteIndented = true }));
        }

        private void uiToProject()
        {
            project.ProjectName = nameTextBox.Text;
            project.ProjectNumber = noTextBox.Text;
            project.Description = descriptionTextBox.Text;
            project.FlashSize = flashSizeComboBox.SelectedIndex;
            project.SampleRate = sampleRateComboBox.SelectedIndex;
        }

        private void projectToUI()
        {
            int totalVoices = 0;
            nameTextBox.Text = project.ProjectName;
            noTextBox.Text = project.ProjectNumber;
            descriptionTextBox.Text = project.Description;
            flashSizeComboBox.SelectedIndex = project.FlashSize;
            powerOnLoopComboBox.SelectedIndex = project.PowerOnLoop;
            powerOnPlayComboBox.SelectedIndex = project.PowerOnPlay;
            antiNoiseDebounceComboBox.SelectedIndex = project.AntiNoiseDebounce;
            sampleRateComboBox.SelectedIndex = project.SampleRate;
            listView1.Items.Clear();
            foreach (PlaylistItem item in project.PlayList)
            {
                ListViewItem listViewItem = new(item.fileName);
                listViewItem.SubItems.Add(item.sampleSize.ToString("X")+"H");
                listViewItem.SubItems.Add(item.audioQuality.ToString());
                listView1.Items.Add(listViewItem);
                totalVoices++;
            }
            updateFreeFlashSpace();
            totalVoiceSectionTextBox.Text = totalVoices.ToString();
            OKY1ComboBox.SelectedIndex = project.OKY1Config;
            OKY2ComboBox.SelectedIndex = project.OKY2Config;
        }

        private void loadProject()
        {
            OpenFileDialog openFileDialog = new();
            openFileDialog.Filter = "MegaHits Json|*.hson;*.json|All Files|*.*";
            if (openFileDialog.ShowDialog() == DialogResult.OK && File.Exists(openFileDialog.FileName))
            {
                fileLoaded = openFileDialog.FileName;
            }
            else
            {
                return;
            }
            try
            {
                project = JsonSerializer.Deserialize<HitsProject>(File.ReadAllText(fileLoaded));
                projectToUI();
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox aboutBox = new();
            aboutBox.ShowDialog();
        }

        private void openProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            loadProject();
        }

        private void flashSizeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            updateFreeFlashSpace();
        }

        private void updateFreeFlashSpace()
        {
            romSpaceLeft = Convert.ToInt32(flashSizeComboBox.SelectedItem.ToString()) * 1024 * 1024 / 8 - 0x4000;
            int totalSpace = 0;
            int totalFlash = romSpaceLeft;
            if (project.PlayList != null)
            {
                foreach (PlaylistItem item in project.PlayList)
                {
                    totalSpace += item.sampleSize;
                }
            }
            romSpaceLeft -= totalSpace;
            romSpaceLeftLabel.Text = (romSpaceLeft < 0) ? "-" : "";
            romSpaceLeftLabel.Text += Math.Abs(romSpaceLeft).ToString("X") + "H / "+ totalFlash.ToString("X")+"H";
            if (romSpaceLeft < 0)
            {
                romSpaceLeftLabel.ForeColor = Color.Red;
            }
            else
            {
                romSpaceLeftLabel.ForeColor = Color.Black;
            }
            uiToProject();
        }

        private void addClipButton_Click(object sender, EventArgs e)
        {
            if (fileLoaded == "")
            {
                MessageBox.Show("Please save the project before adding songs to the playlist","Save the project");
                return;
            }
            OpenFileDialog openFileDialog = new();
            openFileDialog.Filter = "Single Clip File|*.hits";
            if (openFileDialog.ShowDialog() == DialogResult.OK && File.Exists(openFileDialog.FileName))
            {
                if (!File.Exists(Path.GetDirectoryName(fileLoaded) + "/" + Path.GetFileName(openFileDialog.FileName)))
                {
                    File.Copy(openFileDialog.FileName, Path.GetDirectoryName(fileLoaded) + "/" + Path.GetFileName(openFileDialog.FileName));
                }
            }
            else
            {
                return;
            }
            byte[] tempByte = File.ReadAllBytes(Path.GetDirectoryName(fileLoaded) + "/" + Path.GetFileName(openFileDialog.FileName));
            PlaylistItem playlistItem = new();
            playlistItem.fileName = Path.GetFileName(openFileDialog.FileName);
            playlistItem.audioQuality = AudioQualityUtil.GetAudioQualityFromFile(Path.GetDirectoryName(fileLoaded) + "/" + Path.GetFileName(openFileDialog.FileName));
            playlistItem.sampleSize = tempByte.Length - 0x4000;
            project.PlayList.Add(playlistItem);
            projectToUI();
        }

        private void playButton_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) {
                return;
            }
            switch (listView1.SelectedItems[0].SubItems[2].Text) {
                case "Standard":
                    nAudio.playAudio(File.ReadAllBytes(Path.GetDirectoryName(fileLoaded) + "/" + listView1.SelectedItems[0].Text), AudioQuality.Standard);
                    break;
                case "Balance":
                    nAudio.playAudio(File.ReadAllBytes(Path.GetDirectoryName(fileLoaded) + "/" + listView1.SelectedItems[0].Text), AudioQuality.Balance);
                    break;
                case "Efficient":
                    nAudio.playAudio(File.ReadAllBytes(Path.GetDirectoryName(fileLoaded) + "/" + listView1.SelectedItems[0].Text), AudioQuality.Efficient);
                    break;
                default:
                    break;
            }
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            nAudio.stopAudio();
        }

        private void removeClipButton_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
            {
                return;
            }
            project.PlayList.RemoveAt(listView1.SelectedIndices[0]);
            projectToUI();
        }

        private void moveClipUpButton_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
            {
                return;
            }
            int index = listView1.SelectedIndices[0];
            if (listView1.SelectedIndices[0] == 0)
            {
                return;
            }
            PlaylistItem item = project.PlayList[index];
            project.PlayList.Insert(index - 1, item);
            project.PlayList.RemoveAt(index+1);
            projectToUI();
            listView1.Items[index - 1].Selected = true;
        }

        private void moveClipDownButton_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
            {
                return;
            }
            int index = listView1.SelectedIndices[0];
            if (listView1.SelectedIndices[0] >= project.PlayList.Count()-1)
            {
                return;
            }
            PlaylistItem item = project.PlayList[index];
            project.PlayList.Insert(index + 2, item);
            project.PlayList.RemoveAt(index);
            projectToUI();
            listView1.Items[index + 1].Selected = true;
        }

        private bool compileROM() {
            if (romSpaceLeft < 0) {
                MessageBox.Show("ROM size exceeds file size.\nReduce your content or select a larger flash chip.","Compile Error");
            }
            compiledROM = compiler.compile(project, Path.GetDirectoryName(fileLoaded));
            return true;
        }

        private void exportROM() {
            SaveFileDialog saveFileDialog = new();
            saveFileDialog.Filter = "MegaHits Multi-song ROM|*.bin";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllBytes(saveFileDialog.FileName,compiledROM.ToArray());
            }
        }

        private void compileToolStripButton_Click(object sender, EventArgs e)
        {
            if (fileLoaded == "")
            {
                MessageBox.Show("Please save the project before compiling the ROM", "Save the project");
                return;
            }
            compileROM();
        }

        private void exportToolStripButton_Click(object sender, EventArgs e)
        {
            if (fileLoaded == "")
            {
                MessageBox.Show("Please save the project before compiling the ROM", "Save the project");
                return;
            }
            if (compileROM())
            {
                exportROM();
            }
        }

        private void exportROMToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (fileLoaded == "")
            {
                MessageBox.Show("Please save the project before compiling the ROM", "Save the project");
                return;
            }
            if (compileROM())
            {
                exportROM();
            }
        }

        private void compileROMToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (fileLoaded == "")
            {
                MessageBox.Show("Please save the project before compiling the ROM", "Save the project");
                return;
            }
            compileROM();
        }

        private void newProject() {
            fileLoaded = "";
            project = new HitsProject();
            flashSizeComboBox.SelectedIndex = 0;
            powerOnLoopComboBox.SelectedIndex = 0;
            powerOnPlayComboBox.SelectedIndex = 0;
            antiNoiseDebounceComboBox.SelectedIndex = 0;
            sampleRateComboBox.SelectedIndex = 0;
            project.PlayList = new List<PlaylistItem>();
            projectToUI();
        }
        private void newProjectToolstripButton_Click(object sender, EventArgs e)
        {
            newProject();
        }

        private void newProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            newProject();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void hitsPlayerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MegaHits_Converter.HitsConverter hitsConverter = new();
            hitsConverter.ShowDialog();
        }

        private void OKY1ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (OKY1ComboBox.SelectedIndex)
            {
                case 0:
                    OKY1Description.Text = "Ignore the input on this OKY";
                    break;
                case 1:
                    OKY1Description.Text = "Play the next clip when triggered";
                    break;
                default:
                    OKY1Description.Text = "";
                    break;
            }
            project.OKY1Config = OKY1ComboBox.SelectedIndex;
            projectToUI();
        }

        private void OKY2ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (OKY2ComboBox.SelectedIndex)
            {
                case 0:
                    OKY2Description.Text = "Ignore the input on this OKY";
                    break;
                case 1:
                    OKY2Description.Text = "Play the next clip when triggered";
                    break;
                default:
                    OKY2Description.Text = "";
                    break;
            }
            project.OKY2Config = OKY2ComboBox.SelectedIndex;
            projectToUI();
        }

        private void projectToUIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            projectToUI();
        }

        private void powerOnPlayComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            project.PowerOnPlay = powerOnPlayComboBox.SelectedIndex;
            projectToUI();
        }

        private void powerOnLoopComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            project.PowerOnLoop = powerOnLoopComboBox.SelectedIndex;
            projectToUI();
        }

        private void antiNoiseDebounceComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            project.AntiNoiseDebounce = antiNoiseDebounceComboBox.SelectedIndex;
            projectToUI();
        }

        private void projectInfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(project.ToString(),"Project Info");
        }
    }
}
