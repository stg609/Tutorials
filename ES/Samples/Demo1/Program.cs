using System;
using System.Collections.Generic;
using System.Linq;
using Elasticsearch.Net;
using Nest;
using Nest.JsonNetSerializer;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Demo1
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var settings = new ConnectionSettings(
               connectionPool: new SingleNodeConnectionPool(new Uri("http://admin:bestadmin@192.168.123.154:9200")),
               sourceSerializer: (builtin, settings) => new JsonNetSerializer(
                   builtin,
                   settings,
                   () => new JsonSerializerSettings
                   {
                       NullValueHandling = NullValueHandling.Ignore,
                   },
                   contractJsonConverters: new[] { new StringEnumConverter() }))
               .DefaultIndex("test1")
               .DisableDirectStreaming();

            var esClient = new ElasticClient(settings);

            // 创建索引
            var createIndexResponse = esClient.Indices.Create("test1", c => c.Settings(se => se.NumberOfShards(1)
            .NumberOfReplicas(0))
            .Map<SomeData>(d => d
                .AutoMap()
            ));

            #region 反注释下面的代码以添加测试数据

            //var coll = new List<SomeData>
            //{
            //    new SomeData
            //    {
            //        AppId = "3a010d36-e3d1-2bcf-0152-af7fddb3f761",
            //        DataId = "621364bbbd682bb7830ad31f"
            //    },
            //    new SomeData
            //{
            //    AppId = "3a010d36-e3d1-2bcf-0152-af7fddb3f761",
            //    DataId = "621364bbbd682bb7830ad31f",
            //    Coll = new List<InnerData>
            //        {
            //            new InnerData
            //            {
            //                Tag = "Some tag-abc",
            //                Value = "xxx yyy xxx ,dsafdf af afsf"
            //            },
            //            new InnerData
            //            {
            //                Tag = "Some tag1-abc",
            //                Value = "xxx1 yyy xxx ,dsafdf af afsf"
            //            },
            //            new InnerData
            //            {
            //                Tag = "Some tag2",
            //                Value = "xxx1 yyy xxx ,dsafdf af afsf"
            //            }
            //        }
            //},
            //new SomeData
            //    {
            //        AppId = "3a010d36-e3d1-2bcf-0152-xxxxx",
            //        DataId = "xxxx1",
            //        Coll = new List<InnerData>
            //        {
            //            new InnerData
            //            {
            //                Tag = "Some tag2-abc",
            //                Value = "xxx2 yyy xxx ,dsafdf af afsf"
            //            },
            //            new InnerData
            //            {
            //                Tag = "Some tag3",
            //                Value = "xxx12 yyy xxx ,dsafdf af afsf"
            //            },
            //            new InnerData
            //            {
            //                Tag = "Some tag4",
            //                Value = "xxx13 yyy xxx ,dsafdf af afsf"
            //            }
            //        }
            //    }
            //};
            //foreach (var itm in coll)
            //{
            //    var resp = esClient.IndexDocument<SomeData>(itm);
            //}
            #endregion

            /* 通配符模糊搜索，* 代表匹配0个或多个
             * GET /test1/_search
                {
                  "query": {
                    "wildcard": {
                      "dataId": "621364bbbd682bb7830ad31f"
                    }
                  }
                }
             */
            var rslts = esClient.Search<SomeData>(S => S.Query(q => q.Wildcard(m => m.DataId, "*621364bbbd682bb7830ad31f")));

            /* 正则匹配
             * GET /test1/_search
                {
                  "query": {
                    "regexp": {
                      "dataId": "[0-9]+bbbd682bb7830ad31f"
                    }
                  }
                }
             */
            rslts = esClient.Search<SomeData>(S => S.Query(q => q.Regexp(m => m.Field(i => i.DataId).Value("[0-9]+bbbd682bb7830ad31f"))));

            /*
             * 搜索 Nested
             * GET /test1/_search
            {
              "query": {
                "nested": {
                  "path": "coll",
                  "query": {
                    "wildcard": {
                      "coll.tag": {
                        "value": "*-abc"
                      }
                    }
                  }
                }
              }
            }
             */
            rslts = esClient.Search<SomeData>(S => S.Query(q => q.Nested(m => m.Path(p => p.Coll).Query(nq => nq.Wildcard(nm => nm.Coll.First().Tag, "*-abc")))));
        }
    }

    public class SomeData
    {
        [Keyword]
        public string DataId { get; set; }

        [Keyword]
        public string AppId { get; set; }

        [Nested]
        public List<InnerData> Coll { get; set; }
    }

    public class InnerData
    {
        public string Value { get; set; }

        [Keyword]
        public string Tag { get; set; }
    }
}
