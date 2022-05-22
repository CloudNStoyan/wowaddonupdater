namespace AddonUpdaterAlpha
{
    public static class CustomUtils
    {
        public static string GetFileName(string fileUrl) => fileUrl.Split('/')[fileUrl.Split('/').Length - 1];
    }
}
