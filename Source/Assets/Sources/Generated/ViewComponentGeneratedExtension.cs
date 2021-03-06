namespace Entitas {
    public partial class Entity {
        public ViewComponent view { get { return (ViewComponent)GetComponent(ComponentIds.View); } }

        public bool hasView { get { return HasComponent(ComponentIds.View); } }

        public Entity AddView(UnityEngine.GameObject newGameObject) {
            var componentPool = GetComponentPool(ComponentIds.View);
            var component = (ViewComponent)(componentPool.Count > 0 ? componentPool.Pop() : new ViewComponent());
            component.gameObject = newGameObject;
            return AddComponent(ComponentIds.View, component);
        }

        public Entity ReplaceView(UnityEngine.GameObject newGameObject) {
            var componentPool = GetComponentPool(ComponentIds.View);
            var component = (ViewComponent)(componentPool.Count > 0 ? componentPool.Pop() : new ViewComponent());
            component.gameObject = newGameObject;
            ReplaceComponent(ComponentIds.View, component);
            return this;
        }

        public Entity RemoveView() {
            return RemoveComponent(ComponentIds.View);;
        }
    }

    public partial class Matcher {
        static IMatcher _matcherView;

        public static IMatcher View {
            get {
                if (_matcherView == null) {
                    var matcher = (Matcher)Matcher.AllOf(ComponentIds.View);
                    matcher.componentNames = ComponentIds.componentNames;
                    _matcherView = matcher;
                }

                return _matcherView;
            }
        }
    }
}
