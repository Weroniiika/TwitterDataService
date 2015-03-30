Create procedure GetHashtagFreq @requestId int
as
Begin
	select distinct h.HashtagId, h.HashtagWord, Count(h.HashtagId) as TweetCount
	from Hashtags h join TweetHashtags th on h.HashtagId = th.Hashtag_HashtagId
	where th.Tweet_RequestId = @requestId
	Group by h.HashtagWord, h.HashtagId
	Order by TweetCount desc
end
Go