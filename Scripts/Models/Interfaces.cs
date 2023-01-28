using System;

public interface INameable: IEquatable<INameable>
{
    string Name { get; }
}
