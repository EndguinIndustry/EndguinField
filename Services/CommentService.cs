using EndguinField.Models;
using EndguinField.Services.MongoDB;
namespace EndguinField.Services;

public interface ICommentService
{
	List<Comment> GetAllComments();
	Comment? GetCommentById(string commentId);
	void CreateComment(Comment comment);
	void UpdateComment(string commentId, Comment comment);
	void DeleteComment(string commentId);
}

public class CommentService : ICommentService
{
	private readonly ICommentMongoService _commentStore;

	public CommentService(ICommentMongoService commentStore)
	{
		_commentStore = commentStore;
	}

	public List<Comment> GetAllComments()
	{
		return _commentStore.GetAllAsync().GetAwaiter().GetResult();
	}

	public Comment? GetCommentById(string id)
	{
		return _commentStore.GetByIdAsync(id).GetAwaiter().GetResult();
	}

	public void CreateComment(Comment comment)
	{
		_commentStore.CreateAsync(comment).GetAwaiter().GetResult();
	}

	public void UpdateComment(string id, Comment comment)
	{
		_commentStore.UpdateAsync(id, comment).GetAwaiter().GetResult();
	}

	public void DeleteComment(string id)
	{
		_commentStore.DeleteAsync(id).GetAwaiter().GetResult();
	}
}
