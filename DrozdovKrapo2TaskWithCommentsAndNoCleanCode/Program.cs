using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GeneticSharp.Domain;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Crossovers;
using GeneticSharp.Domain.Fitnesses;
using GeneticSharp.Domain.Mutations;
using GeneticSharp.Domain.Populations;
using GeneticSharp.Domain.Selections;
using GeneticSharp.Domain.Terminations;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.WindowsForms;
using static System.Net.Mime.MediaTypeNames;

namespace DrozdovKrapo2TaskWithCommentsAndNoCleanCode
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            // Запуск генетических алгоритмов с различными селекциями
            var selectionResults = new Dictionary<string, int>
            {
                { "EliteSelection", RunGeneticAlgorithm(new EliteSelection(), new UniformCrossover(0.5f), new FlipBitMutation()) },
                { "RouletteWheelSelection", RunGeneticAlgorithm(new RouletteWheelSelection(), new UniformCrossover(0.5f), new FlipBitMutation()) },
                { "TournamentSelection", RunGeneticAlgorithm(new TournamentSelection(), new UniformCrossover(0.5f), new FlipBitMutation()) }
            };

            // Запуск генетических алгоритмов с различными мутациями
            var mutationResults = new Dictionary<string, int>
            {
                { "FlipBitMutation", RunGeneticAlgorithm(new EliteSelection(), new UniformCrossover(0.5f), new FlipBitMutation()) },
                { "InsertionMutation", RunGeneticAlgorithm(new EliteSelection(), new UniformCrossover(0.5f), new InsertionMutation()) },
                { "PartialShuffleMutation", RunGeneticAlgorithm(new EliteSelection(), new UniformCrossover(0.5f), new PartialShuffleMutation()) }
            };

            // Запуск генетических алгоритмов с различными кроссоверами
            var crossoverResults = new Dictionary<string, int>
            {
                { "TwoPointCrossover", RunGeneticAlgorithm(new EliteSelection(), new TwoPointCrossover(), new FlipBitMutation()) },
                { "UniformCrossover", RunGeneticAlgorithm(new EliteSelection(), new UniformCrossover(0.5f), new FlipBitMutation()) }
            };

            // Создание формы и вкладок для графиков
            var form = new Form
            {
                Text = "Графики и диаграммы",
                Width = 800,
                Height = 600
            };

            var tabControl = new TabControl
            {
                Dock = DockStyle.Fill
            };

            form.Controls.Add(tabControl);

            // Вкладка для графика максимального фитнеса
            var maxFitnessTab = new TabPage("Максимальный фитнес");
            var maxFitnessPlotView = new PlotView
            {
                Dock = DockStyle.Fill
            };
            maxFitnessTab.Controls.Add(maxFitnessPlotView);
            tabControl.TabPages.Add(maxFitnessTab);

            // Вкладка для графика среднего фитнеса
            var avgFitnessTab = new TabPage("Средний фитнес");
            var avgFitnessPlotView = new PlotView
            {
                Dock = DockStyle.Fill
            };
            avgFitnessTab.Controls.Add(avgFitnessPlotView);
            tabControl.TabPages.Add(avgFitnessTab);

            // Вкладка для графика минимального фитнеса
            var minFitnessTab = new TabPage("Минимальный фитнес");
            var minFitnessPlotView = new PlotView
            {
                Dock = DockStyle.Fill
            };
            minFitnessTab.Controls.Add(minFitnessPlotView);
            tabControl.TabPages.Add(minFitnessTab);

            // Вкладка для диаграммы селекции
            var selectionDiagramTab = new TabPage("Диаграмма селекции");
            var selectionDiagramPlotView = new PlotView
            {
                Dock = DockStyle.Fill
            };
            selectionDiagramTab.Controls.Add(selectionDiagramPlotView);
            tabControl.TabPages.Add(selectionDiagramTab);

            // Вкладка для диаграммы мутации
            var mutationDiagramTab = new TabPage("Диаграмма мутации");
            var mutationDiagramPlotView = new PlotView
            {
                Dock = DockStyle.Fill
            };
            mutationDiagramTab.Controls.Add(mutationDiagramPlotView);
            tabControl.TabPages.Add(mutationDiagramTab);

            // Вкладка для диаграммы кроссоверов
            var crossoverDiagramTab = new TabPage("Диаграмма кроссоверов");
            var crossoverDiagramPlotView = new PlotView
            {
                Dock = DockStyle.Fill
            };
            crossoverDiagramTab.Controls.Add(crossoverDiagramPlotView);
            tabControl.TabPages.Add(crossoverDiagramTab);

            // Отрисовка графиков фитнеса
            var (maxFitnessHistory, avgFitnessHistory, minFitnessHistory) = RunGeneticAlgorithmWithFitnessTracking(new EliteSelection(), new FlipBitMutation());
            DrawFitnessGraphs(maxFitnessHistory, avgFitnessHistory, minFitnessHistory, maxFitnessPlotView, avgFitnessPlotView, minFitnessPlotView);
            DrawSelectionResults(selectionResults, selectionDiagramPlotView);
            DrawMutationResults(mutationResults, mutationDiagramPlotView);
            DrawCrossoverResults(crossoverResults, crossoverDiagramPlotView);

            System.Windows.Forms.Application.Run(form);
        }

        private static (List<double> maxFitnessHistory, List<double> avgFitnessHistory, List<double> minFitnessHistory) RunGeneticAlgorithmWithFitnessTracking(ISelection selection, IMutation mutation)
        {
            // Создание хромосомы для двух переменных x и y
            var chromosome = new FloatingPointChromosome(
                new double[] { -4, -4 }, // Минимум для x и y
                new double[] { 4, 4 },   // Максимум для x и y
                new int[] { 64, 64 },    // Количество бит для кодирования x и y
                new int[] { 3, 3 });     // Количество дробных бит для x и y

            // Создание популяции
            var population = new Population(100, 100, chromosome);

            // Определение функции пригодности
            var fitness = new FuncFitness((c) =>
            {
                var fc = c as FloatingPointChromosome;
                var values = fc.ToFloatingPoints();
                var x = values[0];
                var y = values[1];
                return -((Math.Pow(x, 2) + Math.Pow(y, 2)) / (Math.Sin(x) + Math.Cos(x)));  // Отрицательное для минимизации
            });

            // Определение кроссовера
            var crossover = new UniformCrossover(0.5f);

            // Определение условия завершения
            var termination = new FitnessStagnationTermination(100);

            // Создание генетического алгоритма
            var ga = new GeneticAlgorithm(
                population,
                fitness,
                selection,
                crossover,
                mutation);

            // Установка условия завершения
            ga.Termination = termination;

            // Списки для хранения истории фитнеса
            var maxFitnessHistory = new List<double>();
            var avgFitnessHistory = new List<double>();
            var minFitnessHistory = new List<double>();

            ga.GenerationRan += (sender, e) =>
            {
                var bestChromosome = ga.BestChromosome as FloatingPointChromosome;
                maxFitnessHistory.Add(bestChromosome.Fitness.Value);

                double avgFitness = population.CurrentGeneration.Chromosomes.Average(c => c.Fitness.Value);
                avgFitnessHistory.Add(avgFitness);

                double minFitness = population.CurrentGeneration.Chromosomes.Min(c => c.Fitness.Value);
                minFitnessHistory.Add(minFitness);

                // Вывод значений фитнеса в консоль
                Console.WriteLine($"Поколение: {ga.GenerationsNumber}, Максимальный фитнес: {bestChromosome.Fitness.Value}, Средний фитнес: {avgFitness}, Минимальный фитнес: {minFitness}");
            };

            // Запуск алгоритма
            ga.Start();

            // Вывод результата в консоль после завершения
            Console.WriteLine($"Селектор: {selection.GetType().Name}, Количество поколений: {ga.GenerationsNumber}");

            return (maxFitnessHistory, avgFitnessHistory, minFitnessHistory);
        }

        private static void DrawFitnessGraphs(List<double> maxFitnessHistory, List<double> avgFitnessHistory, List<double> minFitnessHistory,
                                               PlotView maxFitnessPlotView, PlotView avgFitnessPlotView, PlotView minFitnessPlotView)
        {
            var maxPlotModel = new PlotModel { Title = "Изменение максимального фитнеса по поколениям" };
            var maxLineSeries = new LineSeries { Title = "Максимальный фитнес" };
            for (int i = 0; i < maxFitnessHistory.Count; i++)
            {
                maxLineSeries.Points.Add(new DataPoint(i, -maxFitnessHistory[i]));
            }
            maxPlotModel.Series.Add(maxLineSeries);
            maxFitnessPlotView.Model = maxPlotModel;

            var avgPlotModel = new PlotModel { Title = "Изменение среднего фитнеса по поколениям" };
            var avgLineSeries = new LineSeries { Title = "Средний фитнес" };
            for (int i = 0; i < avgFitnessHistory.Count; i++)
            {
                avgLineSeries.Points.Add(new DataPoint(i, -avgFitnessHistory[i]));
            }
            avgPlotModel.Series.Add(avgLineSeries);
            avgFitnessPlotView.Model = avgPlotModel;

            var minPlotModel = new PlotModel { Title = "Изменение минимального фитнеса по поколениям" };
            var minLineSeries = new LineSeries { Title = "Минимальный фитнес" };
            for (int i = 0; i < minFitnessHistory.Count; i++)
            {
                minLineSeries.Points.Add(new DataPoint(i, -minFitnessHistory[i]));
            }
            minPlotModel.Series.Add(minLineSeries);
            minFitnessPlotView.Model = minPlotModel;
        }

        private static void DrawSelectionResults(Dictionary<string, int> selectionResults, PlotView plotView)
        {
            var plotModel = new PlotModel { Title = "Влияние селекции на количество поколений" };
            var barSeries = new BarSeries { Title = "Количество поколений" };
            foreach (var result in selectionResults)
            {
                barSeries.Items.Add(new BarItem(result.Value));
                Console.WriteLine($"Селектор: {result.Key}, Количество поколений: {result.Value}"); // Вывод результатов селекции в консоль
            }
            plotModel.Series.Add(barSeries);
            plotView.Model = plotModel;
        }

        private static void DrawMutationResults(Dictionary<string, int> mutationResults, PlotView plotView)
        {
            var plotModel = new PlotModel { Title = "Влияние мутации на количество поколений" };
            var barSeries = new BarSeries { Title = "Количество поколений" };
            foreach (var result in mutationResults)
            {
                barSeries.Items.Add(new BarItem(result.Value));
                Console.WriteLine($"Мутация: {result.Key}, Количество поколений: {result.Value}"); // Вывод результатов мутации в консоль
            }
            plotModel.Series.Add(barSeries);
            plotView.Model = plotModel;
        }

        private static void DrawCrossoverResults(Dictionary<string, int> crossoverResults, PlotView plotView)
        {
            var plotModel = new PlotModel { Title = "Влияние кроссоверов на количество поколений" };
            var barSeries = new BarSeries { Title = "Количество поколений" };
            foreach (var result in crossoverResults)
            {
                barSeries.Items.Add(new BarItem(result.Value));
                Console.WriteLine($"Кроссовер: {result.Key}, Количество поколений: {result.Value}"); // Вывод результатов кроссоверов в консоль
            }
            plotModel.Series.Add(barSeries);
            plotView.Model = plotModel;
        }

        private static int RunGeneticAlgorithm(ISelection selection, ICrossover crossover, IMutation mutation)
        {
            // Создание хромосомы для двух переменных x и y
            var chromosome = new FloatingPointChromosome(
                new double[] { -4, -4 }, // Минимум для x и y
                new double[] { 4, 4 },   // Максимум для x и y
                new int[] { 64, 64 },    // Количество бит для кодирования x и y
                new int[] { 3, 3 });     // Количество дробных бит для x и y

            // Создание популяции
            var population = new Population(100, 100, chromosome);

            // Определение функции пригодности
            var fitness = new FuncFitness((c) =>
            {
                var fc = c as FloatingPointChromosome;
                var values = fc.ToFloatingPoints();
                var x = values[0];
                var y = values[1];
                return -((Math.Pow(x, 2) + Math.Pow(y, 2)) / (Math.Sin(x) + Math.Cos(x)));  // Отрицательное для минимизации
            });

            // Определение условия завершения
            var termination = new FitnessStagnationTermination(100);

            // Создание генетического алгоритма
            var ga = new GeneticAlgorithm(
                population,
                fitness,
                selection,
                crossover,
                mutation); // Передаем кроссовер и мутацию как параметры

            // Установка условия завершения
            ga.Termination = termination;

            // Запуск алгоритма
            ga.Start();

            // Вывод результата в консоль после завершения
            Console.WriteLine($"Селектор: {selection.GetType().Name}, Кроссовер: {crossover.GetType().Name}, Количество поколений: {ga.GenerationsNumber}");

            // Возвращаем количество поколений
            return ga.GenerationsNumber;
        }
    }
}
