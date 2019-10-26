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
            comboBox1.SelectedIndex = 1;
            sc = comboBox1.SelectedItem.ToString();
            scv = 1;
        }
        int N = 26;
        double sC, sI, sCs, sIs, scv;
        double[] C, I, Cs, Is, taus, hx, Vy, hxy;
        double[,] px, tau, py;
        double[,,] pxy, s_pxy;
        Random rand = new Random();
        int K = 1;
        int round = 10;
        bool changed = false;
        string sc;
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
                sC += C[k];
                sI += I[k];
            }
            sC /= K;
            sI /= K;
        }

        public void Calc_CIs(int kv)
        {
            Is = new double[kv];
            Cs = new double[kv];
            sIs = 0;
            sCs = 0;
            Calc_Hxy(K, false);
            for (int k = 0; k < kv; k++)
            {
                Is[k] = Vy[k] * (hx[k] - hxy[k]);
                sIs += Is[k];
            }
            Calc_Hxy(K, true);
            for (int k = 0; k < kv; k++)
            {
                Cs[k] = Vy[k] * (Math.Log(N, 2) - hxy[k]);
                sCs += Cs[k];
            }
            sIs /= K;
            sCs /= K;
        }

        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            if (changed)
            {
                round = Convert.ToInt32(numericUpDown4.Value);
                int k = Convert.ToInt32(numericUpDown3.Value) - 1;
                Output_px(k);
                Output_tau(k);
                Output_pxy(k, false);
                Output_CI(K);
            }
        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (changed)
            {
                sc = comboBox1.SelectedItem.ToString();
                switch (comboBox1.SelectedIndex)
                {
                    case 0: scv = 1; break;
                    case 1: scv = 1000; break;
                    case 2: scv = 1000000; break;
                }
                Output_CI(K);
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

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            if (changed)
            {
                int k = Convert.ToInt32(numericUpDown3.Value) - 1;  
                Output_px(k);
                Output_tau(k);
                Output_pxy(k, true);
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

        public void Calc_Hxy(int kv, bool peres)
        {
            hxy = new double[kv];
            Calc_s_pxy(K, peres);
            for (int k = 0; k < kv; k++)
            {
                hxy[k] = 0;
                for (int i = 0; i < N; i++)
                    for (int j = 0; j < N; j++)
                        hxy[k] -= s_pxy[k, i, j] * Math.Log(pxy[k, i, j], 2);
            }
        }

        public void Calc_py(int kv, bool peres)
        {
            py = new double[kv, N];
            for (int k = 0; k < kv; k++)
            {
                for (int i = 0; i < N; i++) py[k, i] = 0;
                for (int j = 0; j < N; j++)
                {
                    for (int i = 0; i < N; i++)
                    {
                        if (peres) py[k, j] += (1.0 / N) * pxy[k, i, j];
                        else py[k, j] += px[k, i] * pxy[k, i, j];
                    }
                }
            }
        }

        public void Calc_s_pxy(int kv, bool peres)
        {
            Calc_py(K, peres);
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
            numericUpDown3.Maximum = numericUpDown2.Value;
            numericUpDown3.Value = 1;
            N = Convert.ToInt32(numericUpDown1.Value);
            K = Convert.ToInt32(numericUpDown2.Value);
            round = Convert.ToInt32(numericUpDown4.Value);
            int k = 0;
            Calc_px(K);
            Calc_tau(K);
            Output_px(k);
            Output_tau(k);
            Calc_pxy(K);
            Output_pxy(k, true);
            Calc_CI(K);
            Calc_CIs(K);
            Output_CI(K);
            changed = true;
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

        public void Output_CI(int kv)
        {
            richTextBox4.Clear();
            textBox1.Text = Math.Round(sI / scv, round).ToString() + " " + sc;
            textBox2.Text = Math.Round(sC / scv, round).ToString() + " " + sc;
            textBox3.Text = Math.Round(sIs / scv, round).ToString() + " " + sc;
            textBox4.Text = Math.Round(sCs / scv, round).ToString() + " " + sc;
            for (int k = 0; k < kv; k++)
            {
                richTextBox4.AppendText("Эксперимент " + (k + 1).ToString() + ": \n");
                richTextBox4.AppendText("Без помех: \n");
                richTextBox4.AppendText("C = " + Math.Round(C[k] / scv, round) + " " + sc + "\n");
                richTextBox4.AppendText("I(x,y) = " + Math.Round(I[k] / scv, round) + " " + sc + "\n\n");
                richTextBox4.AppendText("С помехами: \n");
                richTextBox4.AppendText("C = " + Math.Round(Cs[k] / scv, round) + " " + sc +"\n");
                richTextBox4.AppendText("I(x,y) = " + Math.Round(Is[k] / scv, round) + " " + sc + "\n----------------------------------------------------\n");
            }
        }
    }
}
