see https://www.1024sou.com/article/321315.html

1.运行 Server

dapr run --dapr-http-port 3511 --app-port 7272 --app-id backend --app-protocol grpc dotnet .\server\bin\Debug\net6.0\Server.dll  --app-ssl

2.运行 Client

dapr run --dapr-http-port 3501 --app-port 7575 --app-id frontend --app-protocol grpc dotnet .\client\bin\Debug\net6.0\Client.dll --app-ssl