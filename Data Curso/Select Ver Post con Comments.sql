--select * from Post

--select * from Comment

select p.PostId,
	p.Author,
	p.Message,
	p.Likes,
	(select count (*) from Comment 
		where Comment.PostId = p.PostId) AS Comments,
	p.DatePosted,
	c.Username,
	c.Comment,
	c.Edited,
	c.CommentDate,
	c.CommentId
	from Post p
	LEFT JOIN Comment c
	ON c.PostId = p.PostId