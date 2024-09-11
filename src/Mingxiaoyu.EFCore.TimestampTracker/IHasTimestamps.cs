namespace Mingxiaoyu.EFCore.TimestampTracker
{
    public interface IHasTimestamps
    {
        DateTime CreatedAt { get; set; }
        string CreatedBy { get; set; }
        DateTime? UpdatedAt { get; set; }
        string UpdatedBy { get; set; }
    }

}
