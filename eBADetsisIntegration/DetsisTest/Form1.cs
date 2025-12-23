using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DetsisTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ATSO.IntegrationSoapClient client = new ATSO.IntegrationSoapClient();
           DataTable dt =  client.kurumAdinaGoreSorgula("Kocaeli", 0);
            string a = "ads";
        }
    }
}
