public interface IPrototype<T>
{
    T ShallowCopy();
    T DeepCopy();
}