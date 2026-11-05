namespace SmartRoutine.UI.Helpers
{
    public static class AppSettings
    {
        /// <summary>
        /// true = Testdata, false = real data. Debug builds use test data so that testing never
        /// touches the real database; Release builds always use the real data.
        /// </summary>
#if DEBUG
        public static bool UseTestData { get; set; } = true;
#else
        public static bool UseTestData { get; set; } = false;
#endif
    }
}
