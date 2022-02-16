using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using MongoDB.Bson;

namespace Demo1
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            SimpleDynamicProperty();

            DynamicPropertyViaVariable();

            // 反注释来查看效果
            // demo：继承自 DynamicObject，此时由自定义的这个类型来控制如何对待动态的属性，方法，等。如果是个空的，则会报错
            //DynamicUsingEmptyDynamicObject();

            // demo: 通过 override TryGetMember 和 setGetMember 来对动态的属性进行控制，实现类似 ExpandoObject 的方式
            DynamicUsingDynamicObject();

            DynamicMappingFieldsObject();

            BsonDocumentToDynamicObject();
        }

        private static void DynamicPropertyViaVariable()
        {
            dynamic obj = new ExpandoObject();
            // demo: dynamic with propName varible
            string propName = "B";
            string propValue = "2";

            // 下面这种类似 js 的用法不支持，会提示：Cannot apply indexing with [] to an expression of type 'System.Dynamic.ExpandoObject'”
            // obj[propName] = propValue;

            // 需要通过下面这种
            var dictionary = obj as IDictionary<string, object>;
            dictionary.Add(propName, propValue);
            Console.WriteLine(obj.B);
        }

        private static void SimpleDynamicProperty()
        {
            // demo: dynamic property
            dynamic obj = new ExpandoObject();
            obj.A = 1;
            Console.WriteLine(obj.A);
        }

        private static void DynamicUsingEmptyDynamicObject()
        {
            dynamic obj = new MyEmptyDynamicObject();
            obj.A = 1;
            Console.WriteLine(obj.A);
        }

        private static void DynamicUsingDynamicObject()
        {
            dynamic obj = new MyDynaimcObject();
            obj.A = 1;
            Console.WriteLine(obj.A);

            // 由于我们增加了一个AddProperty, 那么就可以不用像 ExpandoObject 那样转换成 IDictioanry 了
            obj.AddProperty("B", 2);
            Console.WriteLine(obj.B);

            // 如果想要类似 js 这种 obj["xxx"] = 3 这种方式来动态增加属性
            obj["C"] = 3;
            Console.WriteLine(obj.C);

            // 如果想要对某个属性进行重命名
            obj.RenameProperty("B", "B2");
            Console.WriteLine(obj["B2"]);

            //StaticClass a1 = new StaticClass();
            //dynamic obj2 = new ReflectionDynamicObject(a1);
            //obj2.A = 1;
            //var c = (StaticClass)obj2;

            // 如果 D 没有加到 property 中，则会出错。
            Console.WriteLine(obj.D);
        }

        private static void BsonDocumentToDynamicObject()
        {
            BsonClass obj = new BsonClass
            {
                Date = DateTime.Now,
                Value = 1.2m,
                Coll = new List<BsonChildClass>
                {
                    new BsonChildClass
                    {
                        NestDate = DateTime.Now
                    }
                }
            };
            BsonDocument doc = obj.ToBsonDocument();

            var obj2 = BsonTypeMapper.MapToDotNetValue(doc);
            var sJson = Newtonsoft.Json.JsonConvert.SerializeObject(obj2);
            dynamic dyn = Newtonsoft.Json.JsonConvert.DeserializeObject<MyDynaimcObject>(sJson);

            Console.WriteLine(dyn.Date);
            Console.WriteLine(dyn.Value);
        }

        /// <summary>
        /// demo 演示在执行时动态替换字段
        /// </summary>
        private static void DynamicMappingFieldsObject()
        {
            // 完全用 dynamic object 来表示一个对象(及其中的属性，尤其是其中嵌套的属性，如集合）
            dynamic dyn = new MyBsonDocumentDynaimcObject();
            dyn.Date = DateTime.Now;
            dyn.Value = 1.2m;
            var lst = new List<MyBsonDocumentDynaimcObject>();
            dynamic itm = new MyBsonDocumentDynaimcObject();
            itm.NestDate = DateTime.Now;
            itm.NestDecimal = 0.1m;
            lst.Add(itm);
            dyn.Coll = lst;


            // 测试动态映射，__somefield 应该替换成 Date
            var f1 = dyn.__somefield;
            var result = "";

            foreach (var item in dyn.__somecoll)
            {
                result += item.__somecoll__somefield2;
            }

        }
    }


    class StaticClass
    {
        public int A { get; set; }
        public int B { get; set; }
        public int C { get; set; }
    }


    class BsonClass
    {
        public DateTime Date { get; set; }

        public decimal Value { get; set; }

        public List<BsonChildClass> Coll { get; set; }
    }

    class BsonChildClass
    {
        public DateTime NestDate { get; set; }
        public decimal NestDecimal { get; set; }
    }

    class MyEmptyDynamicObject : DynamicObject
    {

    }

    class MyDynaimcObject : DynamicObject
    {
        Dictionary<string, object> _dic = new Dictionary<string, object>();

        // 告诉 dynamic 如何对待没有的属性
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return _dic.TryGetValue(binder.Name, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            AddProperty(binder.Name, value);
            return true;
        }

        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            //var props = binder.Type.GetProperties(System.Reflection.BindingFlags.Public);
            //foreach (var prop in props)
            //{
            //    binder.
            //}
            result = null;
            return false;
        }

        public void AddProperty(string name, object value)
        {
            _dic[name] = value;
        }

        public void RenameProperty(string oldProp, string newProp, object value = null)
        {
            object rawValue = null;
            if (_dic.ContainsKey(oldProp))
            {
                rawValue = _dic[oldProp];
                _dic.Remove(oldProp);
            }

            AddProperty(newProp, value ?? rawValue);
        }

        // 如果我们想类似 js 的这种 dyn["aa"]= 1 ，则可以增加如下方法
        public object this[string propertyName]
        {
            get
            {
                return _dic[propertyName];
            }
            set
            {
                AddProperty(propertyName, value);
            }
        }
    }

    class MyBsonDocumentDynaimcObject : DynamicObject
    {
        Dictionary<string, object> _dic = new Dictionary<string, object>();
       

        public MyBsonDocumentDynaimcObject()
        {
     
        }

        Dictionary<string, string> _mapping = new Dictionary<string, string>
            {
                {"__somefield","Date" },
                {"__somecoll","Coll" },
                {"__somecoll__somefield2","NestDecimal" }
            };


        // 告诉 dynamic 如何对待没有的属性
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            string realName = null;
            if (!_mapping.TryGetValue(binder.Name, out realName))
            {
                realName = binder.Name;
            }
            return _dic.TryGetValue(realName, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            string realName = null;
            if(!_mapping.TryGetValue(binder.Name, out realName))
            {
                realName = binder.Name;
            }

            AddProperty(realName, value);
            return true;
        }

        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            //var props = binder.Type.GetProperties(System.Reflection.BindingFlags.Public);
            //foreach (var prop in props)
            //{
            //    binder.
            //}
            result = null;
            return false;
        }

        public void AddProperty(string name, object value)
        {
            _dic[name] = value;
        }

        public void RenameProperty(string oldProp, string newProp, object value = null)
        {
            object rawValue = null;
            if (_dic.ContainsKey(oldProp))
            {
                rawValue = _dic[oldProp];
                _dic.Remove(oldProp);
            }

            AddProperty(newProp, value ?? rawValue);
        }

        // 如果我们想类似 js 的这种 dyn["aa"]= 1 ，则可以增加如下方法
        public object this[string propertyName]
        {
            get
            {
                return _dic[propertyName];
            }
            set
            {
                AddProperty(propertyName, value);
            }
        }
    }

#nullable enable
    public sealed class ReflectionDynamicObject : DynamicObject
    {
        private const BindingFlags InstanceDefaultBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private const BindingFlags StaticDefaultBindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        private static readonly ConcurrentDictionary<Type, TypeCache> s_cache = new();

        private readonly object? _originalObject;
        private readonly TypeCache _typeCache;

        public ReflectionDynamicObject(object obj)
        {
            _originalObject = obj ?? throw new ArgumentNullException(nameof(obj));

            var type = obj.GetType();
            _typeCache = s_cache.GetOrAdd(type, t => TypeCache.Create(t));
        }

        public ReflectionDynamicObject(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            _typeCache = s_cache.GetOrAdd(type, TypeCache.Create);
        }

        public ReflectionDynamicObject CreateInstance(params object[] parameters)
        {
            var exceptions = new List<Exception>();

            foreach (var constructor in _typeCache.Constructors)
            {
                var ctorParameters = constructor.GetParameters();
                if (ctorParameters.Length != parameters.Length)
                    continue;

                try
                {
                    return new ReflectionDynamicObject(constructor.Invoke(parameters));
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            Exception? innerException = exceptions.Count == 0 ? null : new AggregateException(exceptions);
            throw new ArgumentException($"Cannot create an instance of {_typeCache.Type.FullName} with the provided parameters.", nameof(parameters), innerException);
        }

        public override bool TryGetMember(GetMemberBinder binder, out object? result)
        {
            return TryGetMemberValue(binder.Name, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object? value)
        {
            return TrySetMemberValue(binder.Name, value);
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object? result)
        {
            if (_originalObject == null)
            {
                foreach (var indexer in _typeCache.StaticIndexers)
                {
                    if (TryGetIndex(indexer, _originalObject, indexes, out result))
                        return true;
                }
            }
            else
            {
                foreach (var indexer in _typeCache.InstanceIndexers)
                {
                    if (TryGetIndex(indexer, _originalObject, indexes, out result))
                        return true;
                }
            }

            result = null;
            return false;
        }

        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object? value)
        {
            if (_originalObject == null)
            {
                foreach (var indexer in _typeCache.StaticIndexers)
                {
                    if (TrySetIndex(indexer, instance: null, indexes, value))
                        return true;
                }
            }
            else
            {
                foreach (var indexer in _typeCache.InstanceIndexers)
                {
                    if (TrySetIndex(indexer, _originalObject, indexes, value))
                        return true;
                }
            }

            return false;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object?[]? args, out object? result)
        {
            var type = _typeCache.Type;
            var flags = _originalObject == null ? StaticDefaultBindingFlags : InstanceDefaultBindingFlags;
            flags |= BindingFlags.InvokeMethod;

            while (type != null)
            {
                try
                {
                    result = type.InvokeMember(binder.Name, flags, binder: null, _originalObject, args, culture: null);
                    return true;
                }
                catch (MissingMethodException)
                {
                    type = type.BaseType;
                }
            }

            result = null;
            return false;
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return GetAllMembers().Distinct(StringComparer.Ordinal);

            IEnumerable<string> GetAllMembers()
            {
                foreach (var propertyInfo in _typeCache.InstanceProperties)
                {
                    yield return propertyInfo.Key;
                }

                foreach (var field in _typeCache.InstanceIndexers)
                {
                    yield return field.Name;
                }

                foreach (var field in _typeCache.InstanceFields)
                {
                    yield return field.Key;
                }

                foreach (var propertyInfo in _typeCache.StaticProperties)
                {
                    yield return propertyInfo.Key;
                }

                foreach (var field in _typeCache.StaticIndexers)
                {
                    yield return field.Name;
                }

                foreach (var field in _typeCache.StaticFields)
                {
                    yield return field.Key;
                }
            }
        }

        public override bool TryConvert(ConvertBinder binder, out object? result)
        {
            result = Convert.ChangeType(_originalObject, binder.Type, provider: null);
            return true;
        }

        public override string? ToString()
        {
            if (_originalObject != null)
                return _originalObject.ToString();

            return null;
        }

        private bool TryGetMemberValue(string name, out object? result)
        {
            if (_originalObject == null)
            {
                if (_typeCache.StaticProperties.TryGetValue(name, out var property))
                {
                    result = property.GetValue(null);
                    return true;
                }

                if (_typeCache.StaticFields.TryGetValue(name, out var field))
                {
                    result = field.GetValue(null);
                    return true;
                }
            }
            else
            {
                if (_typeCache.InstanceProperties.TryGetValue(name, out var property))
                {
                    result = property.GetValue(_originalObject);
                    return true;
                }

                if (_typeCache.InstanceFields.TryGetValue(name, out var field))
                {
                    result = field.GetValue(_originalObject);
                    return true;
                }
            }

            result = null;
            return false;
        }

        private bool TrySetMemberValue(string name, object? value)
        {
            if (_originalObject == null)
            {
                if (_typeCache.StaticProperties.TryGetValue(name, out var property))
                {
                    property.SetValue(null, value);
                    return true;
                }

                if (_typeCache.StaticFields.TryGetValue(name, out var field))
                {
                    field.SetValue(null, value);
                    return true;
                }
            }
            else
            {
                if (_typeCache.InstanceProperties.TryGetValue(name, out var property))
                {
                    property.SetValue(_originalObject, value);
                    return true;
                }

                if (_typeCache.InstanceFields.TryGetValue(name, out var field))
                {
                    field.SetValue(_originalObject, value);
                    return true;
                }
            }

            return false;
        }

        private static bool TryGetIndex(PropertyInfo indexer, object? instance, object[] indexes, out object? result)
        {
            try
            {
                result = indexer.GetValue(instance, indexes);
                return true;
            }
            catch (ArgumentException)
            {
            }
            catch (TargetParameterCountException)
            {
            }
            catch (TargetInvocationException)
            {
            }

            result = null;
            return false;
        }

        private static bool TrySetIndex(PropertyInfo indexer, object? instance, object[] indexes, object? value)
        {
            try
            {
                indexer.SetValue(instance, value, indexes);
                return true;
            }
            catch (ArgumentException)
            {
            }
            catch (TargetParameterCountException)
            {
            }
            catch (TargetInvocationException)
            {
            }

            return false;
        }

        private sealed class TypeCache
        {
            private TypeCache(Type type)
            {
                Type = type;
            }

            public Type Type { get; }

            public List<ConstructorInfo> Constructors { get; } = new List<ConstructorInfo>();

            public Dictionary<string, PropertyInfo> InstanceProperties { get; } = new Dictionary<string, PropertyInfo>(StringComparer.Ordinal);
            public Dictionary<string, FieldInfo> InstanceFields { get; } = new Dictionary<string, FieldInfo>(StringComparer.Ordinal);
            public List<PropertyInfo> InstanceIndexers { get; } = new List<PropertyInfo>();

            public Dictionary<string, PropertyInfo> StaticProperties { get; } = new Dictionary<string, PropertyInfo>(StringComparer.Ordinal);
            public Dictionary<string, FieldInfo> StaticFields { get; } = new Dictionary<string, FieldInfo>(StringComparer.Ordinal);
            public List<PropertyInfo> StaticIndexers { get; } = new List<PropertyInfo>();

            public static TypeCache Create(Type type)
            {
                var typeCache = new TypeCache(type);
                typeCache.Constructors.AddRange(type.GetConstructors());

                var currentType = type;
                while (currentType != null)
                {
                    // Instances
                    foreach (var propertyInfo in currentType.GetProperties(InstanceDefaultBindingFlags))
                    {
                        if (propertyInfo.GetIndexParameters().Any())
                        {
                            typeCache.InstanceIndexers.Add(propertyInfo);
                        }
                        else
                        {
                            if (!typeCache.InstanceProperties.ContainsKey(propertyInfo.Name))
                            {
                                typeCache.InstanceProperties.Add(propertyInfo.Name, propertyInfo);
                            }
                        }
                    }

                    foreach (var fieldInfo in currentType.GetFields(InstanceDefaultBindingFlags))
                    {
                        if (!typeCache.InstanceFields.ContainsKey(fieldInfo.Name))
                        {
                            typeCache.InstanceFields.Add(fieldInfo.Name, fieldInfo);
                        }
                    }

                    // Static
                    foreach (var propertyInfo in currentType.GetProperties(StaticDefaultBindingFlags))
                    {
                        if (propertyInfo.GetIndexParameters().Any())
                        {
                            typeCache.StaticIndexers.Add(propertyInfo);
                        }
                        else
                        {
                            if (!typeCache.StaticProperties.ContainsKey(propertyInfo.Name))
                            {
                                typeCache.StaticProperties.Add(propertyInfo.Name, propertyInfo);
                            }
                        }
                    }

                    foreach (var fieldInfo in currentType.GetFields(StaticDefaultBindingFlags))
                    {
                        if (!typeCache.StaticFields.ContainsKey(fieldInfo.Name))
                        {
                            typeCache.StaticFields.Add(fieldInfo.Name, fieldInfo);
                        }
                    }

                    currentType = currentType.BaseType;
                }

                return typeCache;
            }
        }
    }
}
