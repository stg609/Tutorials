$resp = (Invoke-WebRequest -UseBasicParsing -Uri "https://xxx/apis/tenancy/api/app/logicalarrangement/WorkflowInstance/retry" `
        -Method "POST" `
        -Headers @{
        "authority"          = "app.dangquyun.com"
        "method"             = "POST"
        "path"               = "/apis/tenancy/api/app/logicalarrangement/WorkflowInstance/retry"
        "scheme"             = "https"
        "__tenant"           = "3a010d31-750d-5dad-5f3c-8665f3ec8ac0"
        "accept"             = "application/json"
        "authorization"      = "Bearer eyJhbGciOiJSUzI1NiIsImtpZCI6IkRDRTlEQjM2RjZBRkEwNUUyMzlEOUFDNjFCQTcyQ0M4QzEzNDJCNjFSUzI1NiIsInR5cCI6ImF0K2p3dCIsIng1dCI6IjNPbmJOdmF2b0Y0am5ackdHNmNzeU1FMEsyRSJ9.eyJuYmYiOjE2NTI5NDM5MjIsImV4cCI6MTY1Mjk2NTUyMiwiaXNzIjoiaHR0cHM6Ly9hY2NvdW50LmRhbmdxdXl1bi5jb20vdXNlcmlkZW50aXR5IiwiYXVkIjoicHRfYXBpcyIsImNsaWVudF9pZCI6InRlbmFuY3lfanMiLCJzdWIiOiIyNmU2ZDk1NC02NmNhLTQ0YzktYmUzYy00MmJhYjRkMmIwNTEiLCJhdXRoX3RpbWUiOjE2NTI0MzkyNzMsImlkcCI6ImxvY2FsIiwiX190ZW5hbnQiOiIzOWZmODVjMy0zZTljLWUwNDMtZDBlZi0yZWJiZTc0NzQ4YmEiLCJqdGkiOiJFMTFEMDM4N0EwRjdGN0Q5MkQ0RDJEQ0Q1NDhCNTFFNCIsInNpZCI6IkJDMzM4ODI1NjM2NzNDQjdBNEQzRkJGNEM4MjI0OEFCIiwiaWF0IjoxNjUyOTQzOTIyLCJzY29wZSI6WyJvcGVuaWQiLCJ0ZW5hbnRfdXNlciJdLCJhbXIiOlsicHdkIl19.UTqgkkP_L1xoJYVSEgXYidmvJZKRKduOBP604YEUZtdy4YXMzq74nMXk4j6SPM0xpw9Btekz-nEOR0FPDKRz55UPuxmxC_PsDygHuHc0sDm-48BQ1aApZKhWA9Va2rvCljSIo4OJZ3OZolDg99Oj_nhnvF2Bp08m5NWjTpnvGXVbKmZBo2RtXkEzKbXaH06avKxPZvCddhb59sao2ZZI-rDa1QNhaC2HpYLzrX73xdCtr-5Xkc6qoIyiaqPJt8p3Col9RqJ9di58gLmMfB8C7QSTDgtCtqMC_hi3tcC3oknLjctFAZnXupMJVBTHXkUrX1wLodLrZlWnwEddBeKXJw"
        "cache-control"      = "no-cache"
        "origin"             = "https://app.test.powertradepro.com"
        "pragma"             = "no-cache"
        "referer"            = "https://app.dangquyun.com/work-platform/logic-arrange"
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