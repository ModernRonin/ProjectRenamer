namespace ModernRonin.ProjectRenamer
{
    public interface IGit
    {
        void Commit(string msg);
        void EnsureIsClean();
        string GetVersion();
        void Move(string oldPath, string newPath);
        void RollbackAllChanges();
        void StageAllChanges();
    }
}