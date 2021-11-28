using System;

namespace NomaiVR.ReusableBehaviours
{
    public interface IActiveObserver
    {
        bool IsActive { get; }
        event Action OnActivate;
        event Action OnDeactivate;
    }
}
