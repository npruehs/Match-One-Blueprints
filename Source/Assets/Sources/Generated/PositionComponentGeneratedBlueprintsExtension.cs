//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Entitas.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
namespace Entitas
{
    public partial class Entity
    {
        public Entity AddPosition(System.Collections.Generic.IDictionary<string, object> properties)
        {
            var componentPool = this.GetComponentPool(ComponentIds.Position);
            var component = (PositionComponent)(componentPool.Count > 0 ? componentPool.Pop() : new PositionComponent());
            component.x = (int)properties["Position.x"];
            component.y = (int)properties["Position.y"];
            return this.AddComponent(ComponentIds.Position, component);
        }
    }
}