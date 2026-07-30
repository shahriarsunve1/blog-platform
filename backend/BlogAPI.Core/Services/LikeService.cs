using BlogAPI.Data.Repositories;
using BlogAPI.Domain.Entities;
using BlogAPI.Domain.Exceptions;

namespace BlogAPI.Core.Services;

/// <summary>
/// Like service implementation. Liking/unliking is idempotent - liking an
/// already-liked post (or unliking one you haven't liked) is a no-op, not an error.
/// </summary>
public class LikeService : ILikeService
{
    private readonly ILikeRepository _likeRepository;
    private readonly IPostRepository _postRepository;

    public LikeService(ILikeRepository likeRepository, IPostRepository postRepository)
    {
        _likeRepository = likeRepository;
        _postRepository = postRepository;
    }

    public async Task<int> LikeAsync(Guid postId, Guid userId)
    {
        var post = await _postRepository.GetByIdAsync(postId);
        if (post == null)
            throw new EntityNotFoundException("Post not found");

        var existing = await _likeRepository.GetByPostAndUserAsync(postId, userId);
        if (existing == null)
        {
            await _likeRepository.AddAsync(new Like { PostId = postId, UserId = userId });
        }

        return await CountAsync(postId);
    }

    public async Task<int> UnlikeAsync(Guid postId, Guid userId)
    {
        var post = await _postRepository.GetByIdAsync(postId);
        if (post == null)
            throw new EntityNotFoundException("Post not found");

        var existing = await _likeRepository.GetByPostAndUserAsync(postId, userId);
        if (existing != null)
        {
            await _likeRepository.DeleteAsync(existing);
        }

        return await CountAsync(postId);
    }

    private async Task<int> CountAsync(Guid postId)
    {
        var post = await _postRepository.GetPostWithDetailsAsync(postId);
        return post?.Likes.Count ?? 0;
    }
}
