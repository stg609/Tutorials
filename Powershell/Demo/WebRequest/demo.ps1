$resp = (Invoke-WebRequest -UseBasicParsing -Uri "https://xxx/apis/tenancy/api/app/logicalarrangement/WorkflowInstance/retry" `
        -Method "POST" `
        -Headers @{
        "authority"          = "xxx"
        "method"             = "POST"
        "path"               = "/apis/tenancy/api/app/logicalarrangement/WorkflowInstance/retry"
        "scheme"             = "https"
        "__tenant"           = "3a010d31-750d-5dad-5f3c-8665f3ec8ac0"
        "accept"             = "application/json"
        "authorization"      = "Bearer xxx"
        "cache-control"      = "no-cache"
        "origin"             = "https://app.test.powertradepro.com"
        "pragma"             = "no-cache"
        "referer"            = "https://xxx/work-platform/logic-arrange"
        "sec-ch-ua"          = "`" Not A;Brand`";v=`"99`", `"Chromium`";v=`"101`", `"Google Chrome`";v=`"101`""
        "sec-ch-ua-mobile"   = "?0"
        "sec-ch-ua-platform" = "`"Windows`""
        "sec-fetch-dest"     = "empty"
        "sec-fetch-mode"     = "cors"
        "sec-fetch-site"     = "same-origin"
    } `
        -ContentType "application/json;charset=UTF-8" `
        -Body "{`"instanceId`":`"xxx`", `"isSync`":true}")

#转成 json 对象
$respObj = $resp.Content | ConvertFrom-Json
if ($respObj.status -ne 1) {    
    Write-Warning "Failed"    
}