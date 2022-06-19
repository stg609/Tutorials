由于configmap 挂在是使用了 symbolic link 的方式，这种方式优点类似于快捷方式，实际文件并不在 k8s 目录中。.net 原先的 file watch 主要是根据文件最后修改时间，
    但是 symlink 的最后修改时间并没有改变，改变的是文件内容。所以 .net 6 之前，cm 改了但是 .net 6 中的应用无法感知到。

这个问题在 .net 6 已经针对 symlink 做了bug fix。

0. 必须是.Net 6(因为有一个关于 symbolic link 的修复 https://github.com/dotnet/runtime/issues/36091)
1. program.cs 中增加对额外配置文件
builder.Configuration
    .AddJsonFile("k8s/appsettings.k8s.json", optional: true, reloadOnChange: true);

2. deployment.yml 中使用 configmap 作为 mount 方式挂载到 k8s 目录
          volumeMounts:
            - name: configmap-demo-volume
              mountPath: /app/k8s/

    注意，不要使用 subpath，否则 k8s 将不会自动reload (A container using a ConfigMap as a subPath volume mount will not receive ConfigMap updates.)

3. deployment.yml 中增加一个env
          env:
          - name: DOTNET_USE_POLLING_FILE_WATCHER
            value: "true"
    
    对于 symlink 的文件必须使用 poll 的方式才能监测到，如果不使用这种方式，那么仍然无法知道配置文件的变化。但是因为是 polling，所以变更一般不会立刻知道，需要稍微等几秒。本机测试大概3秒。

----------------------
如何使用这个 demo
1. 在AspNetCore\Sample\ConfigMapDemo 这个目录执行  docker build -t stg609/configmap-demo:v0.0.2 . -f .\ConfigMapDemo\Dockerfile, 然后 docker push stg609/configmap-demo:v0.0.2
2. minikube start (可选，直接使用k8s）
3. kubectl apply -f configmap.yml
4. kubectl apply -f deployment.yml
5. 等待 pod running 后，执行 kubectl port-forward configmap-demo-6687d8c856-mzlmn 5000:80 
6. 浏览器中访问 http://localhost:5000/hello
7. 修改 configmap.yml 并 apply
8. 刷新浏览器，大概等几秒后，会发现返回的内容变了。
    
