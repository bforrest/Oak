using System;
using System.Collections.Generic;

namespace Oak.Tests.describe_DynamicModel.describe_Association.Classes
{
    public class Blog : DynamicModel
    {
        Comments comments;

        public Blog()
            : this(new { })
        {

        }

        public Blog(object entity)
            : base(entity)
        {
            comments = new Comments();
        }

        public IEnumerable<dynamic> Associates()
        {
            yield return 
            new HasMany(comments);

            yield return
            new HasMany(comments, "Roses"); //a rose by any other name
        }
    }
}