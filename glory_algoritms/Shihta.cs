namespace SimplexMethod;

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