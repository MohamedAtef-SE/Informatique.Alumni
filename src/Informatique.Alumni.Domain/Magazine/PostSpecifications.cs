using System;
using System.Linq;
using Volo.Abp.Specifications;

namespace Informatique.Alumni.Magazine;

public class BlogPostByCategorySpecification : Specification<BlogPost>
{
    public string Category { get; }

    public BlogPostByCategorySpecification(string category)
    {
        Category = category;
    }

    public override System.Linq.Expressions.Expression<Func<BlogPost, bool>> ToExpression()
    {
        return post => post.Category == Category;
    }
}

public class BlogPostByKeywordSpecification : Specification<BlogPost>
{
    public string Keyword { get; }

    public BlogPostByKeywordSpecification(string keyword)
    {
        Keyword = keyword;
    }

    public override System.Linq.Expressions.Expression<Func<BlogPost, bool>> ToExpression()
    {
        return post => post.Title.Contains(Keyword) || post.Content.Contains(Keyword);
    }
}

public class BlogPostByDateRangeSpecification : Specification<BlogPost>
{
    public DateTime? MinDate { get; }
    public DateTime? MaxDate { get; }

    public BlogPostByDateRangeSpecification(DateTime? minDate, DateTime? maxDate)
    {
        MinDate = minDate;
        MaxDate = maxDate;
    }

    public override System.Linq.Expressions.Expression<Func<BlogPost, bool>> ToExpression()
    {
        return post => (!MinDate.HasValue || post.CreationTime >= MinDate.Value) &&
                       (!MaxDate.HasValue || post.CreationTime <= MaxDate.Value);
    }
}
