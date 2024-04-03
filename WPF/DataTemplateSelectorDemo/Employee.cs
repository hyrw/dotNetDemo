namespace DataTemplateSelectorDemo;

public class Employee(string department, string name, int age) : Person(name, age)
{
    public string Department { get; set; } = department;
}
