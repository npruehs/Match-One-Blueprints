﻿using System;
using System.Collections.Generic;

namespace Entitas {

    /// A pool manages the lifecycle of entities and groups.
    /// You can create and destroy entities and get groups of entities.
    /// The prefered way is to use the generated methods from the code generator to create a Pool, e.g. var pool = Pools.pool;
    public partial class Pool {

        /// Occurs when an entity gets created.
        public event PoolChanged OnEntityCreated;

        /// Occurs when an entity will be destroyed.
        public event PoolChanged OnEntityWillBeDestroyed;

        /// Occurs when an entity got destroyed.
        public event PoolChanged OnEntityDestroyed;

        /// Occurs when a group gets created for the first time.
        public event GroupChanged OnGroupCreated;

        /// Occurs when a group gets cleared.
        public event GroupChanged OnGroupCleared;

        public delegate void PoolChanged(Pool pool, Entity entity);
        public delegate void GroupChanged(Pool pool, Group group);

        /// Returns the sum of components that can be used in this pool.
        /// This value is generated by the code generator.
        public int totalComponents { get { return _totalComponents; } }

        /// Returns all componentPools. componentPools is used to reuse removed components.
        /// The componentPools are managed by the generated methods from the code generator.
        public Stack<IComponent>[] componentPools { get { return _componentPools; } }

        /// The metaData contains information about the pool.
        /// It's used to provide better error messages.
        public PoolMetaData metaData { get { return _metaData; } }

        /// Returns the number of entities in the pool.
        public int count { get { return _entities.Count; } }

        /// Returns the number of entities in the internal ObjectPool for entities which can be reused.
        public int reusableEntitiesCount { get { return _reusableEntities.Count; } }

        /// Returns the number of entities that are currently retained by other objects (e.g. Group, GroupObserver, ReactiveSystem).
        public int retainedEntitiesCount { get { return _retainedEntities.Count; } }

        protected readonly HashSet<Entity> _entities = new HashSet<Entity>(EntityEqualityComparer.comparer);
        protected readonly Dictionary<IMatcher, Group> _groups = new Dictionary<IMatcher, Group>();
        protected readonly List<Group>[] _groupsForIndex;
        readonly Stack<Entity> _reusableEntities = new Stack<Entity>();
        readonly HashSet<Entity> _retainedEntities = new HashSet<Entity>();

        readonly int _totalComponents;
        readonly Stack<IComponent>[] _componentPools;
        int _creationIndex;
        readonly PoolMetaData _metaData;

        Entity[] _entitiesCache;

        // Cache delegates to avoid gc allocations
        Entity.EntityChanged _cachedUpdateGroupsComponentAddedOrRemoved;
        Entity.ComponentReplaced _cachedUpdateGroupsComponentReplaced;
        Entity.EntityReleased _cachedOnEntityReleased;

        /// The prefered way is to use the generated methods from the code generator to create a Pool, e.g. var pool = Pools.pool;
        public Pool(int totalComponents) : this(totalComponents, 0, null) {
        }

        /// The prefered way is to use the generated methods from the code generator to create a Pool, e.g. var pool = Pools.pool;
        public Pool(int totalComponents, int startCreationIndex, PoolMetaData metaData) {
            _totalComponents = totalComponents;
            _componentPools = new Stack<IComponent>[totalComponents];
            _creationIndex = startCreationIndex;

            if (metaData != null) {
                _metaData = metaData;

                if (metaData.componentNames.Length != totalComponents) {
                    throw new PoolMetaDataException(this, metaData);
                }
            } else {
                var componentNames = new string[totalComponents];
                const string prefix = "Index ";
                for (int i = 0, componentNamesLength = componentNames.Length; i < componentNamesLength; i++) {
                    componentNames[i] = prefix + i;
                }
                _metaData = new PoolMetaData("Unnamed Pool", componentNames);
            }

            _groupsForIndex = new List<Group>[totalComponents];

            // Cache delegates to avoid gc allocations
            _cachedUpdateGroupsComponentAddedOrRemoved = updateGroupsComponentAddedOrRemoved;
            _cachedUpdateGroupsComponentReplaced = updateGroupsComponentReplaced;
            _cachedOnEntityReleased = onEntityReleased;
        }

        /// Creates a new entity or gets a reusable entity from the internal ObjectPool for entities.
        public virtual Entity CreateEntity() {
            var entity = _reusableEntities.Count > 0 ? _reusableEntities.Pop() : new Entity(_totalComponents, _componentPools, _metaData);
            entity._isEnabled = true;
            entity._creationIndex = _creationIndex++;
            entity.Retain(this);
            _entities.Add(entity);
            _entitiesCache = null;
            entity.OnComponentAdded += _cachedUpdateGroupsComponentAddedOrRemoved;
            entity.OnComponentRemoved += _cachedUpdateGroupsComponentAddedOrRemoved;
            entity.OnComponentReplaced += _cachedUpdateGroupsComponentReplaced;
            entity.OnEntityReleased += _cachedOnEntityReleased;

            if (OnEntityCreated != null) {
                OnEntityCreated(this, entity);
            }

            return entity;
        }

        /// Destroys the entity, removes all its components and pushs it back to the internal ObjectPool for entities.
        public virtual void DestroyEntity(Entity entity) {
            var removed = _entities.Remove(entity);
            if (!removed) {
                throw new PoolDoesNotContainEntityException("'" + this + "' cannot destroy " + entity + "!",
                    "Did you call pool.DestroyEntity() on a wrong pool?");
            }
            _entitiesCache = null;

            if (OnEntityWillBeDestroyed != null) {
                OnEntityWillBeDestroyed(this, entity);
            }

            entity.destroy();

            if (OnEntityDestroyed != null) {
                OnEntityDestroyed(this, entity);
            }

            if (entity.retainCount == 1) {
                entity.OnEntityReleased -= _cachedOnEntityReleased;
                _reusableEntities.Push(entity);
            } else {
                _retainedEntities.Add(entity);
            }
            entity.Release(this);
        }

        /// Destroys all entities in the pool.
        public virtual void DestroyAllEntities() {
            var entities = GetEntities();
            for (int i = 0, entitiesLength = entities.Length; i < entitiesLength; i++) {
                DestroyEntity(entities[i]);
            }

            _entities.Clear();

            if (_retainedEntities.Count != 0) {
                throw new PoolStillHasRetainedEntitiesException(this);
            }
        }

        /// Determines whether the pool has the specified entity.
        public virtual bool HasEntity(Entity entity) {
            return _entities.Contains(entity);
        }

        /// Returns all entities which are currently in the pool.
        public virtual Entity[] GetEntities() {
            if (_entitiesCache == null) {
                _entitiesCache = new Entity[_entities.Count];
                _entities.CopyTo(_entitiesCache);
            }

            return _entitiesCache;
        }

        /// Returns a group for the specified matcher.
        /// Calling pool.GetGroup(matcher) with the same matcher will always return the same instance of the group.
        public virtual Group GetGroup(IMatcher matcher) {
            Group group;
            if (!_groups.TryGetValue(matcher, out group)) {
                group = new Group(matcher);
                var entities = GetEntities();
                for (int i = 0, entitiesLength = entities.Length; i < entitiesLength; i++) {
                    group.HandleEntitySilently(entities[i]);
                }
                _groups.Add(matcher, group);

                for (int i = 0, indicesLength = matcher.indices.Length; i < indicesLength; i++) {
                    var index = matcher.indices[i];
                    if (_groupsForIndex[index] == null) {
                        _groupsForIndex[index] = new List<Group>();
                    }
                    _groupsForIndex[index].Add(group);
                }

                if (OnGroupCreated != null) {
                    OnGroupCreated(this, group);
                }
            }

            return group;
        }

        /// Clears all groups. This is useful when you want to soft-restart your application.
        public void ClearGroups() {
            foreach (var group in _groups.Values) {
                group.RemoveAllEventHandlers();
                for (int i = 0, n = group.GetEntities().Length; i < n; i++) {
                    group.GetEntities()[i].Release(group);
                }

                if (OnGroupCleared != null) {
                    OnGroupCleared(this, group);
                }
            }
            _groups.Clear();

            for (int i = 0, groupsForIndexLength = _groupsForIndex.Length; i < groupsForIndexLength; i++) {
                _groupsForIndex[i] = null;
            }
        }

        /// Resets the creationIndex back to 0.
        public void ResetCreationIndex() {
            _creationIndex = 0;
        }

        /// Clears the componentPool at the specified index.
        public void ClearComponentPool(int index) {
            var componentPool = _componentPools[index];
            if (componentPool != null) {
                componentPool.Clear();
            }
        }

        /// Clears all componentPools.
        public void ClearComponentPools() {
            for (int i = 0, componentPoolsLength = _componentPools.Length; i < componentPoolsLength; i++) {
                ClearComponentPool(i);
            }
        }

        /// Resets the pool (clears all groups, destroys all entities and resets creationIndex back to 0).
        public void Reset() {
            ClearGroups();
            DestroyAllEntities();
            ResetCreationIndex();
        }

        public override string ToString() {
            return _metaData.poolName;
        }

        protected void updateGroupsComponentAddedOrRemoved(Entity entity, int index, IComponent component) {
            var groups = _groupsForIndex[index];
            if (groups != null) {
                var events = new List<Group.GroupChanged>(groups.Count);
                for (int i = 0, groupsCount = groups.Count; i < groupsCount; i++) {
                    events.Add(groups[i].handleEntity(entity));
                }
                for (int i = 0, eventsCount = events.Count; i < eventsCount; i++) {
                    var groupChangedEvent = events[i];
                    if (groupChangedEvent != null) {
                        groupChangedEvent(groups[i], entity, index, component);
                    }
                }
            }
        }

        protected void updateGroupsComponentReplaced(Entity entity, int index, IComponent previousComponent, IComponent newComponent) {
            var groups = _groupsForIndex[index];
            if (groups != null) {
                for (int i = 0, groupsCount = groups.Count; i < groupsCount; i++) {
                    groups[i].UpdateEntity(entity, index, previousComponent, newComponent);
                }
            }
        }

        protected void onEntityReleased(Entity entity) {
            if (entity._isEnabled) {
                throw new EntityIsNotDestroyedException("Cannot release entity!");
            }
            entity.OnEntityReleased -= _cachedOnEntityReleased;
            _retainedEntities.Remove(entity);
            _reusableEntities.Push(entity);
        }
    }

    public class PoolDoesNotContainEntityException : EntitasException {
        public PoolDoesNotContainEntityException(string message, string hint) :
        base(message + "\nPool does not contain entity!", hint) {
        }
    }

    public class EntityIsNotDestroyedException : EntitasException {
        public EntityIsNotDestroyedException(string message) :
        base(message + "\nEntity is not destroyed yet!",
            "Did you manually call entity.Release(pool) yourself? If so, please don't :)") {
        }
    }

    public class PoolStillHasRetainedEntitiesException : EntitasException {
        public PoolStillHasRetainedEntitiesException(Pool pool) :
        base("'" + pool + "' detected retained entities although all entities got destroyed!",
            "Did you release all entities? Try calling pool.ClearGroups() and systems.ClearReactiveSystems() before calling pool.DestroyAllEntities() to avoid memory leaks.") {
        }
    }

    public class PoolMetaDataException : EntitasException {
        public PoolMetaDataException(Pool pool, PoolMetaData poolMetaData) :
        base("Invalid PoolMetaData for '" + pool + "'!\nExpected " + pool.totalComponents + " componentName(s) but got " + poolMetaData.componentNames.Length + ":",
            string.Join("\n", poolMetaData.componentNames)) {
        }
    }

    public class PoolMetaData {

        public string poolName { get { return _poolName; } }
        public string[] componentNames { get { return _componentNames; } }

        readonly string _poolName;
        readonly string[] _componentNames;

        public PoolMetaData(string poolName, string[] componentNames) {
            _poolName = poolName;
            _componentNames = componentNames;
        }
    }
}
