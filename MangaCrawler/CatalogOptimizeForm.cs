using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MangaCrawlerLib;
using System.Threading;

namespace MangaCrawler
{
    public partial class CatalogOptimizeForm : Form
    {
        public CatalogOptimizeForm()
        {
            InitializeComponent();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            backgroundWorker.CancelAsync();
        }

        private void CatalogOptimizeForm_Load(object sender, EventArgs e)
        {
            label2.Text = String.Format(label2.Text, Catalog.GetCatalogSize() / 1024 / 1024,
                (long)(Settings.Instance.MangaSettings.MaxCatalogSize / 1024 / 1024 * Catalog.COMPACT_RATIO));

            backgroundWorker.RunWorkerAsync();
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Close();
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Catalog.Compact(Settings.Instance.MangaSettings.MaxCatalogSize, 
                () => backgroundWorker.CancellationPending);
        }
    }
}
