using Microsoft.Quantum.Simulation.Core;
using Microsoft.Quantum.Simulation.Simulators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSP
{
    class Program
    {
        const int NUM_RUNS = 1000;
        const int NUM_QAOA_STEPS = 5;

        static Dictionary<int, Data> Dict = new Dictionary<int, Data>();

        // Calculate the cost of Santa's journey
        // segmentCosts defines the cost of each potential segment of the journey
        // segmentUsed indicates whether the segment was part of the itinerary
        static double Cost(double[] segmentCosts, bool[] segmentUsed)
        {
            var totalCost = 0.0;
            for (var i = 0; i < segmentCosts.Length; ++i)
            {
                if (segmentUsed[i])
                {
                    totalCost += segmentCosts[i];
                }
            }
            return totalCost;
        }

        // If the proposed string of boolean values satisfies all of the constraints and therefore
        // represents a valid loop through the destinations, return true. Otherwise return false.
        static bool Satisfactory(bool[] r)
        {
            var HammingWeight = 0;
            for (int i = 0; i < 6; i++)
            {
                if (r[i]) HammingWeight++;
            }
            if (HammingWeight != 4) return false;
            if (r[0] != r[2]) return false;
            if (r[1] != r[3]) return false;
            if (r[4] != r[5]) return false;
            return true;
        }

        static void Main(string[] args)
        {
            // We start by loading the simulator that we will use to run our Q# operations.
            QuantumSimulator qsim = new QuantumSimulator();

            Console.WriteLine("QTSP v5.2\n");

            while (true)
            {
                Console.WriteLine("#11111111111#");
                Console.WriteLine("4 5       6 2 ");
                Console.WriteLine("4   5   6   2 ");
                Console.WriteLine("4     X     2 ");
                Console.WriteLine("4   6   5   2 ");
                Console.WriteLine("4 6       5 2 ");
                Console.WriteLine("#33333333333#");

                Console.WriteLine("enter 6 weights");
                string line = Console.ReadLine().Trim();
                if(line == "exit")
                {
                    break;
                }
                if(line == "default")
                {
                    RunSimulation(qsim, new double[] { 4.70, 9.09, 9.03, 5.70, 8.02, 1.71 });
                    Console.ReadKey(true);
                    Console.Clear();
                    continue;
                }

                string[] split = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                double[] weights = new double[split.Length];
                for(int i = 0; i < split.Length; i++)
                {
                    try
                    {
                        weights[i] = double.Parse(split[i]);
                        
                    }
                    catch
                    {
                        Console.WriteLine("error, invalid number");
                        continue;
                    }
                }
                RunSimulation(qsim, weights);
                Console.ReadKey(true);
                Console.Clear();
            }

            qsim.Dispose();
        }

        private static void RunSimulation(QuantumSimulator qsim, double[] segmentCosts)
        {
            if (segmentCosts.Length != 6)
            {
                Console.WriteLine("error, there have to be 6 weights");
                return;
            }
            Dict.Clear();
            Console.WriteLine("simulating...");

            // Define the costs of journey segments
            //double[] segmentCosts = { 4.70, 9.09, 9.03, 5.70, 8.02, 1.71 };
            // Define the penalty for constraint violation
            double penalty = 20;// segmentCosts.Sum()/2 + 1 ;

            // Here are some magic QAOA parameters that we got by lucky guessing.
            // Theoretically, they should yield the optimal solution in 70.6% of trials.
            double[] dtx = { 0.619193, 0.742566, 0.060035, -1.568955, 0.045490 };
            double[] dtz = { 3.182203, -1.139045, 0.221082, 0.537753, -0.417222 };

            // Convert parameters to QArray<Double> to pass them to Q#
            var tx = new QArray<double>(dtx);
            var tz = new QArray<double>(dtz);
            var costs = new QArray<double>(segmentCosts);

            Data bestRun = null;
            for (int trial = 0; trial < NUM_RUNS; trial++)
            {
                var result = QAOA_santa.Run(qsim, costs, penalty, tx, tz, NUM_QAOA_STEPS).Result;
                var tmp = result.ToArray<bool>();
                var cost = Cost(segmentCosts, tmp);
                var sat = Satisfactory(tmp);

                int key = Calc(tmp);
                if (!Dict.ContainsKey(key))
                {
                    Dict.Add(key, new Data(1, cost, (uint)key, sat));
                    if (sat && (bestRun == null || cost < bestRun.Cost))
                    {
                        bestRun = Dict[key];
                    }
                }
                else
                {
                    Dict[key].Count++;
                }
            }
            Console.WriteLine("simulation complete.");
            if(bestRun != null)
            {
                Console.WriteLine($"best cycle: {bestRun.Weg.ToBinary(6)}, cost = {bestRun.Cost}");
                Console.WriteLine($"{bestRun.Count} runs found the best result");
            }
            else
            {
                Console.WriteLine("no valid cycle was found");
            }
            PrintData();
        }

        private static void PrintData()
        {
            Console.WriteLine("-----------------");
            Console.WriteLine($"data from {NUM_RUNS} runs");

            List<Data> data = Dict.Values.ToList();
            data.Sort((data1, data2) => data2.Count.CompareTo(data1.Count));
            foreach (Data d in data)
            {
                if (d.Valid)
                    Console.ForegroundColor = ConsoleColor.Green;
                else
                    Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(d);
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        private static int Calc(bool[] array)
        {
            int val = array[0] ? 1 : 0;
            for (int i = 1; i < array.Length; i++)
            {
                val <<= 1;
                val |= array[i] ? 1 : 0;
            }
            return val;
        }

        class Data
        {
            public Data(int count, double cost, uint weg, bool valid)
            {
                Count = count;
                Cost = cost;
                Weg = weg;
                Valid = valid;
            }

            public int Count { get; set; }
            public double Cost { get; set; }
            public uint Weg { get; set; }
            public bool Valid { get; set; }

            public override string ToString()
            {
                return String.Format($"{Weg.ToBinary(6)}: {Count} \t cost: {Cost}");
            }
        }
    }

    static class ExtFunc
    {
        public static string ToBinary(this uint number, int bitsLength = 32)
        {
            return NumberToBinary(number, bitsLength);
        }

        public static string NumberToBinary(uint number, int bitsLength = 32)
        {
            string result = Convert.ToString(number, 2).PadLeft(bitsLength, '0');

            return result;
        }
    }
}
