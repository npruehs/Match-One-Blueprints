namespace Entitas {
    public partial class Entity {
        public ScoreValueComponent scoreValue { get { return (ScoreValueComponent)GetComponent(ComponentIds.ScoreValue); } }

        public bool hasScoreValue { get { return HasComponent(ComponentIds.ScoreValue); } }

        public Entity AddScoreValue(int newValue) {
            var componentPool = GetComponentPool(ComponentIds.ScoreValue);
            var component = (ScoreValueComponent)(componentPool.Count > 0 ? componentPool.Pop() : new ScoreValueComponent());
            component.value = newValue;
            return AddComponent(ComponentIds.ScoreValue, component);
        }

        public Entity ReplaceScoreValue(int newValue) {
            var componentPool = GetComponentPool(ComponentIds.ScoreValue);
            var component = (ScoreValueComponent)(componentPool.Count > 0 ? componentPool.Pop() : new ScoreValueComponent());
            component.value = newValue;
            ReplaceComponent(ComponentIds.ScoreValue, component);
            return this;
        }

        public Entity RemoveScoreValue() {
            return RemoveComponent(ComponentIds.ScoreValue);;
        }
    }

    public partial class Matcher {
        static IMatcher _matcherScoreValue;

        public static IMatcher ScoreValue {
            get {
                if (_matcherScoreValue == null) {
                    var matcher = (Matcher)Matcher.AllOf(ComponentIds.ScoreValue);
                    matcher.componentNames = ComponentIds.componentNames;
                    _matcherScoreValue = matcher;
                }

                return _matcherScoreValue;
            }
        }
    }
}
