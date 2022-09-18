using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json.Serialization;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace TrivialDemo.Controllers
{
    public class TextJsonController : Controller
    {
        [HttpPost("DeserializeRef")]
        public IActionResult DeserializeRef()
        {
            List<My> coll = new List<My>();

            My a = new My
            {
                Name = "111"
            };

            coll.Add(a);

            Context c = new Context
            {
                Coll = coll,
                _Item = a
            };

            JsonSerializerOptions options = new()
            {
                // 保存引用
                ReferenceHandler = ReferenceHandler.Preserve,
                WriteIndented = true
            };

            string json = JsonSerializer.Serialize(c, options);
            Context d = JsonSerializer.Deserialize<Context>(json, options);

            // 修改 _item, coll 中对应的 item 也会被修改。
            d._Item.Name = "bbb";

            return Ok();
        }
    }

    public class My
    {
        public string Name { get; set; }
    }

    public class Context
    {
        public List<My> Coll { get; set; }

        public My _Item { get; set; }
    }
}
