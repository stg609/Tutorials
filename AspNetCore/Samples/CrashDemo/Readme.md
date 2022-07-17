用于演示内存溢出后，k8s 杀掉进程但是不会触发任何 .net 事件。


演示 server gc / workstation gc 对于回收的现象

1. 打包（可以直接使用 0.0.15）
1.1 docker build -t stg609/crash-demo:v0.0.15 . -f .\CrashDemo\Dockerfile
1.2 docker push stg609/crash-demo:v0.0.15

2. 运行容器
2.1 调整 depployment.yml 中的 DOTNET_gcServer 环境变量 0 表示 workstation gc, 1 表示 server gc
2.2 kubectl apply -f C:\Projects\Github\Tutorials\AspNetCore\Samples\CrashDemo\deployment.yml (CPU 必须大于1，否则不管怎么配置都是 workstation gc) 

3. kubectl port-forward crash-demo-xxxx 5000:80

4. 打开 http://localhost:5000/swagger/index.html 找到 /Crash/OOMByRolsyn/{max}
4.1 如果是 server gc，max 最多30 就会 OOM
    如果是 workstation gc，max 100 也不会挂 （不过也有一定概率会挂）