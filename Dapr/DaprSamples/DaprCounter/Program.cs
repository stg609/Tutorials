// 演示 self-hosted 模式中的第一个 Dapr 应用程序
// https://docs.microsoft.com/en-us/dotnet/architecture/dapr-for-net-developers/getting-started#build-your-first-dapr-application

using Dapr.Client;

const string storeName = "statestore";
const string key = "counter";

// 通过 daprClient 可以与 dapr sidecar 进行交互
var daprClient = new DaprClientBuilder().Build(); 

/*
获取 key 这个 state，如果不在，则返回默认的 state, 对于 int 而言就是 0
这个示例会存储到 dapr init 时默认提供的 redis contianer ，但是这里并没有显示的与 redis 有任何依赖。
默认的 statestore Component 的配置文件：%USERPROFILE%\.dapr\components 
*/
var counter = await daprClient.GetStateAsync<int>(storeName, key); // 这里 storeName = statestore 告诉了 Dapr sidecar 应该使用哪个 component

/*
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: statestore 
spec:
  type: state.redis // 描述了component type，
  version: v1
  metadata:  // 描述了这个 component 需要的一些信息
  - name: redisHost
    value: localhost:6379
  - name: redisPassword
    value: ""
  - name: actorStateStore
    value: "true"


如果我们想限制只有特定的环境才可以使用某个 compnent，或者只有特定的应用可以使用，那么可以加上 namespace, scope：
下面表示只有 production 的 DaprCounter 可以使用 statestore 这个 redis 的组件

apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: statestore
  namespace: production  // 在 self-hosted 场景，会以 NAMESPACE 这个环境变量来判断，在 k8s 中，则会根据 k8s 的 namespace 来判断，一样才能使用。
spec:
  type: state.redis
  version: v1
  metadata:
  - name: redisHost
    value: localhost:6379
  - name: redisPassword
    value: ""
  - name: actorStateStore
    value: "true"
  scopes:
  - DaprCounter  // 只有 DaprCounter 才可以是用
*/

while (true)
{
    Console.WriteLine($"Counter = {counter++}");

    await daprClient.SaveStateAsync(storeName, key, counter);
    await Task.Delay(1000);
}

// 通过执行 dapr run --app-id DaprCounter dotnet run 来执行，
// 其中 --app-id 是必须的，这个会作为 state store key 的前缀。
// dapr run 会调用底层的 Dapr 运行时，允许 应用程序 和 Dapr sidecar 同时运行，
// 最后的 dotnet run 会告诉 Dapr runtime 运行 .net 应用程序。