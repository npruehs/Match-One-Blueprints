using System.Collections.Generic;
using System.Linq;

using Entitas;
using Entitas.Blueprints;

using UnityEngine;

public static class PoolExtensions
{
    public static Entity CreateRandomPiece(this Pool pool, int x, int y)
    {
        var pieceBlueprintId = PieceBlueprintIds[Random.Range(0, PieceBlueprintIds.Count)];
        var blueprint = BlueprintsController.GetBlueprint(pieceBlueprintId);
        return pool.CreateEntity(blueprint).AddPosition(x, y);
    }

    public static Entity CreateBlocker(this Pool pool, int x, int y) {
        var blockerBlueprintId = BlockerBlueprintIds[Random.Range(0, BlockerBlueprintIds.Count)];
        var blueprint = BlueprintsController.GetBlueprint(blockerBlueprintId);
        return pool.CreateEntity(blueprint).AddPosition(x, y);
    }

    private static BlueprintsController blueprintsController;

    private static BlueprintsController BlueprintsController
    {
        get
        {
            // TODO(np): Clearly, this is not a nice solution.
            // Creating pieces should be moved to some system,
            // and that system should be initialized with blueprint data from the GameController.
            if (blueprintsController == null)
            {
                blueprintsController = Object.FindObjectOfType<BlueprintsController>();
            }

            return blueprintsController;
        }
    }

    private static List<string> pieceBlueprintIds;

    private static List<string> PieceBlueprintIds
    {
        get
        {
            if (pieceBlueprintIds == null)
            {
                pieceBlueprintIds =
                    BlueprintsController.GetBlueprints()
                        .Where(IsPieceBlueprint)
                        .Select(blueprint => blueprint.Id)
                        .ToList();
            }

            return pieceBlueprintIds;
        }
    }

    private static List<string> blockerBlueprintIds;

    private static List<string> BlockerBlueprintIds
    {
        get
        {
            if (blockerBlueprintIds == null)
            {
                blockerBlueprintIds =
                    BlueprintsController.GetBlueprints()
                        .Where(IsBlockerBlueprint)
                        .Select(blueprint => blueprint.Id)
                        .ToList();
            }

            return blockerBlueprintIds;
        }
    }

    private static bool IsPieceBlueprint(IBlueprint blueprint)
    {
        return blueprint.ComponentTypes.Contains(ComponentIds.componentNames[ComponentIds.Interactive]);

    }

    private static bool IsBlockerBlueprint(IBlueprint blueprint)
    {
        return !blueprint.ComponentTypes.Contains(ComponentIds.componentNames[ComponentIds.Movable]);
    }
}

