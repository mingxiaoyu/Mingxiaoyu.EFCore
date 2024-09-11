namespace Mingxiaoyu.EFCore.SoftDelete
{
    public interface ISoftDeletable
    {
        bool IsDeleted { get; set; }
    }
}
