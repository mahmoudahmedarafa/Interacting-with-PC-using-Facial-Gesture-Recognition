using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PR_Final_Project
{
    public class KNN
    {
        private DataGridView dataGridView;
        private TextBox textBox1, textBox3;

        public Initializer init;
        private int[,] confusionMatrix, best_confusion_matrix;
        public int best_k;

        private string[] gesture;

        private Process process;

        public KNN(ref DataGridView dataGridView, ref TextBox textBox1, ref TextBox textBox3)
        {
            this.dataGridView = dataGridView;
            this.textBox1 = textBox1;
            this.textBox3 = textBox3;
            init = new Initializer();
            confusionMatrix = new int[6, 6];
            best_confusion_matrix = new int[6, 6];
            gesture = new string[5];
            best_k = 0;
            process = new Process();
        }

        public void displayBestConfusionMatrix()
        {
            //Form1 form = new Form1();
            //form.clearDataGridView();
            //for (int i = 1; i < 6; i++)
            //{
            //    var row = new DataGridViewRow();
            //    for (int j = 1; j < 6; j++)
            //    {
            //        row.Cells.Add(new DataGridViewTextBoxCell()
            //        {
            //            Value = best_confusion_matrix[i, j]
            //        });
            //    }
            //    form.addRowToDataGridView(row);
            //}
            dataGridView.Rows.Clear();
            for (int i = 1; i < 6; i++)
            {
                var row = new DataGridViewRow();
                for (int j = 1; j < 6; j++)
                {
                    row.Cells.Add(new DataGridViewTextBoxCell()
                    {
                        Value = best_confusion_matrix[i, j]
                    });
                }
                dataGridView.Rows.Add(row);
            }
        }

        public void assignBestConfusionMatrix()
        {
            for (int i = 0; i < 6; i++)
                for (int j = 0; j < 6; j++)
                    best_confusion_matrix[i, j] = confusionMatrix[i, j];
        }

        public void initConfusionMatrix()
        {
            for (int i = 0; i < 6; i++)
                for (int j = 0; j < 6; j++)
                    confusionMatrix[i, j] = 0;
        }

        public double euclideanDistance(List<double> a, List<double> b)
        {
            double d = 0;
            for (int i = 0; i < a.Count; i++)
                d += Math.Pow(a[i] - b[i], 2);
            return Math.Sqrt(d);
        }

        public void prepareConfusionMatrix()
        {
            for (int i = 1; i < 6; i++)
            {
                for (int j = 1; j < 5; j++)
                {
                    confusionMatrix[i, 5] += confusionMatrix[i, j];
                }
            }

            for (int i = 1; i < 6; i++)
            {
                for (int j = 1; j < 5; j++)
                {
                    confusionMatrix[5, i] += confusionMatrix[j, i];
                }
            }
            //Form1 form = new Form1();
            //form.clearDataGridView();
            //for (int i = 1; i < 5; i++)
            //{
            //    var row = new DataGridViewRow();
            //    for (int j = 1; j < 5; j++)
            //    {
            //        row.Cells.Add(new DataGridViewTextBoxCell()
            //        {
            //            Value = confusionMatrix[i, j]
            //        });
            //    }
            //    form.addRowToDataGridView(row);
            //}
        }

        public double calculateOverallAccuracy()
        {
            int sumDiagonal = 0;
            for (int i = 1; i < 4; i++)
            {
                sumDiagonal += confusionMatrix[i, i];
            }

            int sumTesting = 20;
            double overallAccuracy = (double)sumDiagonal / sumTesting;
            overallAccuracy *= 100;
            overallAccuracy = Math.Round(overallAccuracy, 2);
            //label6.Text = overallAccuracy.ToString() + "%";
            //textBox1.Text = overallAccuracy.ToString() + "%";
            return overallAccuracy;
        }

        public void test(List<List<double>> testing_set, int class_num, int k)
        {
            foreach (List<double> testing_sample in testing_set)
            {
                List<Tuple<double, int>> distances = new List<Tuple<double, int>>();
                foreach (List<double> training_sample in init.c1_training_features)
                {
                    double d = euclideanDistance(testing_sample, training_sample);
                    Tuple<double, int> t = new Tuple<double, int>(d, 1);
                    distances.Add(t);
                }

                foreach (List<double> training_sample in init.c2_training_features)
                {
                    double d = euclideanDistance(testing_sample, training_sample);
                    Tuple<double, int> t = new Tuple<double, int>(d, 2);
                    distances.Add(t);
                }

                foreach (List<double> training_sample in init.c3_training_features)
                {
                    double d = euclideanDistance(testing_sample, training_sample);
                    Tuple<double, int> t = new Tuple<double, int>(d, 3);
                    distances.Add(t);
                }

                foreach (List<double> training_sample in init.c4_training_features)
                {
                    double d = euclideanDistance(testing_sample, training_sample);
                    Tuple<double, int> t = new Tuple<double, int>(d, 4);
                    distances.Add(t);
                }

                distances.Sort();

                //for (int k = 1; k <= 60; k++)
                //{
                int[] ki = new int[5];
                for (int i = 0; i < k; i++)
                    ki[distances[i].Item2]++;

                double[] posterior = new double[5];
                double max_posterior = -1;
                int sample_class = 0;
                for (int i = 1; i <= 4; i++)
                {
                    posterior[i] = (double)ki[i] / k;
                    if (posterior[i] > max_posterior)
                    {
                        max_posterior = posterior[i];
                        sample_class = i;
                    }
                }
                confusionMatrix[class_num, sample_class]++;
                //}
            }
        }

        [DllImport("user32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        public void performAction(int sample_class)
        {
            const int SW_SHOWMINIMIZED = 2;
            const int SW_RESTORE = 9;

            gesture[1] = "Closing Eyes";
            gesture[2] = "LookingDown";
            gesture[3] = "LookingFront";
            gesture[4] = "LookingLeft";

            if (sample_class == 1)
            {
                //close firefox
                process.Kill();
            }
            else if (sample_class == 2)
            {
                //minimize firefox
                IntPtr hWnd = FindWindow("Notepad", "Untitled - Notepad");
                if (!hWnd.Equals(IntPtr.Zero))
                    ShowWindowAsync(hWnd, SW_SHOWMINIMIZED);
            }
            else if (sample_class == 3)
            {
                //open firefox
                process = Process.Start("Notepad");
            }
            else
            {
                //restore firefox
                var processes = Process.GetProcessesByName("Notepad.exe");

                IntPtr hWnd = FindWindow("Notepad", "Untitled - Notepad");
                if (!hWnd.Equals(IntPtr.Zero))
                    ShowWindowAsync(hWnd, SW_RESTORE);
            }

            MessageBox.Show(("Human is " + gesture[sample_class]) + ".");
        }

        public int classifySample(List<double> sample, int best_k)
        {
            List<Tuple<double, int>> distances = new List<Tuple<double, int>>();
            foreach (List<double> training_sample in init.c1_training_features)
            {
                double d = euclideanDistance(sample, training_sample);
                Tuple<double, int> t = new Tuple<double, int>(d, 1);
                distances.Add(t);
            }

            foreach (List<double> training_sample in init.c2_training_features)
            {
                double d = euclideanDistance(sample, training_sample);
                Tuple<double, int> t = new Tuple<double, int>(d, 2);
                distances.Add(t);
            }

            foreach (List<double> training_sample in init.c3_training_features)
            {
                double d = euclideanDistance(sample, training_sample);
                Tuple<double, int> t = new Tuple<double, int>(d, 3);
                distances.Add(t);
            }

            foreach (List<double> training_sample in init.c4_training_features)
            {
                double d = euclideanDistance(sample, training_sample);
                Tuple<double, int> t = new Tuple<double, int>(d, 4);
                distances.Add(t);
            }

            distances.Sort();

            //for (int k = 1; k <= 60; k++)
            //{
            int[] ki = new int[5];
            for (int i = 0; i < best_k; i++)
                ki[distances[i].Item2]++;

            double[] posterior = new double[5];
            double max_posterior = -1;
            int sample_class = 0;
            for (int i = 1; i <= 4; i++)
            {
                posterior[i] = (double)ki[i] / best_k;
                if (posterior[i] > max_posterior)
                {
                    max_posterior = posterior[i];
                    sample_class = i;
                }
            }

            performAction(sample_class);

            return sample_class;
        }

        public void trainClassifier()
        {
            //init = new Initializer();
            init.readTrainingDataFromFiles();
            init.readTestingDataFromFiles();
            init.computeTestingDataFeatures();
            init.computeTrainingDataFeatures();

            //distances.AddRange(test(init.c1_testing_features));
            //distances.AddRange(test(init.c2_testing_features));
            //distances.AddRange(test(init.c3_testing_features));
            //distances.AddRange(test(init.c4_testing_features));
            double max_overall_accuracy = 0;

            //Template
            //int x = 7;

            for (int k = 1; k <= 60; k++)
            {
                initConfusionMatrix();

                test(init.c1_testing_features, 1, k);
                test(init.c2_testing_features, 2, k);
                test(init.c3_testing_features, 3, k);
                test(init.c4_testing_features, 4, k);

                prepareConfusionMatrix();

                double cur_overall_accuracy = calculateOverallAccuracy();
                if (cur_overall_accuracy > max_overall_accuracy)
                {
                    max_overall_accuracy = cur_overall_accuracy;
                    best_k = k;
                    assignBestConfusionMatrix();
                }
            }

            //Template
            //best_k = x;

            //Form1 form = new Form1();
            //form.modifyTextbox(best_k.ToString(), 3);
            textBox1.Text = max_overall_accuracy.ToString() + "%";
            textBox3.Text = best_k.ToString();
            displayBestConfusionMatrix();
        }
    }
}
