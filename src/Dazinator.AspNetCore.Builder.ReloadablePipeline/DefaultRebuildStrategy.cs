namespace Dazinator.AspNetCore.Builder.ReloadablePipeline
{
    public static class DefaultRebuildStrategy
    {
        /// <summary>
        /// The default rebuild strategy.
        /// </summary>
        public static IRebuildStrategy Create() => new RebuildOnInvalidateStrategy();
    }

}