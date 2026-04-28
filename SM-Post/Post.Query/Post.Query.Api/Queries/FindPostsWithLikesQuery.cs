using CQRS.Core.Queries;

namespace Post.Query.Api.Queries
{
    public class FindPostsWithLikesQuery : BaseQuery     // Query para buscar todos los post con cierto numero de likes
    {
        public int NumberOfLikes { get; set; }
    }
}
