// net5 c#9

// 这个实际上是一个语法糖，用来简化一个声明 不可变的 引用类型，（实际会生成一个 class, 并且所有属性字段只读，且 override 的比较方法，会比较对象内的属性）
// 可以用来声明一个 值类型 （DDD)
// 原生提供 Copy Clone
// 还提供一个 解构 的方式，用于直接获取内部的属性

var p1 = new Person("cx", 18);
var p2 = new Person("cx", 18);

// 演示：返回内部属性：Person { name = cx, age = 18 }
System.Console.WriteLine(p1.ToString());

// 演示：override 的比较方法，只要类型一样，值一样就认为一样
Console.WriteLine(p1 == p2); // 会返回 True

// 演示：如何修改属性,这里使用了with表达式，创建一个新的对象
var p3 = p2 with { name = "ck" };
System.Console.WriteLine(p3);

record Person(string name, int age);

// 支持继承
record Developer : Person
{
    // 支持添加额外属性
    public int Salary { get; }

    // 支持定义构造函数
    public Developer(string name, int age, int salary) : base(name, age)
    {
        Salary = salary;
    }

    // 支持 override
    public override string ToString()
    {
        return base.ToString();
    }
}