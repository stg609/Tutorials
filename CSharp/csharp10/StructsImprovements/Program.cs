
// net6 c# 10

// 之前结构体必须要定义有参构造函数，现在允许定义无参构造函数
System.Console.WriteLine(new Person());


public struct Person
{
    public string Name { get; set; }

    public Person()
    {
        Name = "cx";
    }

    public Person(string name)
    {
        Name = name;
    }
}