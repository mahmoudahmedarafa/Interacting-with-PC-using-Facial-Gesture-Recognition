using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace PR_Final_Project
{
    public class Bayesian
    {
        private DataGridView dataGridView;
        private TextBox textBox;

        private Initializer init;
        private int[,] confusion_matrix;

        private Matrix<double>[] meu, covariance, inverse;
        private double[] determinant;

        private double[] likelihood, prior, posterior;

        private string[] gesture;

        private Process process;

        public Bayesian(ref DataGridView dataGridView, ref TextBox textBox)
        {
            this.dataGridView = dataGridView;
            this.textBox = textBox;
            init = new Initializer();
            confusion_matrix = new int[6, 6];

            meu = new Matrix<double>[5];
            covariance = new Matrix<double>[5];
            inverse = new Matrix<double>[5];

            for (int i = 1; i <= 4; i++)
            {
                meu[i] = Matrix<double>.Build.Dense(12, 1);
                covariance[i] = Matrix<double>.Build.Dense(12, 12);
                inverse[i] = Matrix<double>.Build.Dense(12, 12);
            }

            determinant = new double[5];

            prior = new double[5];
            likelihood = new double[5];
            posterior = new double[5];

            gesture = new string[5];

            process = new Process();
        }

        public void calculatePrior()
        {
            prior[1] = (double)init.c1_testing_features.Count / 60;
            prior[2] = (double)init.c2_testing_features.Count / 60;
            prior[3] = (double)init.c3_testing_features.Count / 60;
            prior[4] = (double)init.c4_testing_features.Count / 60;
        }

        public double calculateLikelihood(List <double> sample, int class_num)
        {
            double determinator = 2 * Math.PI;
            determinator = Math.Pow(determinator, 6);   //6 = half number of features
            determinator *= Math.Pow(determinant[class_num], 0.5);
            double scalar1 = 1 / determinator;

            double exp_pow = -0.5;

            Matrix<double> x_minus_meu_transpose = Matrix<double>.Build.Random(1, 12);
            Matrix<double> x_minus_meu = Matrix<double>.Build.Random(12, 1);

            for (int i = 0; i < 12; i++)
            {
                x_minus_meu_transpose[0, i] = sample[i] - meu[class_num][i, 0];//meu[i, 0];
                x_minus_meu[i, 0] = sample[i] - meu[class_num][i, 0];
            }

            x_minus_meu_transpose.Multiply(inverse[class_num]);
            x_minus_meu_transpose.Multiply(x_minus_meu);

            exp_pow *= x_minus_meu_transpose[0, 0];
            double scalar2 = Math.Exp(exp_pow);

            return scalar1 * scalar2;
        }

        public void calculateDeterminants()
        {
            for (int i = 1; i <= 4; i++)
                determinant[i] = covariance[i].Determinant();
        }

        public void calculateInverses()
        {
            for (int i = 1; i <= 4; i++)
                inverse[i] = covariance[i].Inverse();
        }

        public void calculateCovarianceMatrices(List <double> sample)
        {
            for (int class_num = 1; class_num <= 4; class_num++)
            {
                for (int i = 0; i < 12; i++)
                {
                    for (int j = 0; j < 12; j++)
                    {
                        double sum = 0;
                        //Note: number of training samples in all classes is the same
                        for (int k = 0; k < init.c1_training_features.Count; k++)
                            sum += (sample[i] - meu[class_num][i, 0]) * (sample[j] - meu[class_num][j, 0]);
                        sum /= init.c1_training_features.Count;
                        covariance[class_num][i, j] = sum;
                    }
                }
            }
        }

        public void calculateMeus()
        {
            for (int i = 0; i < 12; i++)
            {
                for (int j = 0; j < init.c1_training_features.Count; j++)
                    meu[1][i, 0] += init.c1_training_features[j][i];
                meu[1][i, 0] /= init.c1_training_features.Count;

                for (int j = 0; j < init.c2_training_features.Count; j++)
                    meu[2][i, 0] += init.c2_training_features[j][i];
                meu[2][i, 0] /= init.c2_training_features.Count;

                for (int j = 0; j < init.c3_training_features.Count; j++)
                    meu[3][i, 0] += init.c3_training_features[j][i];
                meu[3][i, 0] /= init.c3_training_features.Count;

                for (int j = 0; j < init.c4_training_features.Count; j++)
                    meu[4][i, 0] += init.c4_training_features[j][i];
                meu[4][i, 0] /= init.c4_training_features.Count;
            }
        }

        public void displayConfusionMatrix()
        {
            //Form1 form = new Form1();
            dataGridView.Rows.Clear();
            for (int i = 1; i < 6; i++)
            {
                var row = new DataGridViewRow();
                for (int j = 1; j < 6; j++)
                {
                    row.Cells.Add(new DataGridViewTextBoxCell()
                    {
                        Value = confusion_matrix[i, j]
                    });
                }
                dataGridView.Rows.Add(row);
            }
        }

        public void initConfusionMatrix()
        {
            for (int i = 0; i < 6; i++)
                for (int j = 0; j < 6; j++)
                    confusion_matrix[i, j] = 0;
        }

        public void prepareConfusionMatrix()
        {
            for (int i = 1; i < 6; i++)
            {
                for (int j = 1; j < 5; j++)
                {
                    confusion_matrix[i, 5] += confusion_matrix[i, j];
                }
            }

            for (int i = 1; i < 6; i++)
            {
                for (int j = 1; j < 5; j++)
                {
                    confusion_matrix[5, i] += confusion_matrix[j, i];
                }
            }
        }

        public double calculateOverallAccuracy()
        {
            int sumDiagonal = 0;
            for (int i = 1; i < 4; i++)
            {
                sumDiagonal += confusion_matrix[i, i];
            }

            int sumTesting = 20;
            double overallAccuracy = (double)sumDiagonal / sumTesting;
            overallAccuracy *= 100;
            overallAccuracy = Math.Round(overallAccuracy, 2);
            //label6.Text = overallAccuracy.ToString() + "%";
            //Form1 form = new Form1();
            //form.modifyTextbox(overallAccuracy.ToString() + "%", 1);
            textBox.Text = overallAccuracy.ToString() + "%";
            return overallAccuracy;
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
            gesture[2] = "Looking Down";
            gesture[3] = "Looking Front";
            gesture[4] = "Looking Left";

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

        public int classifySample(List<double> sample)
        {
            double evidence = 0, max_posterior = 0;
            int sample_class = 0;

            for (int i = 1; i <= 4; i++)
            {
                likelihood[i] = calculateLikelihood(sample, i);
                evidence += likelihood[i] * prior[i];
            }

            for (int i = 1; i <= 4; i++)
            {
                posterior[i] = (likelihood[i] * prior[i]) / evidence;
                if (posterior[i] > max_posterior)
                {
                    max_posterior = posterior[i];
                    sample_class = i;
                }
            }

            performAction(sample_class);

            return sample_class;
        }

        public void test(List<List<double>> testing_set, int class_num)
        {
            foreach (List<double> sample in testing_set)
            {
                double evidence = 0, max_posterior = 0;
                int sample_class = 0;

                calculateCovarianceMatrices(sample);
                calculateDeterminants();
                calculateInverses();

                for (int i = 1; i <= 4; i++)
                {
                    likelihood[i] = calculateLikelihood(sample, i);
                    evidence += likelihood[i] * prior[i];
                }

                for (int i = 1; i <= 4; i++)
                {
                    posterior[i] = (likelihood[i] * prior[i]) / evidence;
                    if (posterior[i] > max_posterior)
                    {
                        max_posterior = posterior[i];
                        sample_class = i;
                    }
                }

                confusion_matrix[class_num, sample_class]++;
            }
        }

        public void trainClassifier()
        {
            init.readTrainingDataFromFiles();
            init.readTestingDataFromFiles();
            init.computeTestingDataFeatures();
            init.computeTrainingDataFeatures();

            initConfusionMatrix();

            //Constants .. There is no need to re-calculate them with each testing sample
            calculateMeus();
            calculatePrior();

            test(init.c1_testing_features, 1);
            test(init.c2_testing_features, 2);
            test(init.c3_testing_features, 3);
            test(init.c4_testing_features, 4);

            prepareConfusionMatrix();

            double overall_accuracy = calculateOverallAccuracy();

            displayConfusionMatrix();
            MessageBox.Show("Bayesian Classifier Done!");
        }


    }
}
