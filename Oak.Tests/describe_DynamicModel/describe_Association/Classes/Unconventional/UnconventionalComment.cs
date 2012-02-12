using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oak.Tests.describe_DynamicModel.describe_Association.Classes
{
    public class UnconventionalComment : DynamicModel
    {
        Blogs blogs;

        public UnconventionalComment(object dto)
            : base(dto)
        {
            blogs = new Blogs();
        }

        public IEnumerable<dynamic> Associates()
        {
            yield return
                new BelongsTo(blogs) { ForeignKey = "fkBlogId" };
        }
    }
}
