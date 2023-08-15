// See https://aka.ms/new-console-template for more information

using System.Collections;

var person = new Person("Test", 18);
foreach (var i in person)
{
    Console.WriteLine(i);
}

public class Person : IEnumerable<string>
{
    private string name;
    private int age;

    public Person(string name, int age)
    {
        this.name = name;
        this.age = age;
    }
    
    public IEnumerator<string> GetEnumerator()
    {
        yield return name;
        yield return age.ToString();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}