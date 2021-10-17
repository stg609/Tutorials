using System;

namespace MessageContracts
{
    public interface Demo2MsgA
    {
        // 如果使用 Guid，且名字叫 CorrelationId 则 MassTransit 会自动把该属性作为 CorrealtionId 进行传递
        Guid CorrelationId { get; }

        string Value { get; }
    }

    public interface Demo2MsgB
    {
        string Value { get; }
    }
}