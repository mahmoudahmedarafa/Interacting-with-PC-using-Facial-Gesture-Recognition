using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PR_Final_Project
{
    public class Cross_Validation
    {
        private TextBox textBox;

        private Initializer init;

        private List<List<double>>[] training_set;
        private List<List<double>>[] testing_set;
        private double[] A;
        private int E, k;

        public Cross_Validation(int best_k, Initializer init, ref TextBox textBox)
        {
            this.textBox = textBox;
            this.init = init;
            k = best_k;

            training_set = new List<List<double>>[5];
            testing_set = new List<List<double>>[5];

            A = new double[3];
        }

        public void initSets()
        {
            for (int i = 1; i <= 4; i++)
            {
                training_set[i] = new List<List<double>>();
                testing_set[i] = new List<List<double>>();
            }
        }

        public double euclideanDistance(List<double> a, List<double> b)
        {
            double d = 0;
            for (int i = 0; i < a.Count; i++)
                d += Math.Pow(a[i] - b[i], 2);
            return Math.Sqrt(d);
        }

        public void test(List<List<double>> cur_testing_set, int actual_class)
        {
            foreach (List<double> testing_sample in cur_testing_set)
            {
                List<Tuple<double, int>> distances = new List<Tuple<double, int>>();
                for (int i = 1; i <= 4; i++)
                {
                    foreach (List<double> training_sample in training_set[i])
                    {
                        double d = euclideanDistance(testing_sample, training_sample);
                        Tuple<double, int> t = new Tuple<double, int>(d, i);
                        distances.Add(t);
                    }
                }

                distances.Sort();

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
                if (sample_class != actual_class)
                    E++;
            }
        }

        static void Swap<T>(ref T lhs, ref T rhs)
        {
            T temp;
            temp = lhs;
            lhs = rhs;
            rhs = temp;
        }

        public void trainClassifier()
        {
            for (int i = 1; i <= 4; i++)
            {
                test(testing_set[i], i);
            }

            for (int i = 1; i <= 4; i++)
                Swap(ref training_set[i], ref testing_set[i]);

            for (int i = 1; i <= 4; i++)
            {
                test(testing_set[i], i);
            }
        }

        public int generateRandomNumber()
        {
            Random r = new Random();
            return r.Next(0, 14);
        }

        void partitioTrainingSet()
        {
            int training_cnt = 0;
            bool[] vis = new bool[15];

            while (training_cnt < init.c1_training_features.Count / 2)
            {
                int sample_num = generateRandomNumber();
                if (!vis[sample_num])
                {
                    vis[sample_num] = true;
                    training_cnt++;
                    training_set[1].Add(init.c1_training_features[sample_num]);
                    training_set[2].Add(init.c2_training_features[sample_num]);
                    training_set[3].Add(init.c3_training_features[sample_num]);
                    training_set[4].Add(init.c4_training_features[sample_num]);
                }
            }

            for (int j = 0; j < init.c1_training_features.Count; j++)
            {
                if (!vis[j])
                {
                    testing_set[1].Add(init.c1_training_features[j]);
                    testing_set[2].Add(init.c2_training_features[j]);
                    testing_set[3].Add(init.c3_training_features[j]);
                    testing_set[4].Add(init.c4_training_features[j]);
                }
            }
        }

        public void start()
        {
            for (int j = 0; j < 3; j++)
                A[j] = 0;

            for (int i = 0; i < 3; i++)
            {
                initSets();
                E = 0;

                partitioTrainingSet();
                trainClassifier();
                
                A[i] = (double)E / 60;
            }

            double error = ((A[0] + A[1] + A[2]) / 3) * 100;
            double accuracy = 100 - error;
            accuracy = Math.Round(accuracy, 2);

            //Form1 form = new Form1();
            //form.modifyTextbox(accuracy.ToString(), 2);
            textBox.Text = accuracy.ToString() + "%";
        }
    }
}
