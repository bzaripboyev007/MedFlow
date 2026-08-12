namespace MedFlow.Domain.Common;

public interface IHaveIdProp<T>
{
    T Id { get; set; }
}
