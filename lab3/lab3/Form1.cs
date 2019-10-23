using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace lab3
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        int N = 26;
        double[] C, I, Cs, Is, taus, hx, Vy, hxy;
        double[,] px, tau, py;
        double[,,] pxy, s_pxy;
        Random rand = new Random();
        int K = 1;
        int round = 10;

        public void Calc_CI(int kv)
        {
            C = new double[kv];
            I = new double[kv];
            Vy = new double[kv];
            taus = new double[kv];
            Calc_Hx(K);
            for (int k = 0; k < K; k++)
            {
                taus[k] = 0;
                for (int i = 0; i < N; i++)
                    taus[k] += px[k, i] * tau[k, i];
                Vy[k] = 1.0 / taus[k];
                C[k] = Vy[k] * Math.Log(N, 2);
                I[k] = Vy[k] * hx[k];
            }
        }

        public void Calc_Is(int kv)
        {
            Is = new double[kv];
            Calc_Hxy(K);
            for (int k = 0; k < kv; k++)
            {
                Is[k] = Vy[k] * (hx[k] - hxy[k]);
            }
        }

        public void Calc_Hx(int kv)
        {
            hx = new double[kv];
            for (int k = 0; k < kv; k++)
            {
                hx[k] = 0;
                for (int i = 0; i < N; i++) hx[k] -= px[k, i] * Math.Log(px[k, i], 2);
            }
        }

        public void Calc_pxy(int kv)
        {
            pxy = new double[kv, N, N];
            double q = 1.0 / (2.0 * N);
            for (int k = 0; k < K; k++)
            {
                for (int i = 0; i < N; i++)
                    for (int j = 0; j < N; j++)
                        if (i != j) pxy[k, i, j] = q * rand.NextDouble();
                for (int i = 0; i < N; i++)
                {
                    double s = 0;
                    for (int j = 0; j < N; j++)
                    {
                        s += pxy[k, i, j];
                    }
                    pxy[k, i, i] = 1 - s;
                }
            }
        }

        public void Calc_tau(int kv)
        {
            tau = new double[kv, N];
            double mn = Math.Pow(10, -6);
            for (int k = 0; k < K; k++)
            {
                for (int i = 0; i < N; i++)
                    tau[k, i] = mn*N * rand.NextDouble();
            }
        }

        public void Calc_px(int kv)
        {
            richTextBox1.Clear();
            px = new double[kv, N];
            for (int k = 0; k < K; k++)
            {
                double b = 0;
                double s = 0;
                int offset = 0;
                int count = 0;
                if (N % 10 > 0 || N == 10)
                {
                    b = 1 / (Convert.ToDouble(N) / 2);
                    offset = N / (N / 2);
                }
                else
                {
                    b = 1.0 / 10;
                    offset = N / 10;
                }
                while (s != 1 && count < N)
                {
                    double bv = b;
                    double sv = 0;
                    for (int i = count; i < count + offset - 1; i++)
                    {
                        px[k, i] = bv * rand.NextDouble();
                        bv -= px[k, i];
                        sv += px[k, i];

                    }
                    px[k, count + offset - 1] = b - sv;
                    sv += px[k, count + offset - 1];
                    s += sv;
                    count += offset;
                }
            }
        }

        public void Calc_Hxy(int kv)
        {
            hxy = new double[kv];
            Calc_s_pxy(K);
            for (int k = 0; k < kv; k++)
            {
                hxy[k] = 0;
                for (int i = 0; i < N; i++)
                    for (int j = 0; j < N; j++)
                        hxy[k] -= s_pxy[k, i, j] * Math.Log(pxy[k, i, j], 2);
            }
        }

        public void Calc_py(int kv)
        {
            py = new double[kv, N];
            for (int k = 0; k < kv; k++)
            {
                for (int i = 0; i < N; i++) py[k, i] = 0;
                for (int j = 0; j < N; j++)
                {
                    for (int i = 0; i < N; i++)
                    {
                        py[k, j] += px[k, i] * pxy[k, i, j];
                    }
                }
            }
        }

        public void Calc_s_pxy(int kv)
        {
            Calc_py(K);
            s_pxy = new double[kv, N, N];
            for (int k = 0; k < K; k++)
            {
                for (int i = 0; i < N; i++)
                    for (int j = 0; j < N; j++)
                        s_pxy[k, i, j] = py[k, j] * pxy[k, i, j];
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Calc_px(K);
            Calc_tau(K);
            Output_px(K - 1);
            Output_tau(K - 1);
            Calc_pxy(K);
            Output_pxy(K - 1, true);
            Calc_CI(K);
            Calc_Is(K);
            Output_CI(K - 1);
        }

        public void Output_px(int k)
        {
            richTextBox3.Clear();
            richTextBox1.Clear();
            richTextBox1.AppendText("px : \n");
            double s = 0;
            for (int i = 0; i < N; i++)
            {
                richTextBox1.AppendText("px[" + String.Format("{0:d2}", i + 1) + "] = " + Math.Round(px[k, i], round).ToString() + "\n");
                s += px[k, i];
            }
            richTextBox1.AppendText("\n");
            richTextBox3.AppendText("p(x): \n" + "s = " + Math.Round(s, round).ToString() + "\n\n");
        }

        public void Output_tau(int k)
        {
            
            richTextBox1.AppendText("tau : \n");
           
            for (int i = 0; i < N; i++)
            {
                richTextBox1.AppendText("tau[" + String.Format("{0:d2}", i + 1) + "] = " + Math.Round((tau[k,i]*Math.Pow(10, 6)), round).ToString() + "\n");
            }
            richTextBox1.AppendText("\n");
        }

        public void Output_pxy(int k, bool bb)
        {
            if (bb) richTextBox2.Clear();
            double s = 0;
            richTextBox3.AppendText("p(x/y):\n");
            for (int i = 0; i < N; i++)
            {
                s = 0;
                for (int j = 0; j < N; j++)
                {
                    if (bb) richTextBox2.AppendText(String.Format("{0:f17} ", pxy[k, i, j]));
                    s += pxy[k, i, j];
                }
                if (bb) richTextBox2.AppendText("\n");
                richTextBox3.AppendText("s[" + String.Format("{0:d2}", i + 1) + "] = " + Math.Round(s, round).ToString() + "\n");
            }
            richTextBox3.AppendText("\n");
            if (bb)
                for (int i = 0; i < N; i++)
                {
                    richTextBox2.Find(String.Format("{0:f17} ", pxy[k, i, i]));
                    richTextBox2.SelectionColor = Color.Red;
                }
        }

        public void Output_CI(int k)
        {
            richTextBox4.Clear();
            richTextBox4.AppendText("C без помех = " + Math.Round(C[k], round) + "\n");
            richTextBox4.AppendText("I(x,y) без помех = " + Math.Round(I[k], round) + "\n");
            richTextBox4.AppendText("I(x,y) с помехами = " + Math.Round(Is[k], round) + "\n");
        }
    }
}
