using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace TrivialDemo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DynamicObjectController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<DynamicObjectController> _logger;

        public DynamicObjectController(ILogger<DynamicObjectController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get(CancellationToken cancellationToken)
        {
            dynamic aaa = new MyExpandoObject();
            aaa.Name = "aaa";

            var b = aaa as IDictionary<string, object>;

            var c = b as MyExpandoObject;

            return Content(aaa.Name + aaa.bb);
        }
    }

    public class MyExpandoObject : DynamicObject, IDictionary<string, object> 
    {
        Dictionary<string, object> _dic = new Dictionary<string, object>();

        public ICollection<string> Keys => _dic.Keys;

        public ICollection<object> Values => _dic.Values;

        public int Count => _dic.Count;

        public bool IsReadOnly => false;

        public MyExpandoObject()
        {

        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            bool found = _dic.TryGetValue(binder.Name, out result);
            if (!found)
            {
                result = null;
            }
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            AddProperty(binder.Name, value);
            return true;
        }

        public void AddProperty(string name, object value)
        {
            _dic[name] = value;
        }

        public void Add(string key, object value)
        {
            _dic.Add(key, value);
        }

        public bool ContainsKey(string key)
        {
            return _dic.ContainsKey(key);
        }

        public bool Remove(string key)
        {
            return _dic.Remove(key);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            return _dic.TryGetValue(key, out value);
        }

        public void Add(KeyValuePair<string, object> item)
        {
            _dic.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            _dic.Clear();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return _dic.Contains(item);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _dic.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

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
}
