namespace eGandalf.Epi.PagePreview
{
    /// <summary>
    /// Allows all site visitors to preview, so long as Read and Change permissions are set on anonymous and URL is known
    /// </summary>
    public class AnonymousPagePreview : IPagePreview
    {
        public bool IsAllowed() => true;
    }
}