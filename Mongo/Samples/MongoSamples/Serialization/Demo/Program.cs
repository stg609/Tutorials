using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace Demo
{
    class Program
    {
        const string MongoDBConnectionString = "mongodb://localhost:27017";

        async static Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            // demo1: 不需要使用 class map，由序列化器自动生成。
            DemoNoClassMapExplictily();

            // 演示集合中存在多态的文档，取出来后类型信息丢失
            await DemoPolymorphPart1();

            // 演示多态，设置 discriminator
            //await DemoPollymorphPart2();

            // demo: 含构造函数
            DemoConstructor();

            // demo: 属性名不一样
            DemoDiffName();
            DemoDiffIdName();

            // demo: 部分属性不需要映射
            DemoIgnore();

            // demo: 部分属性如果为 null 则不需要映射
            DemoIgnoreNull();

            // demo: 演示 BsonDocument 有多余字段反序列化不抛错
            DemoIgnoreExtraMembers();

            // demo: 演示 使用 SetExtraElementsMember 来捕获 BsonDocument 多余字段
            DemoExtraMemberCatchAll();
        }

        private static void DemoDiffIdName()
        {
            Test t = new Test
            {
                Id = "1",
                Name = "2"
            };

            var doc = BsonDocument.Parse("{\"Id\":\"111\",\"Name\":\"cx\"}");

            BsonClassMap.RegisterClassMap<Test>(x =>
            {
                x.AutoMap();
                //x.MapMember(y => y.Name).SetElementName("name");
                x.UnmapProperty(y => y.Id);
                x.MapMember(y => y.Id).SetElementName("Id");
            });

            var t2 = BsonSerializer.Deserialize<Test>(doc);
        }

        private static void DemoExtraMemberCatchAll()
        {
            BsonClassMap.RegisterClassMap<WithExtraMemberCatchAll>(cm =>
            {
                cm.AutoMap();
                cm.SetExtraElementsMember(cm.GetMemberMap(c => c.CatchAll)); // CatchAll 则会包含所有 BsonDocument 中多余的字段
            });
            WithExtraMemberCatchAll2 demo = new WithExtraMemberCatchAll2
            {
                Name = "user1",
                Name2 = "user2"
            };
            var doc = demo.ToBsonDocument();
            var obj = BsonSerializer.Deserialize<WithExtraMemberCatchAll>(doc);
        }

        private static void DemoIgnoreExtraMembers()
        {
            BsonClassMap.RegisterClassMap<WithExtraMember>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true); // 如果不设置 true, 则当 BsonDocument 中的字段多余 Class 中定义的字段，则会报错
            });
            WithExtraMember2 demo = new WithExtraMember2
            {
                Name = "user1",
                Name2 = "user2"
            };
            var doc = demo.ToBsonDocument();
            var obj = BsonSerializer.Deserialize<WithExtraMember>(doc);
        }

        private static void DemoIgnoreNull()
        {
            BsonClassMap.RegisterClassMap<WithIgnoreNull>(cm =>
            {
                cm.AutoMap();
                cm.GetMemberMap(c => c.Name2).SetIgnoreIfNull(true);// 或者使用 [BsonIgnoreIfNull]
            });
            WithIgnoreNull demo = new WithIgnoreNull
            {
                Name = "user1",
            };
            var doc = demo.ToBsonDocument();
            var obj = BsonSerializer.Deserialize<WithIgnoreNull>(doc);
        }

        private static void DemoIgnore()
        {
            BsonClassMap.RegisterClassMap<WithIgnore>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(y => y.Name2); // 或者使用 [BsonIgnore]
            });
            WithIgnore demo = new WithIgnore
            {
                Name = "user1",
            };
            var doc = demo.ToBsonDocument();
            var obj = BsonSerializer.Deserialize<WithIgnore>(doc);
        }

        private static void DemoDiffName()
        {
            BsonClassMap.RegisterClassMap<WithDifferName>(cm =>
            {
                cm.AutoMap();

                // 只有上面调用了 AutoMap, 这里才能GetMemberMap，否则将无法找到
                cm.GetMemberMap(y => y.Name).SetElementName("nm");
            });
            WithDifferName demo = new WithDifferName
            {
                Name = "user1",
            };
            var doc = demo.ToBsonDocument();
            var obj = BsonSerializer.Deserialize<WithDifferName>(doc);
        }

        private static void DemoNoClassMapExplictily()
        {
            NoNeedClassMap1 demo = new NoNeedClassMap1
            {
                Date = DateTime.Parse("2000-01-01"),
                Name = "user1",
                Price = 1.2m
            };

            var doc = demo.ToBsonDocument();
            var obj = BsonSerializer.Deserialize<NoNeedClassMap1>(doc);
        }

        private static async Task DemoPolymorphPart1()
        {
            // demo2: 演示集合中存在多态的文档，取出来后类型信息丢失
            Cat demo2Cat = new Cat
            {
                Name = "cat1"
            };
            Dog demo2Dog = new Dog
            {
                Name = "dog1"
            };
            List<Anmial> coll = new List<Anmial>
            {
                demo2Cat,demo2Dog
            };

            BsonClassMap.RegisterClassMap<House>(cm =>
            {
                cm.AutoMap(); // 如果大部分字段都自动映射，则加上 AutoMap。

            });

            var client = new MongoClient(MongoDBConnectionString);
            var database = client.GetDatabase("TestMongoDBStore");
            var house = database.GetCollection<House>("house");


            // 1. 如果要观察效果，第2次执行的时候注释掉下面语句
            await database.DropCollectionAsync("house");
            await database.CreateCollectionAsync("house");
            house.InsertOne(new House { Pets = coll }); // 数据库中保存为：Pets:[{ _t: Cat, Name: cat1}, {_t: Dog, Name:dog1}]
            // 注释结束位置

            // 2. 观察第一次和第二次的区别，
            // 第一次因为是刚插入完就取出来，所以多台的关系内存中会自动创建，所以取出来的时候 pet 可以正常解析到Cat,Dog类型。
            // 第二次因为注释掉了上面代码，所以没有 class map，此时返回的 pet 都是 Animal
            var data = house.AsQueryable().FirstOrDefault();
        }

        private static async Task DemoPollymorphPart2()
        {
            // demo2(part2): 演示集合中存在多态的文档
            Cat demo2Cat = new Cat
            {
                Name = "cat1"
            };
            Dog demo2Dog = new Dog
            {
                Name = "dog1"
            };
            Lion demo2Lion = new Lion
            {
                Name = "lion"
            };
            List<Anmial> coll = new List<Anmial>
            {
                demo2Cat,demo2Dog,demo2Lion
            };

            BsonClassMap.RegisterClassMap<House>(cm =>
            {
                cm.AutoMap(); // 如果大部分字段都自动映射，则加上 AutoMap。
            });
            BsonClassMap.RegisterClassMap<Anmial>(cm =>
            {
                cm.AutoMap();
                cm.SetIsRootClass(true); // 设置是父类，则生成的 _t 中会携带父类信息，这样在查询的时候就可以基于父类进行查找。
            });

            // 3. 通过注册子类的 class map，可以正确识别到子类，或者在父类Animal 上，增加 [BsonKnownTypes(typeof(Cat), typeof(Dog))]
            BsonClassMap.RegisterClassMap<Cat>();
            BsonClassMap.RegisterClassMap<Dog>();

            var client = new MongoClient(MongoDBConnectionString);
            var database = client.GetDatabase("TestMongoDBStore");
            var house = database.GetCollection<House>("house");


            // 1. 如果要观察效果，第2次执行的时候注释掉下面语句
            await database.DropCollectionAsync("house");
            await database.CreateCollectionAsync("house");
            house.InsertOne(new House { Pets = coll }); // 如果 setIsRootClass 没有开启，则数据库中保存为：Pets:[{ _t: Cat, Name: cat1}, {_t: Dog, Name:dog1}, {_t: Lion, Name: lion}], 如果 SetIsRootClass 开启，则数据库中保存为 Pets:[{ _t: [Animal,Cat], Name: cat1}, {_t: [Animal,Dog], Name:dog1}, {_t: [Animal,Cat,Lion], Name: lion}]
            // 注释结束位置

            // 2. 观察第一次和第二次的区别，
            // 第一次因为是刚插入完就取出来，所以多台的关系内存中会自动创建，所以取出来的时候 pet 可以正常解析到Cat,Dog类型。
            // 第二次因为注释掉了上面代码，所以没有 class map，此时返回的 pet 都是 Animal
            var data = house.AsQueryable().FirstOrDefault();
        }

        private static void DemoConstructor()
        {
            BsonClassMap.RegisterClassMap<WithConstructorClassMap1>(cm =>
            {
                cm.AutoMap(); // 如果大部分字段都自动映射，则加上 AutoMap。

                // 对 AutoMap 中的配置进行覆盖
                cm.MapMember(p => p.Date); // readonly 不会自动映射，必须手动加入到 map 中。或使用 [BsonElement] attribute
                cm.MapCreator(p => new WithConstructorClassMap1(p.Date)); // 有构造函数的必须手动配置，否则反序列化的时候将不会调用构造函数
            });
            WithConstructorClassMap1 demo3 = new WithConstructorClassMap1(DateTime.Parse("2000-01-01"))
            {
                Name = "user1",
                Price = 1.2m
            };
            var demo3Bson = demo3.ToBsonDocument();
            var demo3Class = BsonSerializer.Deserialize<WithConstructorClassMap1>(demo3Bson);
        }
    }

    class NoNeedClassMap1
    {
        // field
        public DateTime Date;

        // prop
        public string Name { get; set; }

        public decimal Price { get; set; }
    }

    class WithConstructorClassMap1
    {
        public WithConstructorClassMap1(DateTime dt)
        {
            Date = dt;
        }

        // field
        public readonly DateTime Date;

        // prop
        public string Name { get; set; }

        public decimal Price { get; set; }
    }

    class House
    {
        public string Id { get; set; }
        public List<Anmial> Pets { get; set; }
    }

    class Anmial
    {
        public string Name { get; set; }
    }

    class Cat : Anmial
    {

    }

    class Dog : Anmial
    {

    }
    class Lion : Cat
    {
    }

    class Tiger : Cat
    {
    }


    class WithDifferName
    {
        public string Name { get; set; }

    }


    class WithIgnore
    {
        public string Name { get; set; }
        public string Name2 { get; set; }
    }

    class WithIgnoreNull
    {
        public string Name { get; set; }
        public string Name2 { get; set; }
    }

    class WithExtraMember
    {
        public string Name { get; set; }
    }
    class WithExtraMember2
    {
        public string Name { get; set; }
        public string Name2 { get; set; }
    }

    class WithExtraMemberCatchAll
    {
        public string Name { get; set; }

        public BsonDocument CatchAll { get; set; }
    }
    class WithExtraMemberCatchAll2
    {
        public string Name { get; set; }
        public string Name2 { get; set; }
    }

    class Test
    {
        public string Id { get; set; }

        public string Name { get; set; }
    }
}
