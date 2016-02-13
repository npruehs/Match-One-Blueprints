namespace Entitas {
    public partial class Entity {
        public ScoreComponent score { get { return (ScoreComponent)GetComponent(ComponentIds.Score); } }

        public bool hasScore { get { return HasComponent(ComponentIds.Score); } }

        public Entity AddScore(int newValue) {
            var componentPool = GetComponentPool(ComponentIds.Score);
            var component = (ScoreComponent)(componentPool.Count > 0 ? componentPool.Pop() : new ScoreComponent());
            component.value = newValue;
            return AddComponent(ComponentIds.Score, component);
        }

        public Entity ReplaceScore(int newValue) {
            var componentPool = GetComponentPool(ComponentIds.Score);
            var component = (ScoreComponent)(componentPool.Count > 0 ? componentPool.Pop() : new ScoreComponent());
            component.value = newValue;
            ReplaceComponent(ComponentIds.Score, component);
            return this;
        }

        public Entity RemoveScore() {
            return RemoveComponent(ComponentIds.Score);;
        }
    }

    public partial class Pool {
        public Entity scoreEntity { get { return GetGroup(Matcher.Score).GetSingleEntity(); } }

        public ScoreComponent score { get { return scoreEntity.score; } }

        public bool hasScore { get { return scoreEntity != null; } }

        public Entity SetScore(int newValue) {
            if (hasScore) {
                throw new EntitasException("Could not set score!\n" + this + " already has an entity with ScoreComponent!",
                    "You should check if the pool already has a scoreEntity before setting it or use pool.ReplaceScore().");
            }
            var entity = CreateEntity();
            entity.AddScore(newValue);
            return entity;
        }

        public Entity ReplaceScore(int newValue) {
            var entity = scoreEntity;
            if (entity == null) {
                entity = SetScore(newValue);
            } else {
                entity.ReplaceScore(newValue);
            }

            return entity;
        }

        public void RemoveScore() {
            DestroyEntity(scoreEntity);
        }
    }

    public partial class Matcher {
        static IMatcher _matcherScore;

        public static IMatcher Score {
            get {
                if (_matcherScore == null) {
                    var matcher = (Matcher)Matcher.AllOf(ComponentIds.Score);
                    matcher.componentNames = ComponentIds.componentNames;
                    _matcherScore = matcher;
                }

                return _matcherScore;
            }
        }
    }
}
