using Replica.Structures;

namespace Replica.Utils { 
    public interface IInputEvent {
        public void OnInput(ref NetworkInput input);
    }

    public interface IPreUpdate {
        public void PreUpdate();
    }
}
