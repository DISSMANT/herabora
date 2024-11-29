namespace SimplexMethod;

public class BruteForceSolution
{
    public static void BruteForce()
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
            new() { 0.5, 0.5 }, // 0.5 + 0.5
            new() { 0.2, 0.8 }, // 0.2 + 0.8
            new() { 0.1, 0.9 } // 0.1 + 0.9
        };

        // Перебор всех весовых комбинаций и поиск оптимальных решений
        var optimalSolutions = new List<SelectedCombination>();

        foreach (var weights in weightCombinations)
        {
            // Перебор всех типов сочетаний
            foreach (var combinationType in combinationTypes)
            {
                // Проверяем, что количество весов соответствует количеству категорий в сочетании
                if (weights.Count != combinationType.Count)
                    continue; // Пропускаем, если не совпадает

                // Получаем список категорий для текущего типа сочетания
                List<Category> involvedCategories = categories
                    .Where(c => combinationType.Contains(c.Name))
                    .ToList();

                // Проверяем, что все категории присутствуют
                if (involvedCategories.Count != combinationType.Count)
                    continue; // Пропускаем, если какая-то категория отсутствует

                // Получаем все возможные выборки шихт из вовлечённых категорий
                var allSelections = GetAllSelections(involvedCategories);

                foreach (var selection in allSelections)
                {
                    // Вычисляем средние значения зольности и пластичности с учетом весов
                    var avgAsh = 0.0;
                    var avgPlasticity = 0.0;
                    var totalCost = 0.0;

                    for (int i = 0; i < selection.Count; i++)
                    {
                        avgAsh += weights[i] * selection[i].Ash;
                        avgPlasticity += weights[i] * selection[i].Plasticity;
                        totalCost += weights[i] * selection[i].Cost;
                    }

                    // Проверка ограничений
                    if (avgAsh >= 7.5 && avgAsh <= 9.5 &&
                        avgPlasticity >= 7.0 && avgPlasticity <= 14.0)
                    {
                        // Сохраняем валидную комбинацию
                        optimalSolutions.Add(new SelectedCombination
                        {
                            CombinationType = combinationType,
                            Shihtas = selection,
                            Weights = new List<double>(weights),
                            AverageAsh = avgAsh,
                            AveragePlasticity = avgPlasticity,
                            TotalCost = totalCost
                        });
                    }
                }
            }
        }

        // Группировка оптимальных решений по весовым комбинациям
        var groupedSolutions = optimalSolutions
            .GroupBy(c => string.Join("+", c.Weights.Select(w => w.ToString("0.0"))))
            .ToList();

        // Вывод результатов для каждой весовой комбинации
        foreach (var group in groupedSolutions)
        {
            var optimalCombination = group.OrderBy(c => c.TotalCost).FirstOrDefault();
            if (optimalCombination != null)
            {
                Console.WriteLine(
                    $"Оптимальная комбинация для весов: {optimalCombination.Weights[0]} + {optimalCombination.Weights[1]}");
                Console.WriteLine($"Тип сочетания: {string.Join(" + ", optimalCombination.CombinationType)}");
                foreach (var shihta in optimalCombination.Shihtas)
                {
                    int index = optimalCombination.Shihtas.IndexOf(shihta);
                    double weight = optimalCombination.Weights[index];
                    Console.WriteLine(
                        $"- {shihta.Name} (Категория: {shihta.Category}, Вес: {weight}, Стоимость: {shihta.Cost}, Пластичность: {shihta.Plasticity}, Зольность: {shihta.Ash})");
                }

                Console.WriteLine($"Средняя зольность: {optimalCombination.AverageAsh:F2}");
                Console.WriteLine($"Средняя пластичность: {optimalCombination.AveragePlasticity:F2}");
                Console.WriteLine($"Суммарные затраты: {optimalCombination.TotalCost:F2}");
                Console.WriteLine(new string('-', 50));
            }
        }

        // Дополнительная проверка, если для некоторых весов не найдено решений
        foreach (var weights in weightCombinations)
        {
            string weightKey = string.Join("+", weights.Select(w => w.ToString("0.0")));
            if (groupedSolutions.All(g => g.Key != weightKey))
            {
                Console.WriteLine($"Для весовой комбинации {weightKey} не найдена подходящая комбинация шихт.");
                Console.WriteLine(new string('-', 50));
            }
        }
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

        // Категория К (x1 - x9)
        categories.First(c => c.Name == "К").Shihtas.Add(new Shihta("x1", "К", 8.3, 14.0, 10.43));
        categories.First(c => c.Name == "К").Shihtas.Add(new Shihta("x2", "К", 7.3, 14.2, 10.2));
        categories.First(c => c.Name == "К").Shihtas.Add(new Shihta("x3", "К", 6.3, 14.4, 10.68));

        categories.First(c => c.Name == "К").Shihtas.Add(new Shihta("x4", "К", 9.6, 10.0, 7.56));
        categories.First(c => c.Name == "К").Shihtas.Add(new Shihta("x5", "К", 8.6, 10.2, 7.4));
        categories.First(c => c.Name == "К").Shihtas.Add(new Shihta("x6", "К", 7.6, 10.4, 7.68));

        categories.First(c => c.Name == "К").Shihtas.Add(new Shihta("x7", "К", 8.9, 14.0, 12.57));
        categories.First(c => c.Name == "К").Shihtas.Add(new Shihta("x8", "К", 7.9, 14.2, 12.47));
        categories.First(c => c.Name == "К").Shihtas.Add(new Shihta("x9", "К", 6.9, 14.4, 13.17));

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

    // Метод для инициализации типов сочетаний с весовыми комбинациями
    static List<CombinationType> InitializeCombinationTypes()
    {
        return new List<CombinationType>
        {
            // К = Г
            new CombinationType(
                new List<string> { "К", "Г" },
                new List<List<double>>
                {
                    new List<double> { 0.5, 0.5 },
                    new List<double> { 0.2, 0.8 },
                    new List<double> { 0.1, 0.9 }
                }
            ),
            // К = ОС
            new CombinationType(
                new List<string> { "К", "ОС" },
                new List<List<double>>
                {
                    new List<double> { 0.5, 0.5 },
                    new List<double> { 0.2, 0.8 },
                    new List<double> { 0.1, 0.9 }
                }
            ),
            // К = СС
            new CombinationType(
                new List<string> { "К", "СС" },
                new List<List<double>>
                {
                    new List<double> { 0.5, 0.5 },
                    new List<double> { 0.2, 0.8 },
                    new List<double> { 0.1, 0.9 }
                }
            ),
            // К = ОС + СС
            new CombinationType(
                new List<string> { "К", "ОС", "СС" },
                new List<List<double>>
                {
                    new List<double> { 0.33, 0.33, 0.34 },
                    new List<double> { 0.2, 0.3, 0.5 },
                    new List<double> { 0.1, 0.4, 0.5 }
                }
            ),
            // К = Г + ОС + СС
            new CombinationType(
                new List<string> { "К", "Г", "ОС", "СС" },
                new List<List<double>>
                {
                    new List<double> { 0.25, 0.25, 0.25, 0.25 },
                    new List<double> { 0.2, 0.2, 0.3, 0.3 },
                    new List<double> { 0.1, 0.3, 0.3, 0.3 }
                }
            )
            // Добавьте дополнительные сочетания и весовые комбинации, если необходимо
        };
    }

    // Метод для получения всех возможных выборок шихт из вовлечённых категорий
    static IEnumerable<List<Shihta>> GetAllSelections(List<Category> involvedCategories)
    {
        // Начнём с пустого списка
        IEnumerable<List<Shihta>> selections = new List<List<Shihta>> { new List<Shihta>() };

        foreach (var category in involvedCategories)
        {
            // Для каждой категории добавляем все возможные шихты
            selections = from seq in selections
                from shihta in category.Shihtas
                select new List<Shihta>(seq) { shihta };
        }

        return selections;
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

    // Класс для представления типа сочетания и его весовых комбинаций
    public class CombinationType
    {
        public List<string> Categories { get; set; }
        public List<List<double>> WeightCombinations { get; set; }

        public CombinationType(List<string> categories, List<List<double>> weightCombinations)
        {
            Categories = categories;
            WeightCombinations = weightCombinations;
        }
    }
}