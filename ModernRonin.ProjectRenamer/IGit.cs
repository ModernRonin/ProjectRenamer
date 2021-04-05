namespace ModernRonin.ProjectRenamer
{
    public interface IGit
    {
        void Commit(string msg);
        void EnsureIsClean();
        string GetVersion();
        void Move(string oldDir, string newDir);
        void RollbackAllChanges();
        void StageAllChanges();
    }
}