using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PR_Final_Project
{
    public partial class Form1 : Form
    {
        private int _selectedIndex, _selectedIndex2;

        private Bayesian Bayesian_classifier;
        private KNN KNN_Classifier;

        private List<Tuple<double, double>> sample_positions;
        private List<double> sample_features;

        public Form1()
        {
            InitializeComponent();
        }

        public void addRowToDataGridView(DataGridViewRow row)
        {
            dataGridView1.Rows.Add(row);
        }

        public void clearDataGridView()
        {
            dataGridView1.Rows.Clear();
        }

        public void modifyTextbox(string text, int text_box_number)
        {
            if (text_box_number == 1)
                textBox1.Text = text;
            else if (text_box_number == 2)
                textBox2.Text = text;
            else if (text_box_number == 3)
                textBox3.Text = text;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            _selectedIndex = comboBox1.SelectedIndex;

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.Items.Add("Bayesian Classifier");
            comboBox1.Items.Add("K-Nearest Neighbor Classifier");

            comboBox2.Items.Add("Bayesian Classifier");
            comboBox2.Items.Add("K-Nearest Neighbor Classifier");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (_selectedIndex == 0)
            {
                Bayesian_classifier = new Bayesian(ref dataGridView1, ref textBox1);
                Bayesian_classifier.trainClassifier();
                //List<double> sample = new List<double>();
                //int result = classifier.classifySample(sample, best_k);
            }
            else
            {
                KNN_Classifier = new KNN(ref dataGridView1, ref textBox1, ref textBox3);
                //List<double> sample = new List<double>();
                KNN_Classifier.trainClassifier();
                //int result = classifier.classifySample(sample, best_k);
                Cross_Validation validator = new Cross_Validation(KNN_Classifier.best_k, KNN_Classifier.init, ref textBox2);
                validator.start();
            }
        }

        public bool isBadFeature(int feature_num)
        {
            //1 8 10 12 13 15 16
            if (feature_num >= 16)
                feature_num--;
            int[] arr = { 1, 8, 10, 12, 13, 15, 16 };
            for (int i = 0; i < arr.Length; i++)
                if (feature_num == arr[i])
                    return true;
            return false;
        }

        public double euclideanDistance(Tuple<double, double> t1, Tuple<double, double> t2)
        {
            return Math.Sqrt(Math.Pow(t1.Item1 - t2.Item1, 2) + Math.Pow(t1.Item2 - t2.Item2, 2));
        }

        public void parseTestFile()
        {
            sample_positions = new List<Tuple<double, double>>();
            sample_features = new List<double>();
            
            OpenFileDialog choofdlog = new OpenFileDialog();
            choofdlog.Filter = "All Files (*.*)|*.*";
            choofdlog.FilterIndex = 1;
            choofdlog.Multiselect = true;

            if (choofdlog.ShowDialog() == DialogResult.OK)
            {
                string sFileName = choofdlog.FileName;
                string[] contents = System.IO.File.ReadAllLines(sFileName);
                for (int i = 3; i < contents.Length - 1; i++)
                {
                    string[] tmp = contents[i].Split(' ');
                    Tuple<double, double> position = new Tuple<double, double>(double.Parse(tmp[0]), double.Parse(tmp[1]));
                    sample_positions.Add(position);
                }
            }

            for (int j = 0; j < 20; j++)
            {
                if (j == 14 || isBadFeature(j + 1))
                    continue;
                //double feature = euclideanDistance(c1_training_positions[i][j], c1_training_positions[i][14]);
                double feature = euclideanDistance(sample_positions[j], sample_positions[14]);
                sample_features.Add(feature);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            parseTestFile();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (_selectedIndex2 == 0)
            {
                if (_selectedIndex == 1)
                {
                    Bayesian_classifier = new Bayesian(ref dataGridView1, ref textBox1);
                    Bayesian_classifier.trainClassifier();
                }
                Bayesian_classifier.classifySample(sample_features);
            }
            else
            {
                if (_selectedIndex == 0)
                {
                    KNN_Classifier = new KNN(ref dataGridView1, ref textBox1, ref textBox3);
                    //List<double> sample = new List<double>();
                    KNN_Classifier.trainClassifier();
                    //int result = classifier.classifySample(sample, best_k);
                    Cross_Validation validator = new Cross_Validation(KNN_Classifier.best_k, KNN_Classifier.init, ref textBox2);
                    validator.start();
                }
                KNN_Classifier.classifySample(sample_features, KNN_Classifier.best_k);
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            _selectedIndex2 = comboBox2.SelectedIndex;
        }
    }
}
