using Google.OrTools.LinearSolver;

namespace ShihtaOptimization
{
    // Класс для представления шихты
    public class Shihta
    {
        public string Name { get; set; }
        public string Category { get; set; }
        public double Cost { get; set; }
        public double Plasticity { get; set; }
        public double Ash { get; set; }

        public Shihta(string name, string category, double cost, double plasticity, double ash)
        {
            Name = name;
            Category = category;
            Cost = cost;
            Plasticity = plasticity;
            Ash = ash;
        }
    }

    // Класс для представления категории
    public class Category
    {
        public string Name { get; set; }
        public List<Shihta> Shihtas { get; set; }

        public Category(string name)
        {
            Name = name;
            Shihtas = new List<Shihta>();
        }
    }

    // Класс для хранения выбранной комбинации
    public class SelectedCombination
    {
        public List<string> CombinationType { get; set; }
        public List<Shihta> Shihtas { get; set; }
        public List<double> Weights { get; set; }
        public double AverageAsh { get; set; }
        public double AveragePlasticity { get; set; }
        public double TotalCost { get; set; }
    }

    class WorkingSimplex
    {
        public static void Solve()
        {
            // Инициализация категорий и шихт
            var categories = InitializeCategories();

            // Определение типов сочетаний
            var combinationTypes = new List<List<string>>
            {
                new() { "К", "Г" },
                new() { "К", "ОС" },
                new() { "К", "СС" },
                new() { "К", "ОС", "СС" },
                new() { "К", "Г", "ОС", "СС" }
            };

            // Определение заданных весовых комбинаций
            var weightCombinations = new List<List<double>>
            {
                new() { 0.5, 0.5 },      // 0.5 + 0.5
                new() { 0.2, 0.8 },      // 0.2 + 0.8
                new() { 0.1, 0.9 }       // 0.1 + 0.9
            };

            // Перебор всех весовых комбинаций и поиск оптимальных решений
            var optimalSolutions = new List<SelectedCombination?>();

            foreach (var weights in weightCombinations)
            {
                // Для каждой весовой комбинации найдём оптимальное решение
                SelectedCombination? bestCombination = null;
                var minTotalCost = double.MaxValue;

                // Перебор всех типов сочетаний
                foreach (var combinationType in combinationTypes)
                {
                    // Проверяем, что количество весов соответствует количеству категорий в сочетании
                    if (weights.Count != combinationType.Count)
                        continue; // Пропускаем, если не совпадает

                    // Получаем список категорий для текущего типа сочетания
                    var involvedCategories = categories
                        .Where(c => combinationType.Contains(c.Name))
                        .ToList();

                    // Проверяем, что все категории присутствуют
                    if (involvedCategories.Count != combinationType.Count)
                        continue; // Пропускаем, если какая-то категория отсутствует

                    // Создаём решатель
                    var solver = Solver.CreateSolver("GLOP");
                    if (solver == null)
                    {
                        Console.WriteLine("Не удалось создать решатель.");
                        return;
                    }

                    // Создаём переменные: для каждой шихты в вовлечённых категориях
                    // Переменные представляют долю веса, назначенную каждой шихте
                    var variables = new Dictionary<string, Variable>();
                    foreach (var category in involvedCategories)
                    {
                        foreach (var shihta in category.Shihtas)
                        {
                            // Переменная x_шифта
                            variables[shihta.Name] = solver.MakeNumVar(0.0, 1.0, $"x_{shihta.Name}");
                        }
                    }

                    // Ограничения: для каждой категории, сумма долей шихт равна заданному весу
                    for (var i = 0; i < combinationType.Count; i++)
                    {
                        var categoryName = combinationType[i];
                        var assignedWeight = weights[i];

                        var category = categories.First(c => c.Name == categoryName);
                        var categoryVariables = category.Shihtas.Select(s => variables[s.Name]).ToList();

                        // Сумма x_j для данной категории = assignedWeight
                        var categoryConstraint = solver.MakeConstraint(assignedWeight, assignedWeight, $"Sum_{categoryName}");
                        foreach (var var in categoryVariables)
                        {
                            categoryConstraint.SetCoefficient(var, 1);
                        }
                    }

                    // Ограничения на среднюю зольность
                    // avgAsh = sum(ash_j * x_j) / sum(x_j) = sum(ash_j * x_j) = между 7.5 и 9.5
                    var totalWeight = weights.Sum(); // Должно быть 1.0
                    var ashExpr = new LinearExpr();
                    var plasticityExpr = new LinearExpr();

                    foreach (var category in involvedCategories)
                    {
                        foreach (var shihta in category.Shihtas)
                        {
                            ashExpr += shihta.Ash * variables[shihta.Name];
                            plasticityExpr += shihta.Plasticity * variables[shihta.Name];
                        }
                    }

                    // Зольность
                    solver.Add(ashExpr >= 7.5);
                    solver.Add(ashExpr <= 9.5);

                    // Пластичность
                    solver.Add(plasticityExpr >= 7.0);
                    solver.Add(plasticityExpr <= 14.0);

                    // Целевая функция: минимизация суммарных затрат
                    var objective = solver.Objective();
                    foreach (var shihta in involvedCategories.SelectMany(category => category.Shihtas))
                    {
                        objective.SetCoefficient(variables[shihta.Name], shihta.Cost);
                    }
                    objective.SetMinimization();

                    // Решение задачи
                    var resultStatus = solver.Solve();

                    // Проверка статуса решения
                    if (resultStatus == Solver.ResultStatus.OPTIMAL)
                    {
                        var totalCost = objective.Value();

                        if (totalCost < minTotalCost)
                        {
                            minTotalCost = totalCost;
                            // Собираем выбранные шихты
                            var selectedShihtas = new List<Shihta>();
                            var selectedWeights = new List<double>();

                            foreach (var category in involvedCategories)
                            {
                                foreach (var shihta in category.Shihtas)
                                {
                                    var weight = variables[shihta.Name].SolutionValue();
                                    if (weight > 1e-6) // Учитываем только выбранные шихты
                                    {
                                        selectedShihtas.Add(shihta);
                                        selectedWeights.Add(weight);
                                    }
                                }
                            }

                            // Вычисляем средние значения
                            var avgAsh = selectedShihtas.Select((s, idx) => s.Ash * selectedWeights[idx]).Sum();
                            var avgPlasticity = selectedShihtas.Select((s, idx) => s.Plasticity * selectedWeights[idx]).Sum();

                            bestCombination = new SelectedCombination
                            {
                                CombinationType = combinationType,
                                Shihtas = selectedShihtas,
                                Weights = selectedWeights,
                                AverageAsh = avgAsh,
                                AveragePlasticity = avgPlasticity,
                                TotalCost = totalCost
                            };
                        }
                    }
                    // Можно добавить обработку других статусов, если необходимо
                }

                // Добавляем найденную оптимальную комбинацию для текущей весовой комбинации
                if (bestCombination != null)
                {
                    optimalSolutions.Add(bestCombination);
                }
            }

            // Вывод результатов
            foreach (var solution in optimalSolutions)
            {
                Console.WriteLine($"Оптимальная комбинация для весов: {string.Join(" + ", solution.Weights.Select(w => w.ToString("0.0")))}");
                Console.WriteLine($"Тип сочетания: {string.Join(" + ", solution.CombinationType)}");
                for (int i = 0; i < solution.Shihtas.Count; i++)
                {
                    var shihta = solution.Shihtas[i];
                    var weight = solution.Weights[i];
                    Console.WriteLine($"- {shihta.Name} (Категория: {shihta.Category}, Вес: {weight:F1}, Стоимость: {shihta.Cost}, Пластичность: {shihta.Plasticity}, Зольность: {shihta.Ash})");
                }
                Console.WriteLine($"Средняя зольность: {solution.AverageAsh:F2}");
                Console.WriteLine($"Средняя пластичность: {solution.AveragePlasticity:F2}");
                Console.WriteLine($"Суммарные затраты: {solution.TotalCost:F2}");
                Console.WriteLine(new string('-', 50));
            }

            // Дополнительная проверка, если для некоторых весов не найдено решений
            // В текущей реализации, если решения не найдены, они просто не добавляются в список
            // Можно реализовать дополнительную логику для уведомления об этом
        }

        // Метод для инициализации категорий и шихт
        static List<Category> InitializeCategories()
        {
            // Создание категорий
            var categories = new List<Category>
            {
                new("К"),
                new("Ж"),
                new("Г"),
                new("ОС"),
                new("СС")
            };

            // Добавление шихт в категории

            // Категория К (x1 - x12)
            categories.First(c => c.Name == "К").Shihtas.Add(new Shihta("x1", "К", 8.3, 14.0, 10.43));
            categories.First(c => c.Name == "К").Shihtas.Add(new Shihta("x2", "К", 7.3, 14.2, 10.2));
            categories.First(c => c.Name == "К").Shihtas.Add(new Shihta("x3", "К", 6.3, 14.4, 10.68));

            categories.First(c => c.Name == "К").Shihtas.Add(new Shihta("x4", "К", 9.6, 10.0, 7.56));
            categories.First(c => c.Name == "К").Shihtas.Add(new Shihta("x5", "К", 8.6, 10.2, 7.4));
            categories.First(c => c.Name == "К").Shihtas.Add(new Shihta("x6", "К", 7.6, 10.4, 7.68));

            categories.First(c => c.Name == "К").Shihtas.Add(new Shihta("x7", "К", 8.9, 14.0, 12.57));
            categories.First(c => c.Name == "К").Shihtas.Add(new Shihta("x8", "К", 7.9, 14.2, 12.47));
            categories.First(c => c.Name == "К").Shihtas.Add(new Shihta("x9", "К", 6.9, 14.4, 13.17));

            categories.First(c => c.Name == "К").Shihtas.Add(new Shihta("x10", "К", 8.6, 7.0, 10.34));
            categories.First(c => c.Name == "К").Shihtas.Add(new Shihta("x11", "К", 7.6, 7.2, 10.31));
            categories.First(c => c.Name == "К").Shihtas.Add(new Shihta("x12", "К", 6.6, 7.4, 10.41));

            // Категория Ж (x13 - x18)
            categories.First(c => c.Name == "Ж").Shihtas.Add(new Shihta("x13", "Ж", 10.5, 30.0, 12.69));
            categories.First(c => c.Name == "Ж").Shihtas.Add(new Shihta("x14", "Ж", 9.5, 30.3, 13.02));
            categories.First(c => c.Name == "Ж").Shihtas.Add(new Shihta("x15", "Ж", 8.5, 30.6, 13.86));

            categories.First(c => c.Name == "Ж").Shihtas.Add(new Shihta("x16", "Ж", 9.2, 28.0, 9.22));
            categories.First(c => c.Name == "Ж").Shihtas.Add(new Shihta("x17", "Ж", 8.2, 28.3, 9.48));
            categories.First(c => c.Name == "Ж").Shihtas.Add(new Shihta("x18", "Ж", 7.2, 28.6, 9.79));

            // Категория Г (x19 - x21)
            categories.First(c => c.Name == "Г").Shihtas.Add(new Shihta("x19", "Г", 7.5, 13.0, 8.8));
            categories.First(c => c.Name == "Г").Shihtas.Add(new Shihta("x20", "Г", 6.5, 13.2, 8.87));
            categories.First(c => c.Name == "Г").Shihtas.Add(new Shihta("x21", "Г", 5.5, 13.4, 9.48));

            // Категория ОС (x22 - x24, x28)
            categories.First(c => c.Name == "ОС").Shihtas.Add(new Shihta("x22", "ОС", 9.0, 7.0, 11.81));
            categories.First(c => c.Name == "ОС").Shihtas.Add(new Shihta("x23", "ОС", 8.0, 7.2, 11.83));
            categories.First(c => c.Name == "ОС").Shihtas.Add(new Shihta("x24", "ОС", 7.0, 7.4, 12.21));
            categories.First(c => c.Name == "ОС").Shihtas.Add(new Shihta("x28", "ОС", 6.0, 6.0, 8.64));

            // Категория СС (x25 - x27)
            categories.First(c => c.Name == "СС").Shihtas.Add(new Shihta("x25", "СС", 7.0, 6.0, 9.15));
            categories.First(c => c.Name == "СС").Shihtas.Add(new Shihta("x26", "СС", 6.0, 6.0, 9.11));
            categories.First(c => c.Name == "СС").Shihtas.Add(new Shihta("x27", "СС", 5.0, 6.0, 9.77));

            return categories;
        }

        // Метод для получения всех возможных выборок шихт из вовлечённых категорий
        static IEnumerable<List<Shihta>> GetAllSelections(List<Category> involvedCategories)
        {
            // Начнём с пустого списка
            IEnumerable<List<Shihta>> selections = new List<List<Shihta>> { new() };

            foreach (var category in involvedCategories)
            {
                // Для каждой категории добавляем все возможные шихты
                selections = from seq in selections
                            from shihta in category.Shihtas
                            select new List<Shihta>(seq) { shihta };
            }

            return selections;
        }
    }
}
