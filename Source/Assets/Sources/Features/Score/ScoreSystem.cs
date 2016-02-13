using System.Collections.Generic;
using System.Linq;

using Entitas;
using UnityEngine;

public class ScoreSystem : IInitializeSystem, ISetPool {
    Pool _pool;

    public void SetPool(Pool pool) {
        _pool = pool;

        _pool.OnEntityWillBeDestroyed += this.OnEntityWillBeDestroyed;
    }

    public void Initialize() {
        _pool.SetScore(0);
    }

    private void OnEntityWillBeDestroyed(Pool pool, Entity entity)
    {
        if (!entity.hasScoreValue)
        {
            return;
        }

        _pool.ReplaceScore(_pool.score.value + entity.scoreValue.value);
    }
}

