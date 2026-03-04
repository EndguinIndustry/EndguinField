using EndguinField.Models;
using EndguinField.Services.MongoDB;
namespace EndguinField.Services;

public interface ICommentService
{
	Task<List<Comment>> GetAllCommentsAsync();
	Task<Comment?> GetCommentByIdAsync(string commentId);
	Task CreateCommentAsync(Comment comment);
	Task UpdateCommentAsync(string commentId, Comment comment);
	Task DeleteCommentAsync(string commentId);
	Task DeleteCommentsAsync(IEnumerable<string> commentIds);
}

public class CommentService : ICommentService
{
	private readonly ICommentMongoService _commentStore;

	public CommentService(ICommentMongoService commentStore)
	{
		_commentStore = commentStore;
	}

	public async Task<List<Comment>> GetAllCommentsAsync()
	{
		return await _commentStore.GetAllAsync();
	}

	public async Task<Comment?> GetCommentByIdAsync(string id)
	{
		return await _commentStore.GetByIdAsync(id);
	}

	public async Task CreateCommentAsync(Comment comment)
	{
		if (string.IsNullOrWhiteSpace(comment.Content))
		{
			throw new ArgumentException("Comment cannot be empty.");
		}
		if (comment.Content.Length > 1000)
		{
			throw new ArgumentException("Comment cannot exceed 1000 characters.");
		}
		await _commentStore.CreateAsync(comment);
	}

	public async Task UpdateCommentAsync(string id, Comment comment)
	{
		await _commentStore.UpdateAsync(id, comment);
	}

	public async Task DeleteCommentAsync(string id)
	{
		await _commentStore.DeleteAsync(id);
	}

	public async Task DeleteCommentsAsync(IEnumerable<string> commentIds)
	{
		var tasks = commentIds.Select(id => _commentStore.DeleteAsync(id));
		await Task.WhenAll(tasks);
	}
}
