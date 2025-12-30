namespace ScenesInBuildEditor
{
    /// <summary>
    /// Data model representing a scene in the project.
    /// </summary>
    internal class SceneEntry
    {
        public string Path;
        public string Name;
        public bool IsInBuild;
        public int BuildIndex;
    }
}
