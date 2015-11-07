using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace oras_dsf
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            richTextBox1.AllowDrop = true;
            this.DragEnter += new DragEventHandler(tabMain_DragEnter);
            this.DragDrop += new DragEventHandler(tabMain_DragDrop);
            richTextBox1.DragEnter += new DragEventHandler(tabMain_DragEnter);
            richTextBox1.DragDrop += new DragEventHandler(tabMain_DragDrop);
            richTextBox1.Text = "Drop save file in, and the program will overwrite with the fixed checksums.\n\nBy Kaphotics\nProjectPokemon.org";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "All Files|*.*";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string path = ofd.FileName;
                parse(path);
            }
        }
        private void parse(string path)
        {
            int savelen = 0x5A00;
            byte[] data = File.ReadAllBytes(path);
            if (data.Length != savelen)
                MessageBox.Show("Save data is not valid size!\n\n(0x5A00 / 23040 bytes)", "Error");

            uint[] start =  {
                               0x0000, // 0
                               0x0C00, // 1
                               0x0E00, // 2
                               0x1000, // 3
                               0x1200, // 4
                               0x1400, // 5
                               0x1600, // 6
                               0x1800, // 7
                               0x3A00, // 8
                               0x3C00, // 9
                               0x3E00, // A
                               0x4600, // B
                               0x4C00, // C
                               0x4E00, // D
                               0x5000, // E
                               0x5400, // F
                            };
            uint[] length = {
                                0xB90,	// 0 [0x0000]
                                0x2C,	// 1 [0x0C00]
                                0x38,	// 2 [0x0E00]
                                0x150,	// 3 [0x1000]
                                0x04,	// 4 [0x1200]
                                0x08,	// 5 [0x1400]
                                0x24,	// 6 [0x1600]
                                0x2100,	// 7 [0x1800] overworld data
                                0x130,	// 8 [0x3A00]
                                0x170,	// 9 [0x3C00] trainer data
                                0x61C,	// A [0x3E00] party data
                                0x504,	// B [0x4600]
                                0x04,	// C [0x4C00]
                                0x48,	// D [0x4E00]
                                0x400,	// E [0x5000]
                                0x25C,	// F [0x5400]
                            };

            int csoff = 0x5800;
            richTextBox1.Text = "Fixing Save File...\n";
            for (int i = 0; i < length.Length; i++)
            {
                ushort curchk = BitConverter.ToUInt16(data,csoff + i * 8 + 0x1A);
                richTextBox1.AppendText(i.ToString("X1") + " - Current CHK: " + curchk.ToString("X4"));
                byte[] chkregion = new Byte[length[i]];
                Array.Copy(data, start[i], chkregion, 0, length[i]);
                ushort checksum = ccitt16(chkregion);
                Array.Copy(BitConverter.GetBytes(checksum), 0, data, csoff + i * 8 + 0x1A, 2);
                richTextBox1.AppendText(" | New CHK: " + checksum.ToString("X4")+"\n");
            }
            richTextBox1.AppendText("All fixed! Overwriting save with fixed version.\n");
            try
            {
                File.WriteAllBytes(path, data);
                richTextBox1.AppendText("Saved to:\n" + path);
            }
            catch (Exception e) { MessageBox.Show("Unable to save.\n\n" + e, "Error"); }
        }
        internal static UInt16 ccitt16(byte[] data)
        {
            ushort crc = 0xFFFF;
            for (int i = 0; i < data.Length; i++)
            {
                crc ^= (ushort)(data[i] << 8);
                for (int j = 0; j < 8; j++)
                {
                    if ((crc & 0x8000) > 0)
                        crc = (ushort)((crc << 1) ^ 0x1021);
                    else
                        crc <<= 1;
                }
            }
            return crc;
        }
        private void tabMain_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }
        private void tabMain_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            string path = files[0]; // open first D&D
            parse(path);
        }
    }
}
