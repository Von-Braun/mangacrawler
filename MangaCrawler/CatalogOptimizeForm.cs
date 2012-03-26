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
using MangaCrawler.Properties;

namespace MangaCrawler
{
    public partial class CatalogOptimizeForm : Form
    {
        public CatalogOptimizeForm()
        {
            Icon = Icon.FromHandle(Resources.Manga_Crawler_Orange.GetHicon());
            InitializeComponent();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            backgroundWorker.CancelAsync();
        }

        private void CatalogOptimizeForm_Load(object sender, EventArgs e)
        {
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
