
namespace EquipmentBookingSystem1.Domain;

public class EquipmentItem
{
    public Guid Id { get; }
    public Guid ModelId { get; }

    public EquipmentItem(Guid id, Guid modelId)
    {
        Id = id;
        ModelId = modelId;
    }
}
