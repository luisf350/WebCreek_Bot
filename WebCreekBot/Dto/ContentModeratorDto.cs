using System.Collections.Generic;

namespace WebCreekBot.Dto
{
    public class ContentModeratorDto
    {
        public string OriginalText { get; set; }

        public string Language { get; set; }

        public List<TermsDto> Terms { get; set; }
    }

    public class TermsDto
    {
        public string Term { get; set; }
    }
}