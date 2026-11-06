namespace EquipmentBookingSystem1.Application;

public class SaveResult
{
    public bool IsSuccess { get; }

    private SaveResult(bool isSuccess)
    {
        IsSuccess = isSuccess;
    }

    public static SaveResult Success()
    {
        return new SaveResult(true);
    }

    public static SaveResult Failure()
    {
        return new SaveResult(false);
    }
}