using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Informatique.Alumni.Magazine;

public interface IMagazineAppService : IApplicationService
{
    Task<MagazineIssueDto> CreateIssueAsync(CreateUpdateMagazineIssueDto input);
    Task<PagedResultDto<MagazineIssueDto>> GetIssuesAsync(PagedAndSortedResultRequestDto input);
    Task DeleteIssueAsync(Guid id);
    
    // Graduate Portal Methods
    Task<PagedResultDto<MagazineListDto>> GetListAsync(GetMagazinesInput input);
    Task<Volo.Abp.Content.IRemoteStreamContent> DownloadAsync(Guid id);
}

public interface IBlogAppService : IApplicationService
{
    Task<BlogPostDto> CreatePostAsync(CreateUpdateBlogPostDto input);
    Task<BlogPostDto> UpdatePostAsync(Guid id, CreateUpdateBlogPostDto input);
    Task<BlogPostDto> GetAsync(Guid id);
    Task<PagedResultDto<BlogPostDto>> GetListAsync(PostSearchInputDto input);
    Task DeletePostAsync(Guid id);

    Task<PostCommentDto> AddCommentAsync(Guid postId, CreatePostCommentDto input);
    Task<PagedResultDto<PostCommentDto>> GetCommentsAsync(Guid postId, PagedResultRequestDto input);
    Task ApproveCommentAsync(Guid postId, Guid commentId);
    Task DeleteCommentAsync(Guid postId, Guid commentId);
}

public class PostSearchInputDto : PagedAndSortedResultRequestDto
{
    public string? Category { get; set; }
    public string? Keyword { get; set; }
    public bool? IsFeatured { get; set; }
    public string? Tag { get; set; }
    public DateTime? MinDate { get; set; }
    public DateTime? MaxDate { get; set; }
}
