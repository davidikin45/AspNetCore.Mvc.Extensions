namespace AspNetCore.Mvc.Extensions.Email
{
    public class EmailMessage
    {
        public string ReplyDisplayName { get; set; }
        public string ReplyEmail { get; set; }
        public string ToDisplayName { get; set; }
        public string ToEmail { get; set; }
        public string Subject { get; set; }
        public string PlainBody { get; set; }
        public string HtmlBody { get; set; }
    }
}
