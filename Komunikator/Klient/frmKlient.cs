using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;
using komunikaty;


namespace Klient
{
    public partial class frmKlient : Form
    {
        private TcpClient klient = null;
        private bool czypolaczono = false;
        private BinaryReader r = null;
        private BinaryWriter w = null;

        public frmKlient()
        {
            InitializeComponent();
        }

        public void wyswietl(RichTextBox o, string tekst)
        {
            o.Focus();
            o.AppendText(tekst);
            o.ScrollToCaret();
            txtWysylane.Focus();
        }

        private void Polaczenie_DoWork(object sender, DoWorkEventArgs e)
        {
            klient = new TcpClient();
            wyswietl(txtLog, "Próbuje się połączyć\n");
            klient.Connect(IPAddress.Parse(txtIP.Text), int.Parse(txtPort.Text));
            wyswietl(txtLog, "Połączenie nawiązane\nŻądam zezwolenia\n");
            NetworkStream stream = klient.GetStream();
            w = new BinaryWriter(stream);
            r = new BinaryReader(stream);
            w.Write(KomunikatyKlienta.Zadaj);
            if (r.ReadString() == KomunikatySerwera.OK)
            {
                wyswietl(txtLog, "Połączono\n");
                czypolaczono = true;
                cmdWyslij.Enabled = true;
                Odbieranie.RunWorkerAsync();
            }
            else
            {
                wyswietl(txtLog, "Brak odpowiedzi\nRozlaczono\n");
                czypolaczono = false;
                if (klient != null) klient.Close();
                cmdWyslij.Enabled = false;
                cmdPolacz.Text = "Połącz";
            }
        }

        private void Odbieranie_DoWork(object sender, DoWorkEventArgs e)
        {
            string tekst;
            while ((tekst = r.ReadString()) != KomunikatySerwera.Rozlacz)
            {
                wyswietl(txtOdbieranie, "===== Rozmówca =====\n" + tekst + '\n');
            }
            cmdWyslij.Enabled = false;
            wyswietl(txtLog, "Rozlaczono\n");
            cmdPolacz.Text = "Połącz";
            czypolaczono = false;
            if (klient != null) klient.Close();
        }

        private void cmdWyslij_Click(object sender, EventArgs e)
        {
            string tekst = txtWysylane.Text;
            if (tekst == "") { txtWysylane.Focus(); return; }
            if (tekst[tekst.Length - 1] == '\n')
                tekst = tekst.TrimEnd('\n');
            w.Write(tekst);
            wyswietl(txtOdbieranie, "===== Ja =====\n" + tekst + '\n');
            txtWysylane.Text = "";
        }

        private void cmdPolacz_Click(object sender, EventArgs e)
        {
            if (cmdPolacz.Text == "Połącz")
            {
                Polaczenie.RunWorkerAsync();
                cmdPolacz.Text = "Rozłącz";
            }
            else
            {
                if (czypolaczono)
                {
                    w.Write(KomunikatyKlienta.Rozlacz);
                    klient.Close();
                    czypolaczono = false;
                }
                cmdPolacz.Text = "Połącz";
                cmdWyslij.Enabled = false;
                wyswietl(txtLog, "Rozlaczono\n");
            }
        }

        private void frmKlient_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (czypolaczono)
            {
                w.Write(KomunikatyKlienta.Rozlacz);
                klient.Close();
                czypolaczono = false;
            }
            Polaczenie.CancelAsync();
            Odbieranie.CancelAsync();
        }
    }
}
