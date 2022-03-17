# obj to json string
$obj = @{ query = @{match_all=@{}}};
Write-Output  $obj | ConvertTo-Json -Depth 10



# es 查询
$esQuery = "{
    ""query"": {
        ""match_all"": {}
    }
}"

$elasticUri = 'http://192.168.123.154:9200/test4/_search?pretty'
$username = "admin"
$password = "bestadmin"
$base64AuthInfo = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(("{0}:{1}" -f $username,$password)))

Invoke-Restmethod -Headers @{Authorization=("Basic {0}" -f $base64AuthInfo)} -uri $elasticUri -method POST -Body $elasticQuery

# es create document
$esPost = "{
    ""dataId"": ""testId"",
    ""data"": {
        ""dataId"" : ""test1d"",
        ""modifiedRecord"" : [
              {
                ""fieldWholeName"" : ""test.whole.name"",
                ""operRecord"" : [
                  {
                    ""formWholeName"" : ""this is你好 form whole name"",
                    ""oldValue"" : ""this is old value of f2"",
                    ""currentFormOperationType"" : 0,
                    ""field"" :""f2"",
                    ""newValue"" : "" this is new value of f2""
                  }
                ]
              }
            ]

    }
}"

$elasticUri = 'http://192.168.123.154:9200/test4/_doc'
Invoke-Restmethod -Headers @{Authorization=("Basic {0}" -f $base64AuthInfo)} -uri $elasticUri -ContentType 'application/json;charset=utf-8' -method POST -Body $esPost


# es delete document
$esDelete = "{
    ""query"": {
        ""match"": {
          ""dataId"": ""606e5e87b86e3e1ffba5c793""
        }
    }
}"

$elasticUri = 'http://192.168.123.154:9201/low_code_data_change_record_dev/_delete_by_query'
Invoke-Restmethod -Headers @{Authorization=("Basic {0}" -f $base64AuthInfo)} -uri $elasticUri -ContentType 'application/json;charset=utf-8' -method POST -Body $esDelete