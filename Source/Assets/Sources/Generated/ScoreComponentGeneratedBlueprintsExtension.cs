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
        public Entity AddScore(System.Collections.Generic.IDictionary<string, object> properties)
        {
            var componentPool = this.GetComponentPool(ComponentIds.Score);
            var component = (ScoreComponent)(componentPool.Count > 0 ? componentPool.Pop() : new ScoreComponent());
            component.value = (int)properties["Score.value"];
            return this.AddComponent(ComponentIds.Score, component);
        }
    }
}