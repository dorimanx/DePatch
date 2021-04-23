
namespace DePatch.CoolDown
{

    public class EntityIdCooldownKey : ICooldownKey
    {

        private long EntityId { get; }

        public EntityIdCooldownKey(long EntityId)
        {
            this.EntityId = EntityId;
        }

        public override bool Equals(object obj)
        {
            return obj is EntityIdCooldownKey key &&
                   EntityId == key.EntityId;
        }

        public override int GetHashCode()
        {
            return -1619204625 + EntityId.GetHashCode();
        }
    }
}
